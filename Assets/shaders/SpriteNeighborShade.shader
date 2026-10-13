Shader "Diesel/SpriteNeighborShade"
{

    Properties
    {
        [PerRendererData] _MainTex ("Текстура спрайта", 2D) = "white" {}
        [PerRendererData] _Color ("Оттенок", Color) = (1, 1, 1, 1)
        _RendererColor ("Оттенок рендерера", Color) = (1, 1, 1, 1)

        _PixelSize ("Размер пикселя экрана, px", Float) = 1.0

        _DarkThreshold ("Порог темноты соседа", Float) = 0.35
        _ShadeRadius ("Радиус опроса, пикселей текстуры", Float) = 3.0
        _ShadeStrength ("Сила затемнения", Float) = 1.0
        _SelfShade ("Затенять сам тёмный пиксель", Float) = 1.0
        _ShadeColor ("Цвет тени", Color) = (0.35, 0.40, 0.55, 1)

        _ShadeSamples ("Сэмплов в кольце", Float) = 8.0
        _ShadeRings ("Колец", Float) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Pass
        {
            Name "SpriteNeighborShade"

            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define SHADE_MAX_SAMPLES 12
            #define SHADE_MAX_RINGS 4

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            half4  _Color;
            half4  _RendererColor;

            float _PixelSize;
            float _DarkThreshold;
            float _ShadeRadius;
            float _ShadeStrength;
            float _SelfShade;
            half4  _ShadeColor;
            float _ShadeSamples;
            float _ShadeRings;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float Luma(half3 c)
            {
                return dot(c, half3(0.299, 0.587, 0.114));
            }

            float RingDarkness(float2 uv, float radiusTexels, float2 texel, int samples, float threshold)
            {
                float sum = 0.0;

                for (int i = 0; i < SHADE_MAX_SAMPLES; i++)
                {
                    if (i >= samples)
                        break;

                    float angle = (float)i / (float)samples * 6.2831853;
                    float2 dir = float2(cos(angle), sin(angle));

                    half4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + dir * (radiusTexels * texel));

                    float solid = step(0.001, s.a);

                    sum += saturate(threshold - Luma(s.rgb)) * solid;
                }

                return sum / (float)samples;
            }

            float NeighborShade(float2 uv, float radiusTexels, float2 texel, int samples, int rings, float threshold)
            {
                if (radiusTexels <= 0.0 || samples <= 0 || rings <= 0)
                    return 0.0;

                float total = 0.0;
                float weight = 0.0;

                for (int r = 0; r < SHADE_MAX_RINGS; r++)
                {
                    if (r >= rings)
                        break;

                    float ringRadius = radiusTexels * ((float)r + 1.0) / (float)rings;
                    float w = 1.0 - (float)r / (float)rings;

                    total += w * RingDarkness(uv, ringRadius, texel, samples, threshold);
                    weight += w;
                }

                if (weight <= 0.0)
                    return 0.0;

                return total / weight;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {

                float2 uv = input.uv;

                if (_PixelSize > 1.0)
                {
                    float2 grid = max(floor(_ScreenParams.xy / _PixelSize), float2(1.0, 1.0));
                    uv = floor(uv * grid) / grid;
                }

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float2 texel = _MainTex_TexelSize.xy;

                int samples = (int)clamp(_ShadeSamples, 1.0, (float)SHADE_MAX_SAMPLES);
                int rings = (int)clamp(_ShadeRings, 1.0, (float)SHADE_MAX_RINGS);

                float amount = NeighborShade(uv, _ShadeRadius, texel, samples, rings, _DarkThreshold);

                float selfDark = saturate(_DarkThreshold - Luma(tex.rgb)) * step(0.001, tex.a);
                amount = saturate(amount + selfDark * _SelfShade);

                float3 col = tex.rgb * _Color.rgb * _RendererColor.rgb;

                col = lerp(col, col * _ShadeColor.rgb, amount * _ShadeStrength);

                float alpha = tex.a * _Color.a * _RendererColor.a;

                return half4(col * alpha, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
