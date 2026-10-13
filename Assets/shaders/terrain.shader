Shader "Diesel/TerrainPixel"
{
    Properties
    {
        _MainTex ("Земля", 2D) = "white" {}
        _UseMainTex ("Использовать текстуру", Range(0, 1)) = 0

        _PixelSize ("Размер пикселя", Range(0.005, 0.2)) = 0.035

        _NoiseScale ("Масштаб шума", Float) = 2
        [Range(1, 32)] _NoiseOctaves ("Октав шума", Float) = 16
        [Range(0, 1)] _NoiseGain ("Затухание октав", Float) = 1
        [Range(0, 1)] _NoiseAmount ("Сила шума", Float) = 0.7
        _Seed ("Сид", Float) = 0

        _NormalStrength ("Сила нормали", Range(0, 2)) = 0.7
        _NormalScale ("Масштаб нормали", Float) = 3
        _NormalAmount ("Высота нормали", Range(0, 1)) = 0.55

        [Range(0, 3)] _DepthNoise ("Шум на глубине", Float) = 1.91
        _DepthScale ("Глубина затемнения", Float) = 3
        [Range(0, 1)] _DepthDark ("Затемнение на глубине", Float) = 1

        [Range(0, 1)] _DepthBlend ("Плавность затемнения", Float) = 1

        _GrassThickness ("Толщина травы", Float) = 0.18

        _GrassOffset ("Сдвиг травы", Float) = 0
        [Range(0, 1)] _GrassNoise ("Сила шума в траве", Float) = 0.7

        _GrassBladeScaleX ("Стебли: шаг по горизонтали", Float) = 4
        _GrassBladeScaleY ("Стебли: шаг по вертикали", Float) = 1

        [Range(0, 0.12)] _GrassJitter ("Шум линии травы", Float) = 0.07

        [Range(0, 2)] _Dither ("Дизеринг", Float) = 1

        [Range(0, 1)] _EdgeNoise ("Рваность края травы", Float) = 0.5

        [Range(0, 1)] _GrassBlend ("Плавность перехода в грунт", Float) = 1

        [Range(0, 1)] _GrassDarken ("Затемнение вокруг травы", Float) = 0.45

        _GrassColor ("Цвет травы", Color) = (0.36, 0.42, 0.22, 1)
        _SoilColor ("Цвет грунта", Color) = (0.33, 0.24, 0.15, 1)

        [Range(2, 64)] _Tones ("Ступеней тона", Float) = 32

        _DayColor ("Днём", Color) = (1, 1, 1, 1)
        _NightColor ("Ночью", Color) = (0.26, 0.28, 0.34, 1)
    }

    SubShader
    {
        Tags
        {

            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "TerrainPixelUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/InputData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/SurfaceData2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float2 depth : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float2 positionWS : TEXCOORD0;
                float2 depth : TEXCOORD1;

                half2 lightingUV : TEXCOORD2;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float _UseMainTex;
            float _PixelSize;
            float _NoiseScale;
            float _NoiseOctaves;
            float _NoiseGain;
            float _NoiseAmount;
            float _Seed;
            float _NormalStrength;
            float _NormalScale;
            float _NormalAmount;
            float _DepthNoise;
            float _DepthScale;
            float _DepthDark;
            float _DepthBlend;
            float _GrassThickness;

            float _GrassOffset;

            float _GrassNoise;
            float _GrassBladeScaleX;
            float _GrassBladeScaleY;
            float _GrassJitter;
            float _Dither;
            float _EdgeNoise;
            float _GrassBlend;
            float _GrassDarken;
            half4 _GrassColor;
            half4 _SoilColor;
            float _Tones;
            half4 _DayColor;
            half4 _NightColor;
            CBUFFER_END

            float4 _SkySunAzEl;

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

            float2 PixelCell(float2 p)
            {
                float s = max(_PixelSize, 0.0001);
                return (floor(p / s) + 0.5) * s;
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

                float2 p = floor(id) + floor(float2(_Seed * 3.0, _Seed * 7.0));

                float2 tile = floor(p / 8.0);
                float2 phase = floor((Hash2(tile + 53.7) * 0.5 + 0.5) * 8.0);
                p = p - tile * 8.0 + phase;
                p = p - floor(p / 8.0) * 8.0;

                return Bayer8(p) * 64.0 / 63.0;
            }

            float LineJitter(float2 id, float block)
            {
                float2 w = floor(id / HASH_WRAP);
                float2 r = id - w * HASH_WRAP;

                float2 v = frac(w * float2(0.1031, 0.1030) + float2(0.1030, 0.1031));

                r += v * 17.0;

                float h = frac(Hash2(r).x * 0.5 + 0.5 + _Seed * 0.3173);

                float q = floor(h * 4.0 + 0.5) / 4.0;

                float j = (q * 2.0 - 1.0) * max(_GrassJitter, 0.0);

                return floor(j / block + 0.5) * block;
            }

            float Quantize(float v, float lo, float hi, float tones)
            {
                float range = max(hi - lo, 0.0001);
                float steps = max(tones, 2.0) - 1.0;

                float t = saturate((v - lo) / range);

                return lo + floor(t * steps + 0.5) / steps * range;
            }

            float GrassSurround(float2 cell, float cellDepth, float offset, float s)
            {
                float sum = 0.0;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        float2 nc = PixelCell(cell + float2((float)dx, (float)dy) * s);
                        float2 nid = floor(nc / s);

                        float nline = _GrassThickness + LineJitter(nid, s);

                        float ndither = DitherThreshold(nid);

                        float nedge = (ndither - 0.5) * max(_Dither, 0.0) * s;

                        float ndepth = cellDepth + (cell.y - nc.y);

                        float ndist = offset + nline + nedge - ndepth;

                        float fade = max(_GrassBlend * max(nline, 0.0001), 0.0001);

                        float g = smoothstep(0.0, fade, ndist);

                        if (offset > 0.0)
                            g *= saturate((ndepth - offset) / max(s, 0.0001));

                        sum += g;
                    }
                }

                return sum / 8.0;
            }

            half3 LinearToGammaApprox(half3 c)
            {
                half3 x = max(c, half3(0.0h, 0.0h, 0.0h));
                half3 g = half3(0.416666667h, 0.416666667h, 0.416666667h);
                half3 y = half3(
                    pow(x.r, g.r),
                    pow(x.g, g.g),
                    pow(x.b, g.b)
                );
                return max(1.055h * y - 0.055h, half3(0.0h, 0.0h, 0.0h));
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz).xy;
                output.depth = input.depth;
                output.color = input.color;
                output.lightingUV = half2(ComputeScreenPos(output.positionCS / output.positionCS.w).xy);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {

                float depth = input.depth.x;

                float deepT = saturate(depth / max(_DepthScale, 0.0001));

                float deep = lerp(deepT, deepT * deepT * (3.0 - 2.0 * deepT), _DepthBlend);

                float s = max(_PixelSize, 0.0001);

                float2 cell = PixelCell(input.positionWS);

                float2 cellId = floor(cell / s);

                float cellDepth = depth + (input.positionWS.y - cell.y);

                float grassLine = _GrassThickness + LineJitter(cellId, s);

                float dither = DitherThreshold(cellId);

                float edge = (dither - 0.5) * max(_Dither, 0.0) * s;

                float offset = max(_GrassOffset, -0.4);

                if (offset >= 0.0)
                    clip(depth);
                else
                    clip(cellDepth - offset - edge);

                float grassLineNoisy = grassLine;

                if (cellDepth < offset + grassLine * (1.0 + max(_EdgeNoise, 0.0) * 2.0))
                {
                    float edgeGrain = Fbm(
                        cell * _GrassBladeScaleX * 0.75,
                        _Seed + 31.7,
                        _NoiseOctaves,
                        _NoiseGain
                    );

                    grassLineNoisy += (edgeGrain - 0.5) * max(_EdgeNoise, 0.0) * grassLine;
                }

                float grassDist = offset + grassLineNoisy + edge - cellDepth;

                float fade = max(_GrassBlend * grassLine, 0.0001);

                float grassT = smoothstep(0.0, fade, grassDist);

                if (offset > 0.0)
                {
                    float edgeTop = (DitherThreshold(cellId + float2(3.0, 5.0)) - 0.5)
                        * max(_Dither, 0.0)
                        * s;

                    grassT *= saturate((cellDepth - offset + edgeTop) / s);
                }

                float surround = GrassSurround(cell, cellDepth, offset, s) * (1.0 - grassT);

                float n = Fbm(cell * _NoiseScale, _Seed, _NoiseOctaves, _NoiseGain);

                if (grassT > 0.0)
                {
                    float blades = Fbm(
                        cell * float2(_GrassBladeScaleX, _GrassBladeScaleY),
                        _Seed + 7.0,
                        _NoiseOctaves,
                        _NoiseGain
                    );

                    n = lerp(n, blades, grassT);
                }

                float amount = lerp(_NoiseAmount * lerp(1.0, _DepthNoise, deep), _GrassNoise, grassT);

                float range = lerp(1.0, 1.0 - _DepthDark, deep);

                range *= lerp(1.0, 1.0 - _GrassDarken, surround);

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

                half3 albedo = lerp(
                    LinearToGammaApprox(_SoilColor.rgb),
                    LinearToGammaApprox(_GrassColor.rgb),
                    grassT
                );

                if (_UseMainTex > 0.5)
                    albedo *= SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.positionWS * _MainTex_ST.xy + _MainTex_ST.zw).rgb;

                half3 col = albedo * tone;

                col += (dither - 0.5) * max(_Dither, 0.0) * 0.035;

                col *= lerp(_NightColor.rgb, _DayColor.rgb, saturate(_SkySunAzEl.z));

                SurfaceData2D surfaceData;
                InputData2D inputData;
                InitializeSurfaceData(col, 1.0h, surfaceData);
                InitializeInputData(float2(0.0, 0.0), input.lightingUV, inputData);

                return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Name "TerrainPixelNormals"
            Tags { "LightMode" = "NormalsRendering" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex NormalVert
            #pragma fragment NormalFrag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float _Seed;
            float _PixelSize;
            float _NormalScale;
            float _NormalStrength;
            float _NormalAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                float2 positionWS : TEXCOORD0;
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

            float NormalHeight(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float total = 0.0;

                for (int i = 0; i < 4; i++)
                {
                    float fi = (float)i;

                    float2 shift = float2(
                        Rnd(fi * 127.1 + _Seed * 311.7) * 12.0,
                        Rnd(fi * 269.5 + _Seed * 183.3) * 12.0
                    );

                    value += amplitude * Noise(p * frequency + shift);
                    total += amplitude;

                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return (value / total) * _NormalAmount;
            }

            Varyings NormalVert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz).xy;

                return output;
            }

            half4 NormalFrag(Varyings input) : SV_Target
            {

                float e = max(_PixelSize, 0.005);

                float2 p = input.positionWS * _NormalScale;

                float c = NormalHeight(p);
                float gx = NormalHeight(p + float2(e, 0.0)) - NormalHeight(p - float2(e, 0.0));
                float gy = NormalHeight(p + float2(0.0, e)) - NormalHeight(p - float2(0.0, e));

                half3 normal = normalize(half3(-gx * _NormalStrength, -gy * _NormalStrength, 1.0h));

                half4 o;
                o.rgb = 0.5 * (normal + 1.0h);
                o.a = 1.0h;

                return o;
            }
            ENDHLSL
        }
    }

    Fallback Off
}