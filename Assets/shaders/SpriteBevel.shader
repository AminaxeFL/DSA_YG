Shader "Diesel/SpriteBevel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Спрайт", 2D) = "white" {}
        _MaskTex ("Маска света", 2D) = "white" {}
        _NormalMap ("Карта нормалей", 2D) = "bump" {}
        [MaterialToggle] _ZWrite ("ZWrite", Float) = 0

        _Bevel      ("Сила бевола", Range(0, 2)) = 0.4
        _BevelColor ("Цвет бевола", Color) = (1, 1, 1, 1)
        _BevelThick ("Толщина бевола, текселей", Range(1, 8)) = 1
        _Brightness ("Яркость", Range(0, 4)) = 1
        [Range(0, 1)] _Alpha ("Прозрачность", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Name "SpriteBevelLit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Assets/shaders/SkySun.hlsl"

            #pragma vertex LitVertex
            #pragma fragment LitFragment

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma target 3.0

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                half4 color : COLOR;

                float2 positionWS : TEXCOORD2;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
                float4 _NormalMap_ST;

                float _Bevel;
                half4 _BevelColor;
                float _BevelThick;
                float _Brightness;
                float _Alpha;
            CBUFFER_END

            float4 _MainTex_TexelSize;

            float4 _SkySunAzEl;

            float AlphaAt(float2 p)
            {
                float outside = (p.x < 0.0 || p.y < 0.0 || p.x > 1.0 || p.y > 1.0) ? 1.0 : 0.0;

                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, saturate(p)).a * (1.0 - outside);
            }

            Varyings LitVertex(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonLitVertex(input);
                o.color = input.color;
                o.positionWS = TransformObjectToWorld(input.positionOS).xy;

                return o;
            }

            half4 LitFragment(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                half4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                if (base.a <= 0.0)
                    discard;

                float texel = max(_MainTex_TexelSize.x, 1.0 / 16384.0);
                float thick = max(_BevelThick, 1.0);

                float2 offX = float2(texel * thick, 0.0);
                float2 offY = float2(0.0, texel * thick);

                float aXn0 = AlphaAt(uv - offX);
                float aXp0 = AlphaAt(uv + offX);
                float aYn0 = AlphaAt(uv - offY);
                float aYp0 = AlphaAt(uv + offY);

                float anyEdge = 1.0 - min(min(aXn0, aXp0), min(aYn0, aYp0));
                anyEdge *= base.a;

                float3 rgb = base.rgb * input.color.rgb;

                if (_Bevel > 0.001 && anyEdge > 0.0)
                {
                    float um00 = ddx(uv.x);
                    float um01 = ddy(uv.x);
                    float um10 = ddx(uv.y);
                    float um11 = ddy(uv.y);

                    float udet = um00 * um11 - um01 * um10;

                    if (abs(udet) < 1e-12)
                        udet = udet < 0.0 ? -1e-12 : 1e-12;

                    float uinvDet = 1.0 / udet;

                    float2x2 toScreen = float2x2(
                        um11 * uinvDet, -um01 * uinvDet,
                        -um10 * uinvDet, um00 * uinvDet
                    );

                    float2 wsdx = float2(ddx(input.positionWS.x), ddx(input.positionWS.y));
                    float2 wsdy = float2(ddy(input.positionWS.x), ddy(input.positionWS.y));

                    float2 grad = float2(aXp0 - aXn0, aYp0 - aYn0);

                    float2 gradPx = mul(toScreen, -grad);

                    float2 gradWorld = wsdx * gradPx.x + wsdy * gradPx.y;

                    float wlen = max(length(gradWorld), 0.0001);

                    float2 normalW = gradWorld / wlen;

                    float3 sunDir = SkySunDir(_SkySunAzEl.x, _SkySunAzEl.y);

                    float facing = saturate(dot(normalW, sunDir.xy));

                    float bevel = anyEdge * facing * _Bevel * saturate(_SkySunAzEl.z);

                    rgb += _BevelColor.rgb * bevel;
                }

                rgb *= max(_Brightness, 0.0);

                half alpha = base.a * input.color.a * saturate(_Alpha);

                half4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));

                SurfaceData2D surfaceData;
                InputData2D inputData;

                InitializeSurfaceData(rgb, alpha, maskTex, normalTS, surfaceData);
                InitializeInputData(uv, input.lightingUV, inputData);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }

    Fallback Off
}