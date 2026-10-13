Shader "Diesel/SpriteGrass"
{

    Properties
    {
        [PerRendererData] _MainTex ("Спрайт", 2D) = "white" {}
        [PerRendererData] _Color ("Оттенок", Color) = (1, 1, 1, 1)
        _RendererColor ("Оттенок рендерера", Color) = (1, 1, 1, 1)

        _GrassColor ("Цвет травы", Color) = (1, 1, 1, 1)

        [Range(1, 16)] _PixelSize ("Размер пикселя, tex", Float) = 2

        [Range(0.3, 1)] _BladeHeight ("Высота травинок", Float) = 0.9

        [Range(0.05, 0.8)] _BladeWidth ("Ширина травинок", Float) = 0.3

        [Range(0, 1)] _BladeBend ("Развилка в стороны", Float) = 0.35

        [Range(0, 8)] _HeightJitter ("Рваность краёв, блоков", Float) = 2.5

        [Range(0, 1)] _EdgeNoise ("Рваность кромки по шуму", Float) = 0.5

        [Range(0, 0.5)] _WindStrength ("Ветер: наклон верха", Float) = 0.12

        [Range(0, 10)] _WindSpeed ("Ветер: скорость порывов", Float) = 2.5

        [Range(0, 1)] _WindFlutter ("Ветер: трепет кромки", Float) = 0.5

        _NoiseScaleX ("Зерно: шаг по горизонтали", Float) = 4
        _NoiseScaleY ("Зерно: шаг по вертикали", Float) = 2

        [Range(1, 32)] _NoiseOctaves ("Октав шума", Float) = 16
        [Range(0, 1)] _NoiseGain ("Затухание октав", Float) = 1

        [Range(0, 1)] _NoiseAmount ("Сила шума", Float) = 0.7

        _Seed ("Сид", Float) = 0

        [Range(0, 1)] _BottomDark ("Затемнение к низу", Float) = 0.4

        [Range(2, 64)] _Tones ("Ступеней тона", Float) = 32

        [Range(0, 2)] _Dither ("Дизеринг", Float) = 1

        _DayColor ("Днём", Color) = (1, 1, 1, 1)
        _NightColor ("Ночью", Color) = (0.26, 0.28, 0.34, 1)

        [Range(0, 2)] _Bevel ("Бевол края", Float) = 0.4
        _BevelColor ("Цвет бевола", Color) = (1, 1, 1, 1)
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
            Name "SpriteGrassUnlit"

            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/shaders/SkySun.hlsl"

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize;

            half4 _Color;
            half4 _RendererColor;
            half4 _GrassColor;

            float _PixelSize;
            float _BladeHeight;
            float _BladeWidth;
            float _BladeBend;
            float _HeightJitter;
            float _EdgeNoise;
            float _NoiseScaleX;
            float _NoiseScaleY;
            float _NoiseOctaves;
            float _NoiseGain;
            float _NoiseAmount;
            float _Seed;
            float _BottomDark;
            float _Tones;
            float _Dither;
            float _WindStrength;
            float _WindSpeed;
            float _WindFlutter;
            half4 _DayColor;
            half4 _NightColor;

            float _Bevel;
            half4 _BevelColor;

            float4 _SkySunAzEl;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                float2 positionWS : TEXCOORD1;

                half2 lightingUV   : TEXCOORD2;
                float4 color      : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #define HASH_WRAP 65536.0

            float2 Hash2(float2 p)
            {
                float2 w = floor(p / HASH_WRAP);
                float2 l = p - w * HASH_WRAP;

                float2 v = frac(w * float2(0.1031, 0.1030) + float2(0.1030, 0.1031));

                l += v * 17.0;

                l = frac(l * float2(17.1, 31.7) * (1.0 + 0.09 * dot(v, float2(0.7, 0.3))));

                l += dot(l, l + 45.32) + dot(v, float2(71.7, 91.3));

                l = frac(l * (l.yx + float2(9.1, 0.1)) + v * 0.73);

                return l * 2.0 - 1.0;
            }

            float Noise(float2 p)
            {
                float2 w = floor(p / HASH_WRAP);
                float2 r = p - w * HASH_WRAP;

                float2 i = floor(r);
                float2 f = r - i;
                float2 u = f * f * (3.0 - 2.0 * f);

                float2 corner = i + w * HASH_WRAP;

                float a = dot(Hash2(corner), f);
                float b = dot(Hash2(corner + float2(1.0, 0.0)), f - float2(1.0, 0.0));
                float c = dot(Hash2(corner + float2(0.0, 1.0)), f - float2(0.0, 1.0));
                float d = dot(Hash2(corner + float2(1.0, 1.0)), f - float2(1.0, 1.0));

                return 0.5 + 0.5 * lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float Rnd(float seed)
            {
                return frac(sin(seed * 12.9898) * 43758.5453);
            }

            float Fbm(float2 p, float seed, float octaves, float gain)
            {
                int steps = (int)clamp(octaves, 1.0, 32.0);

                float maxFrequency = 16777216.0 / HASH_WRAP;

                float maxCoord = max(max(abs(p.x), abs(p.y)), 1.0);

                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float total = 0.0;

                for (int i = 0; i < 32; i++)
                {
                    if (i >= steps)
                        break;

                    if (frequency > maxFrequency)
                        break;

                    if (maxCoord * frequency > 16777216.0)
                        break;

                    float fi = (float)i;

                    float2 shift = float2(
                        Rnd(fi * 127.1 + seed * 311.7) * 12.0,
                        Rnd(fi * 269.5 + seed * 183.3) * 12.0
                    );

                    float2 coord = p * frequency + shift;

                    value += amplitude * Noise(coord);
                    total += amplitude;

                    frequency *= 2.0;
                    amplitude *= max(gain, 0.0);
                }

                if (total <= 0.0)
                    return 0.0;

                return value / total;
            }

            float Bayer2(float2 p)
            {
                p = floor(p);

                return frac(p.x * 0.5 + p.y * p.y * 0.75);
            }

            float Bayer8(float2 p)
            {
                return Bayer2(floor(p * 0.25)) * 0.0625
                    + Bayer2(floor(p * 0.5)) * 0.25
                    + Bayer2(p);
            }

            float DitherThreshold(float2 id)
            {

                return Bayer8(id) * 64.0 / 63.0;
            }

            float HeightJitter(float2 id, float block)
            {
                float2 w = floor(id / HASH_WRAP);
                float2 r = id - w * HASH_WRAP;

                float2 v = frac(w * float2(0.1031, 0.1030) + float2(0.1030, 0.1031));

                r += v * 17.0;

                r = frac(r * float2(17.1, 31.7) * (1.0 + 0.09 * dot(v, float2(0.7, 0.3))));

                float h = frac(dot(r, float2(53.1, 160.3)) + _Seed * 0.3173);

                float q = floor(h * 4.0 + 0.5) / 4.0;

                float j = (q * 2.0 - 1.0) * max(_HeightJitter, 0.0) * block;

                return floor(j / block + 0.5) * block;
            }

            float Quantize(float v, float lo, float hi, float tones)
            {
                float range = max(hi - lo, 0.0001);
                float steps = max(tones, 2.0) - 1.0;

                float t = saturate((v - lo) / range);

                return lo + floor(t * steps + 0.5) / steps * range;
            }

            float WindGust(float worldX, float time)
            {
                float phase = frac(abs(_Seed) * 0.61803) * 6.2832;

                float t = time * max(_WindSpeed, 0.0);

                float a = sin(t - worldX * 0.45 + phase);

                float b = sin(t * 1.63 - worldX * 0.7 + phase * 1.71 + 1.3);

                return a * 0.7 + b * 0.3;
            }

            float GrassOutside(float2 p, float2 grid, float blockUvX, float blockUvY, float blockSlotX)
            {
                float slot = floor(p.x * 8.0);

                float localX = p.x * 8.0 - slot;

                float rndH = Rnd(slot * 127.1 + _Seed * 311.7 + 3.7);
                float rndB = Rnd(slot * 269.5 + _Seed * 183.3 + 11.3);

                float height = saturate(_BladeHeight) * lerp(0.65, 1.0, rndH);

                float t = p.y / max(height, 0.001);

                float bendDir = (slot - 3.5) / 3.5;

                float bend = max(_BladeBend, 0.0) * lerp(0.75, 1.25, rndB);

                float centerX = 0.5
                    + bendDir * bend * t * t
                    + (rndB - 0.5) * 0.2 * t;

                float halfW = max(_BladeWidth, 0.0) * 0.5
                    * pow(1.0 - saturate(t), 0.7);

                float dx = abs(localX - centerX);

                float2 pid = floor(p * grid);

                float dither = DitherThreshold(pid);

                float tip = height
                    + HeightJitter(pid + float2(5.3, 7.1), blockUvY)
                    + (dither - 0.5) * max(_Dither, 0.0) * blockUvY;

                float side = halfW
                    + HeightJitter(pid + float2(27.1, 13.7), blockSlotX)
                    + (dither - 0.5) * max(_Dither, 0.0) * blockSlotX;

                return (dx > side || p.y > tip) ? 1.0 : 0.0;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 pos = input.positionOS.xyz;

                float gust = WindGust(TransformObjectToWorld(pos).x, _Time.y);

                float stiff = 0.7 + 0.6 * frac(abs(_Seed) * 0.37);

                pos.x += gust * max(_WindStrength, 0.0) * stiff
                    * input.uv.y * input.uv.y;

                output.positionCS = TransformObjectToHClip(pos);
                output.positionWS = TransformObjectToWorld(pos).xy;
                output.uv = input.uv;
                output.color = input.color;
                output.lightingUV = half2(ComputeScreenPos(output.positionCS / output.positionCS.w).xy);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float s = max(_PixelSize, 1.0);

                float m00 = ddx(uv.x);
                float m01 = ddy(uv.x);
                float m10 = ddx(uv.y);
                float m11 = ddy(uv.y);

                float det = m00 * m11 - m01 * m10;

                if (abs(det) < 1e-12)
                    det = det < 0.0 ? -1e-12 : 1e-12;

                float invDet = 1.0 / det;

                float2x2 toScreen = float2x2(
                    m11 * invDet, -m01 * invDet,
                    -m10 * invDet, m00 * invDet
                );

                float spriteW = length(mul(toScreen, float2(1.0, 0.0)));
                float spriteH = length(mul(toScreen, float2(0.0, 1.0)));

                float2 texels = max(_MainTex_TexelSize.zw, float2(1.0, 1.0));

                float2 grid = texels / s;

                grid = min(grid, max(float2(spriteW, spriteH), float2(1.0, 1.0)));

                float2 blockId = floor(uv * grid);

                float2 blockUv = (blockId + 0.5) / grid;

                float blockUvX = max(1.0 / grid.x, 1e-6);
                float blockUvY = max(1.0 / grid.y, 1e-6);

                float blockSlotX = blockUvX * 8.0;

                float2 dwsdx = float2(ddx(input.positionWS.x), ddx(input.positionWS.y));
                float2 dwsdy = float2(ddy(input.positionWS.x), ddy(input.positionWS.y));

                float2 dScreen = mul(toScreen, blockUv - uv);

                float2 blockWs = input.positionWS + dwsdx * dScreen.x + dwsdy * dScreen.y;

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, blockUv);

                half alpha = tex.a * input.color.a * _Color.a * _RendererColor.a;

                if (alpha <= 0.003)
                    discard;

                float dither = DitherThreshold(blockId);

                float edge = (dither - 0.5) * max(_Dither, 0.0);

                float slot = floor(blockUv.x * 8.0);

                float localX = blockUv.x * 8.0 - slot;

                float rndH = Rnd(slot * 127.1 + _Seed * 311.7 + 3.7);

                float rndB = Rnd(slot * 269.5 + _Seed * 183.3 + 11.3);

                float height = saturate(_BladeHeight) * lerp(0.65, 1.0, rndH);

                float t = blockUv.y / max(height, 0.001);

                float bendDir = (slot - 3.5) / 3.5;

                float bend = max(_BladeBend, 0.0) * lerp(0.75, 1.25, rndB);

                float centerX = 0.5
                    + bendDir * bend * t * t
                    + (rndB - 0.5) * 0.2 * t;

                float halfW = max(_BladeWidth, 0.0) * 0.5
                    * pow(1.0 - saturate(t), 0.7);

                float dx = abs(localX - centerX);

                float tip = height
                    + HeightJitter(blockId + float2(5.3, 7.1), blockUvY)
                    + edge * blockUvY;

                float side = halfW
                    + HeightJitter(blockId + float2(27.1, 13.7), blockSlotX)
                    + edge * blockSlotX;

                float band = (max(_EdgeNoise, 0.0) * 4.0 + max(_HeightJitter, 0.0) + 1.0);

                bool nearEdge = abs(blockUv.y - height) < band * blockUvY
                    || abs(dx - halfW) < band * blockSlotX;

                if (nearEdge)
                {
                    float grain = Fbm(
                        blockWs * float2(_NoiseScaleX, _NoiseScaleY),
                        _Seed + 31.7,
                        _NoiseOctaves,
                        _NoiseGain
                    );

                    tip += (grain - 0.5) * max(_EdgeNoise, 0.0) * 4.0 * blockUvY;
                    side += (grain - 0.5) * max(_EdgeNoise, 0.0) * 4.0 * blockSlotX;

                    float flutter = sin(
                        _Time.y * max(_WindSpeed, 0.0) * 3.0
                        + blockId.x * 2.1 + blockId.y * 1.7 + _Seed
                    );

                    tip += flutter * max(_WindFlutter, 0.0) * blockUvY;
                    side += flutter * max(_WindFlutter, 0.0) * blockSlotX;
                }

                if (dx > side)
                    discard;

                if (blockUv.y > tip)
                    discard;

                float depth = saturate((tip - blockUv.y) / max(tip, 0.0001));

                float range = lerp(1.0, 1.0 - max(_BottomDark, 0.0), depth);

                float n = Fbm(
                    blockWs * float2(_NoiseScaleX, _NoiseScaleY),
                    _Seed,
                    _NoiseOctaves,
                    _NoiseGain
                );

                float amount = max(_NoiseAmount, 0.0);

                float lo = (1.0 - amount) * range;
                float hi = (1.0 + amount * 0.35) * range;

                float shade = lerp(1.0 - amount, 1.0 + amount * 0.35, n) * range;

                float toneStep = (hi - lo) / max(_Tones - 1.0, 1.0);

                float tone = Quantize(
                    shade + (dither - 0.5) * max(_Dither, 0.0) * toneStep,
                    lo,
                    hi,
                    _Tones
                );

                half3 col = tex.rgb * _GrassColor.rgb * input.color.rgb * _Color.rgb * _RendererColor.rgb;

                col *= tone;

                col += (dither - 0.5) * max(_Dither, 0.0) * 0.035;

                col *= lerp(_NightColor.rgb, _DayColor.rgb, saturate(_SkySunAzEl.z));

                float qXm = GrassOutside(blockUv - float2(blockUvX, 0.0), grid, blockUvX, blockUvY, blockSlotX);
                float qXp = GrassOutside(blockUv + float2(blockUvX, 0.0), grid, blockUvX, blockUvY, blockSlotX);
                float qYm = GrassOutside(blockUv - float2(0.0, blockUvY), grid, blockUvX, blockUvY, blockSlotX);
                float qYp = GrassOutside(blockUv + float2(0.0, blockUvY), grid, blockUvX, blockUvY, blockSlotX);

                float anyEdge = max(max(qXm, qXp), max(qYm, qYp)) * saturate(alpha);

                if (_Bevel > 0.001 && anyEdge > 0.0)
                {
                    float2 gradShape = float2(qXp - qXm, qYp - qYm);

                    float2 gradPx = mul(toScreen, gradShape);

                    float2 gradWorld = dwsdx * gradPx.x + dwsdy * gradPx.y;

                    float wlen = max(length(gradWorld), 0.0001);

                    float2 normalW = gradWorld / wlen;

                    float3 sunDir = SkySunDir(_SkySunAzEl.x, _SkySunAzEl.y);

                    float facing = saturate(dot(normalW, sunDir.xy));

                    float bevel = anyEdge * facing * _Bevel * saturate(_SkySunAzEl.z);

                    col += _BevelColor.rgb * bevel;
                }

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(col, alpha, surfaceData);
                InitializeInputData(input.uv, input.lightingUV, inputData);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
