Shader "Diesel/Smoke2D"
{
    Properties
    {
        _Radius    ("Радиус", Range(0.05, 1)) = 0.5
        _Roundness ("Объём", Range(0, 1)) = 0.85
        _Steps     ("Шагов марча", Range(2, 12)) = 6

        _Density   ("Плотность", Range(0, 4)) = 1.6
        _Softness  ("Мягкость края", Range(0.001, 0.4)) = 0.12
        _LightDir  ("Направление света", Vector) = (0.45, 0.6, 0.66)
        _Ambient   ("Ambient", Range(0, 1)) = 0.35
        _Rim       ("Rim", Range(0, 1)) = 0.35
        _PixelSize ("Пикселизация", Range(1, 32)) = 1
        _Seed      ("Сид", Float) = 0
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

        Pass
        {
            Name "Smoke2DUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float  _Radius;
            float  _Roundness;
            int    _Steps;
            float  _Density;
            float  _Softness;
            float4 _LightDir;
            float  _Ambient;
            float  _Rim;
            float  _PixelSize;
            float  _Seed;

            float Hash11(float n)
            {
                n = n - floor(n / 1024.0) * 1024.0;
                return frac(sin(n) * 43758.5453123);
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float R = max(_Radius, 0.001);

                float grid = max(floor(_PixelSize + 0.5), 1.0);
                float2 puv = (floor(input.uv * grid) + 0.5) / grid;

                float2 p = (puv - 0.5) / R;
                float rq = length(p);
                float edge = max(_Softness, 0.001);

                if (rq > 1.0 + edge * 4.0)
                    discard;

                float depth = max(_Roundness, 0.001);
                int steps = clamp(_Steps, 2, 12);
                float jitter = Hash11(input.uv.x * 131.7 + input.uv.y * 77.3 + _Seed + _Time.y);

                float sum = 0.0;

                for (int i = 0; i < 12; i++)
                {
                    if (i >= steps)
                        break;

                    float t = (float(i) + jitter) / (float)steps;
                    float z = (t - 0.5) * 2.0 * depth;

                    float d = length(float3(p, z)) - 1.0;

                    sum += saturate(-d * _Density);
                }

                float body = saturate(sum / (float)steps * 1.6);
                float cover = saturate(1.0 - (rq - 1.0) / edge);

                float alpha = body * cover * input.color.a;

                if (alpha <= 0.002)
                    discard;

                float zs = sqrt(max(0.0, 1.0 - rq * rq));
                float3 normal = normalize(float3(p, zs));

                float3 lightDir = normalize(_LightDir.xyz);
                float ndl = saturate(dot(normal, lightDir));

                float diffuse = lerp(_Ambient, 1.0, ndl);
                float rim = pow(1.0 - saturate(zs), 3.0) * _Rim;

                float3 tint = input.color.rgb;
                float3 rgb = tint * diffuse + tint * rim;

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}