Shader "Diesel/SpriteRust"
{
    Properties
    {
        [PerRendererData] _MainTex ("Спрайт", 2D) = "white" {}
        _MaskTex ("Маска света", 2D) = "white" {}
        _NormalMap ("Карта нормалей", 2D) = "bump" {}
        [MaterialToggle] _ZWrite ("ZWrite", Float) = 0

        _Rust      ("Уровень ржавчины", Range(0, 1)) = 0.5
        _PixelSize ("Размер пикселя, текселей", Range(1, 16)) = 4
        _Threshold ("Порог ржавчины", Range(0, 1)) = 0.45
        _Softness  ("Мягкость края", Range(0.001, 0.5)) = 0.08

        _Scale   ("Размер пятна, текселей", Float) = 48
        _Octaves ("Октавы", Range(1, 6)) = 4
        _Seed    ("Сид", Float) = 0

        _Brightness ("Яркость", Range(0, 4)) = 1

        _RustDark  ("Ржавчина, тень", Color) = (0.20, 0.08, 0.03, 1)
        _RustLight ("Ржавчина, блик", Color) = (0.74, 0.44, 0.19, 1)
        _RustPit   ("Глубина коррозии", Range(0, 1)) = 0.5

        _Crack      ("Плотность разлома", Range(0, 1)) = 0.4
        _CrackDepth ("Глубина разлома, пикселей", Range(1, 6)) = 3
        _CrackScale ("Масштаб разлома", Float) = 0.7
        _CrackWidth ("Толщина трещины", Range(0.01, 0.6)) = 0.12
        _CrackColor ("Цвет трещины", Color) = (0.10, 0.08, 0.07, 1)

        _Bevel      ("Бевол края", Range(0, 2)) = 0.4
        _BevelColor ("Цвет бевола", Color) = (1, 1, 1, 1)
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
            Name "SpriteRustLit"
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

                float _Rust;
                float _PixelSize;
                float _Threshold;
                float _Softness;
                float _Scale;
                float _Octaves;
                float _Seed;
                float _Brightness;
                half4 _RustDark;
                half4 _RustLight;
                float _RustPit;

                float _Crack;
                float _CrackDepth;
                float _CrackScale;
                float _CrackWidth;
                half4 _CrackColor;

                float _Bevel;
                half4 _BevelColor;
            CBUFFER_END

            float4 _MainTex_TexelSize;

            float4 _SkySunAzEl;

            #define HASH_WRAP 64.0

            float HashWrap(float2 p)
            {
                p = p - floor(p / HASH_WRAP) * HASH_WRAP;
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                f = f * f * (3.0 - 2.0 * f);

                float a = HashWrap(i);
                float b = HashWrap(i + float2(1.0, 0.0));
                float c = HashWrap(i + float2(0.0, 1.0));
                float d = HashWrap(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float Fbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                float total = 0.0;

                for (int i = 0; i < 6; i++)
                {
                    if (i >= octaves)
                        break;

                    float fi = (float)i;

                    float2 shift = float2(
                        HashWrap(float2(fi * 1.7 + _Seed, fi * 3.1)) * 12.0,
                        HashWrap(float2(fi * 2.9 + _Seed, fi * 5.3)) * 12.0
                    );

                    value += amp * ValueNoise(p * freq + shift);
                    total += amp;

                    freq *= 2.0;
                    amp *= 0.5;
                }

                if (total <= 0.0)
                    return 0.0;

                return value / total;
            }

            float RidgedFbm(float2 p, int octaves)
            {
                float value = 0.0;
                float amp = 0.5;
                float freq = 1.0;
                float total = 0.0;

                for (int i = 0; i < 6; i++)
                {
                    if (i >= octaves)
                        break;

                    float fi = (float)i;

                    float2 shift = float2(
                        HashWrap(float2(fi * 3.7 + _Seed, fi * 1.9)) * 12.0,
                        HashWrap(float2(fi * 4.3 + _Seed, fi * 2.7)) * 12.0
                    );

                    float n = ValueNoise(p * freq + shift);

                    float ridge = 1.0 - abs(n * 2.0 - 1.0);
                    ridge *= ridge;

                    value += amp * ridge;
                    total += amp;

                    freq *= 2.0;
                    amp *= 0.5;
                }

                if (total <= 0.0)
                    return 0.0;

                return value / total;
            }

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

                float pixel = max(_PixelSize, 1.0);

                float2 texel = max(_MainTex_TexelSize.xy, float2(1.0, 1.0) / 16384.0);

                float2 blockUV = texel * pixel;

                float2 cellUV = floor(uv / blockUV) * blockUV;

                float2 texelCoord = cellUV / texel;

                float2 noiseUV = texelCoord / max(_Scale, 0.001) + _Seed;

                int octaves = (int)clamp(_Octaves, 1.0, 6.0);

                float crackAmount = saturate(_Crack);

                float2 offX = float2(blockUV.x, 0.0);
                float2 offY = float2(0.0, blockUV.y);

                float depthLimit = clamp(_CrackDepth, 1.0, 6.0);

                float dist = depthLimit + 1.0;

                for (int j = 1; j <= 6; j++)
                {
                    if (crackAmount <= 0.0)
                        break;

                    if ((float)j > depthLimit)
                        break;

                    float fj = (float)j;

                    float2 sx = offX * fj;
                    float2 sy = offY * fj;

                    float aXn = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - sx).a;
                    float aXp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + sx).a;
                    float aYn = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - sy).a;
                    float aYp = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + sy).a;

                    float mn = min(min(aXn, aXp), min(aYn, aYp));

                    if (mn < 1.0)
                    {
                        dist = fj;
                        break;
                    }
                }

                float band = step(dist, depthLimit);

                float core = 0.0;
                float halo = 0.0;

                if (crackAmount > 0.0 && band > 0.0)
                {
                    float crackScale = max(_CrackScale, 0.001);

                    float crackNoise = RidgedFbm(noiseUV / crackScale + 31.7, octaves);

                    float gate = HashWrap(floor(noiseUV / crackScale) + 7.3);

                    float present = step(gate, crackAmount);

                    float w = clamp(_CrackWidth, 0.01, 0.6);

                    core = step(1.0 - w, crackNoise) * band * present;

                    float wWide = min(w * 3.0, 0.9);

                    halo = saturate((crackNoise - (1.0 - wWide)) / max(wWide, 0.001)) * band * present;
                }

                half alpha = base.a * input.color.a;

                if (core > 0.0)
                    alpha = 0.0h;

                float3 rgb = base.rgb;

                float rustAmount = saturate(_Rust);

                if (rustAmount > 0.0)
                {
                    float n = Fbm(noiseUV, octaves);

                    float mask = saturate((n - _Threshold) / max(_Softness, 0.001));
                    mask *= rustAmount;

                    if (mask > 0.0)
                    {
                        float relief = Fbm(noiseUV * 1.9 + 13.0, max(octaves - 1, 1));

                        float3 rust = lerp(_RustDark.rgb, _RustLight.rgb, saturate(n * 1.35));
                        rust = lerp(rust, _RustDark.rgb, saturate((0.5 - relief) * 1.3));

                        float pit = saturate((0.42 - relief) * 2.2);
                        rust = lerp(rust, _RustDark.rgb, pit * _RustPit);

                        rgb = lerp(rgb, rust, mask);
                    }
                }

                rgb *= input.color.rgb;

                rgb = lerp(rgb, rgb * _CrackColor.rgb, saturate(halo) * (1.0 - core));

                float aXn0 = AlphaAt(uv - offX);
                float aXp0 = AlphaAt(uv + offX);
                float aYn0 = AlphaAt(uv - offY);
                float aYp0 = AlphaAt(uv + offY);

                float anyEdge = 1.0 - min(min(aXn0, aXp0), min(aYn0, aYp0));

                anyEdge *= base.a;

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

                if (_Bevel > 0.001 && anyEdge > 0.0)
                {

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