Shader "Diesel/VolumetricClouds"
{

    Properties
    {
        _CloudScale      ("Масштаб облаков", Float) = 1.0
        _CloudDetail     ("Детализация", Float) = 1.7
        _CloudSpeed      ("Скорость ветра", Float) = 1.0
        _CloudHeight     ("Высота слоя", Float) = 1.0
        _CloudThickness  ("Толщина слоя", Float) = 2.0

        _CloudCoverage   ("Плотность покрытия", Range(0, 1)) = 0.45
        _CloudDensity    ("Плотность облаков", Float) = 1.0
        _MarchSteps      ("Шагов марча", Range(8, 64)) = 32

        _UseGlobalSun   ("Солнце от DayNightCycle", Range(0, 1)) = 1
        _SunColor       ("Цвет солнца", Color) = (1.00, 0.94, 0.78, 1)
        _SunIntensity   ("Яркость диска", Range(0, 8)) = 3.0
        _SunSize        ("Размер диска", Range(0.002, 0.30)) = 0.05
        _SunGlow        ("Свечение вокруг", Range(0, 3)) = 0.5
        _SunGlowSize    ("Ширина свечения", Range(0.02, 1.50)) = 0.40
        _SunScreenScale ("Отступ диска от края", Range(0.50, 1.00)) = 0.90
        _SunElevation   ("Высота над горизонтом", Range(-0.5, 1.5)) = 0.6

        _MoonColor      ("Цвет луны", Color) = (0.84, 0.88, 1.00, 1)
        _MoonIntensity  ("Яркость диска луны", Range(0, 8)) = 1.7
        _MoonSize       ("Размер диска луны", Range(0.002, 0.30)) = 0.045
        _MoonGlow       ("Свечение вокруг луны", Range(0, 3)) = 0.30
        _MoonGlowSize   ("Ширина свечения луны", Range(0.02, 1.50)) = 0.28

        _BelowFade      ("Глубина затухания под горизонтом", Range(0.05, 1.00)) = 0.45

        _LightAngle      ("Азимут солнца", Float) = 0.6
        _LightScatter    ("Рассеивание", Range(0, 4)) = 1.2

        _FadeStart       ("Начало затухания", Float) = 12.0
        _FadeEnd         ("Конец затухания", Float) = 30.0

        _PixelSize       ("Размер пикселя, экранных px", Range(1, 16)) = 4.0
        _SkyBands        ("Полос неба", Range(2, 128)) = 128.0

        _SkyZenith       ("Небо, зенит", Color) = (0.16, 0.35, 0.62, 1)
        _SkyHorizon      ("Небо, горизонт", Color) = (0.72, 0.82, 0.90, 1)
        _HorizonHeight   ("Высота горизонта", Range(-1, 1)) = 0.4

        _NightStrength   ("Сила ночи", Range(0, 1)) = 1.0
        _NightAmbient    ("Ночная подсветка", Color) = (0.32, 0.32, 0.38, 1)
        _NightZenith     ("Ночь, зенит", Color) = (0.09, 0.098, 0.138, 1)
        _NightHorizon    ("Ночь, горизонт", Color) = (0.205, 0.22, 0.256, 1)
        _TwilightStrength ("Сила заката", Range(0, 1)) = 0.7
        _TwilightColor   ("Цвет заката", Color) = (1.00, 0.62, 0.38, 1)

        _TerrainSpeed    ("Скорость сдвига рельефа", Float) = 0.02
        _TerrainSeed     ("Сид рельефа", Float) = 0.0
        _TerrainBase     ("Базовая линия рельефа", Float) = 0.30
        _TerrainPixel    ("Размер пикселя рельефа, px", Float) = 5.0

        _ParallaxHills   ("Параллакс холмов", Float) = 0.025
        _ParallaxClouds  ("Параллакс облаков", Float) = 0.008

        _ParallaxZoomRef ("Опорный зум для параллакса", Range(0.5, 10)) = 2.5

        _HillFadeDepth   ("Глубина затемнения холма", Float) = 0.35
        _HillFadeColor   ("Цвет темноты холма", Color) = (0.05, 0.06, 0.10, 1)

        _NoiseOctaves    ("Октав шума рельефа", Float) = 4.0
        _NoiseGain       ("Затухание октав", Float) = 0.5
        _NoiseLacunarity ("Рост частоты октав", Float) = 2.02
        _RidgeDetailFreq ("Частота деталей силуэта", Float) = 2.3
        _RidgeDetailSeed ("Сид деталей силуэта", Float) = 1.7
        _FillFreq        ("Частота шума в холме", Float) = 2.0
        _FillSteps       ("Ступеней тона в холме", Float) = 4.0
        _FillDark        ("Тон холма, тень", Float) = 0.84
        _FillLight       ("Тон холма, свет", Float) = 1.16

        _HillNearHeight  ("Высота ближнего холма", Float) = 0.16
        _HillNearScale   ("Масштаб ближнего холма", Float) = 2.4
        _HillNearSpeed   ("Общий множитель скорости холма", Float) = 1.0
        _HillNearRough   ("Изломы ближнего холма", Float) = 0.45
        _HillNearColor   ("Цвет ближнего холма", Color) = (0.28, 0.36, 0.34, 1)

        _TreeDensity   ("Плотность деревьев", Range(0, 1)) = 0.5
        _TreeSectors   ("Секторов деревьев", Range(8, 128)) = 48
        _TreeOffset    ("Сдвиг высоты деревьев", Range(-0.3, 0.3)) = 0.0
        _TreeHeight    ("Высота деревьев", Float) = 0.06
        _TreeWidth     ("Ширина деревьев", Float) = 0.6
        _TreeDark      ("Темнота силуэта", Range(0, 1)) = 0.5
        _TreeSeed      ("Сид деревьев", Float) = 0.0

        _FrontBase     ("Базовая линия переднего холма", Float) = 0.22
        _FrontHeight   ("Высота переднего холма", Float) = 0.12
        _FrontScale    ("Масштаб переднего холма", Float) = 1.8
        _FrontRough    ("Изломы переднего холма", Float) = 0.4
        _FrontColor    ("Цвет переднего холма", Color) = (0.33, 0.40, 0.34, 1)
        _ParallaxFront ("Параллакс переднего холма", Float) = 0.04

        [Range(0, 2)] _HillBevel ("Бевол холмов: сила", Float) = 0.4
        [Range(1, 8)] _HillThick ("Бевол холмов: толщина, px", Float) = 1
        _HillBevelColor ("Бевол холмов: цвет", Color) = (1, 1, 1, 1)
        [Range(0, 2)] _TreeBevel ("Бевол деревьев: сила", Float) = 0.4
        [Range(1, 8)] _TreeThick ("Бевол деревьев: толщина, px", Float) = 1
        _TreeBevelColor ("Бевол деревьев: цвет", Color) = (1, 1, 1, 1)

        _RidgeNoiseSpeed ("Скорость шума силуэта, px/с", Float) = 6.0
        _FillNoiseSpeed  ("Скорость шума заливки, px/с", Float) = 3.0
        _FillNoiseDriftY ("Сдвиг заливки по вертикали, px/с", Float) = 1.5

        _CloudBase       ("Облака, низ", Color) = (0.55, 0.60, 0.68, 1)
        _CloudTop        ("Облака, верх", Color) = (1.00, 1.00, 1.00, 1)
        _CloudLight      ("Облака, свет", Color) = (1.00, 0.95, 0.82, 1)
        _CloudFar        ("Облака вдали", Color) = (0.22, 0.25, 0.33, 1)
        _FarDarken       ("Затемнение вдали", Range(0, 1)) = 0.75

        [HideInInspector] _MainTex ("MainTex (не используется)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {

            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
            "IgnoreProjector"= "True"
            "PreviewType"    = "Plane"
        }

        Pass
        {
            Name "CloudsUnlit"

            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "SkySun.hlsl"

            float _CloudScale;
            float _CloudDetail;
            float _CloudSpeed;
            float _CloudCoverage;
            float _CloudDensity;
            float _CloudHeight;
            float _CloudThickness;
            float _MarchSteps;
            float _LightAngle;
            float _UseGlobalSun;
            float4 _SkySunAzEl;
            float _LightScatter;
            half4  _SunColor;
            float  _SunIntensity;
            float  _SunSize;
            float  _SunGlow;
            float  _SunGlowSize;
            float  _SunScreenScale;
            float  _SunElevation;
            half4  _MoonColor;
            float  _MoonIntensity;
            float  _MoonSize;
            float  _MoonGlow;
            float  _MoonGlowSize;
            float  _BelowFade;
            float _FadeStart;
            float _FadeEnd;
            half4  _SkyZenith;
            half4  _SkyHorizon;
            float  _HorizonHeight;
            float  _PixelSize;
            float  _SkyBands;

            float  _NightStrength;
            half4  _NightAmbient;
            half4  _NightZenith;
            half4  _NightHorizon;
            float  _TwilightStrength;
            half4  _TwilightColor;

            float  _TerrainSpeed;
            float  _TerrainSeed;
            float  _TerrainBase;
            float  _TerrainPixel;

            float  _HillNearHeight;
            float  _HillNearScale;
            float  _HillNearSpeed;
            float  _HillNearRough;
            half4  _HillNearColor;

            float  _TreeDensity;
            float  _TreeSectors;
            float  _TreeOffset;
            float  _TreeHeight;
            float  _TreeWidth;
            float  _TreeDark;
            float  _TreeSeed;

            float  _FrontBase;
            float  _FrontHeight;
            float  _FrontScale;
            float  _FrontRough;
            half4  _FrontColor;
            float  _ParallaxFront;

            float  _HillBevel;
            float  _HillThick;
            half4  _HillBevelColor;
            float  _TreeBevel;
            float  _TreeThick;
            half4  _TreeBevelColor;

            float  _RidgeNoiseSpeed;
            float  _FillNoiseSpeed;
            float  _FillNoiseDriftY;

            float  _HillFadeDepth;
            half4  _HillFadeColor;

            float  _ParallaxHills;
            float  _ParallaxClouds;
            float  _ParallaxZoomRef;

            float  _NoiseOctaves;
            float  _NoiseGain;
            float  _NoiseLacunarity;
            float  _RidgeDetailFreq;
            float  _RidgeDetailSeed;
            float  _FillFreq;
            float  _FillSteps;
            float  _FillDark;
            float  _FillLight;

            half4  _CloudBase;
            half4  _CloudTop;
            half4  _CloudLight;
            half4  _CloudFar;
            float  _FarDarken;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            half4 fragColor : SV_Target;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            #define HASH_WRAP 65536.0

            float2 hash2(float2 p)
            {
                p = p - floor(p / HASH_WRAP) * HASH_WRAP;

                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                p = frac(p * (p.yx + float2(9.1, 0.1)));
                return p * 2.0 - 1.0;
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = dot(hash2(i), f);
                float b = dot(hash2(i + float2(1.0, 0.0)), f - float2(1.0, 0.0));
                float c = dot(hash2(i + float2(0.0, 1.0)), f - float2(0.0, 1.0));
                float d = dot(hash2(i + float2(1.0, 1.0)), f - float2(1.0, 1.0));

                return 0.5 + 0.5 * lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float rnd(float seed)
            {
                return frac(sin(seed * 12.9898) * 43758.5453);
            }

            float fbmTuned(float2 p, float seed, float octaves, float gain, float lacunarity)
            {
                int steps = (int)clamp(octaves, 1.0, 8.0);

                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float total = 0.0;

                for (int i = 0; i < 8; i++)
                {
                    if (i >= steps)
                        break;

                    float fi = (float)i;

                    float2 shift = float2(
                        rnd(fi * 127.1 + seed * 311.7) * 12.0,
                        rnd(fi * 269.5 + seed * 183.3) * 12.0
                    );

                    value += amplitude * noise(p * frequency + shift);
                    total += amplitude;

                    frequency *= max(lacunarity, 1.0);
                    amplitude *= max(gain, 0.0);
                }

                if (total <= 0.0)
                    return 0.0;

                return value / total * 0.9375;
            }

            float fbm(float2 p, float seed)
            {
                return fbmTuned(p, seed, 4.0, 0.5, 2.02);
            }

            float2 ParallaxOffset(float strength)
            {
                if (strength <= 0.0)
                    return float2(0.0, 0.0);

                const float PARALLAX_MAX_Y = 0.06;

                float2 cam = _WorldSpaceCameraPos.xy;

                float refOrtho = max(_ParallaxZoomRef, 0.0001);
                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);

                float2 worldToUv = float2(1.0 / (2.0 * refOrtho * aspect),
                                          1.0 / (2.0 * refOrtho));

                float2 offset = cam * strength * worldToUv;
                offset.y = clamp(offset.y, -PARALLAX_MAX_Y, PARALLAX_MAX_Y);

                return offset;
            }

            float cloudDensity(float3 pos, float time, float seed)
            {
                float2 uv = pos.xz / _CloudScale;
                uv += float2(time * _CloudSpeed * 0.08, time * _CloudSpeed * 0.03);

                uv += ParallaxOffset(_ParallaxClouds);

                float n1 = fbm(uv, seed);
                float n2 = fbm(uv * _CloudDetail + float2(100.0, -50.0), seed * 3.7);
                float n = lerp(n1, n2, 0.35);

                float h = (pos.y - _CloudHeight) / _CloudThickness;
                float profile = smoothstep(0.0, 0.25, h) * (1.0 - smoothstep(0.7, 1.0, h));

                float dist = length(pos);
                float fade = 1.0 - smoothstep(_FadeStart, _FadeEnd, dist);

                float d = smoothstep(_CloudCoverage, 1.0, n) * profile * _CloudDensity * fade;
                return max(d, 0.0);
            }

            float sunVisibility(float3 sunDir, float time)
            {

                float horizon = smoothstep(0.0, 0.15, sunDir.y);

                if (horizon <= 0.001)
                    return 1.0;

                float toTop = min((_CloudHeight + _CloudThickness) / sunDir.y, 24.0);

                int steps = 8;
                float stepLength = toTop / (float)steps;
                float visibility = 1.0;

                for (int i = 0; i < 8; i++)
                {
                    float t = stepLength * ((float)i + 0.5);
                    float d = cloudDensity(sunDir * t, time, 11.0);

                    if (d > 0.001)
                    {
                        float a = 1.0 - exp(-d * stepLength * 2.5);
                        visibility *= 1.0 - a;
                    }
                }

                return lerp(1.0, visibility, horizon);
            }

            float3 skyGradient(float height, float dayBlend)
            {
                float t = clamp(height, 0.0, 1.0);
                t = pow(t, 0.45);

                float bands = max(2.0, floor(_SkyBands));
                t = floor(t * bands) / (bands - 1.0);
                t = clamp(t, 0.0, 1.0);

                float3 day = lerp(_SkyHorizon.rgb, _SkyZenith.rgb, t);
                float3 night = lerp(_NightHorizon.rgb, _NightZenith.rgb, t);

                return lerp(night, day, clamp(dayBlend, 0.0, 1.0));
            }

            float RidgeLine(float2 p, float baseY, float amp, float scale, float rough, float seed, float drift, float rowStep)
            {
                float2 q = float2(p.x * scale + drift, seed);

                float n = fbmTuned(q, seed, _NoiseOctaves, _NoiseGain, _NoiseLacunarity);
                n = lerp(n, fbmTuned(q * _RidgeDetailFreq, seed * _RidgeDetailSeed, _NoiseOctaves, _NoiseGain, _NoiseLacunarity), rough);

                float y = baseY + (n - 0.5) * amp;

                return floor(y / rowStep) * rowStep;
            }

            float3 HillFill(float2 p, half3 tint, float scale, float seed, float driftX, float driftY)
            {
                float2 q = float2(p.x * scale + driftX, p.y * scale + driftY);

                float n = clamp(fbmTuned(q, seed, _NoiseOctaves, _NoiseGain, _NoiseLacunarity), 0.0, 1.0);

                float steps = max(_FillSteps, 1.0);
                if (steps > 2.0)
                    n = floor(n * (steps - 1.0) + 0.5) / (steps - 1.0);

                return tint * lerp(_FillDark, _FillLight, n);
            }

            float BelowLine(float v, float edge, float soft)
            {

                if (soft <= 0.0)
                    return step(v, edge);

                return smoothstep(edge + soft, edge - soft, v);
            }

            float3 FadeBelow(float3 c, float v, float edge, float depth)
            {
                if (depth <= 0.0)
                    return c;

                float t = saturate((edge - v) / depth);

                float steps = max(_FillSteps, 1.0);
                if (steps > 2.0)
                    t = floor(t * (steps - 1.0) + 0.5) / (steps - 1.0);

                return lerp(c, _HillFadeColor.rgb, t);
            }

            float2 TerrainGrid(float size)
            {
                return max(floor(_ScreenParams.xy / max(size, 1.0)), float2(1.0, 1.0));
            }

            float2 SnapPixel(float2 uv, float size)
            {
                float2 grid = TerrainGrid(size);
                return floor(uv * grid) / grid;
            }

            float PixelDrift(float time, float speedPx, float gridX)
            {
                if (gridX <= 0.0)
                    return 0.0;

                return time * speedPx / gridX;
            }

            float TerrainRow(float size)
            {
                float rows = max(floor(_ScreenParams.y / max(size, 1.0)), 1.0);
                return 1.0 / rows;
            }

            float3 HillBevel(float v, float stepY, float edge, float lineXm, float lineXp,
                             float aspect, float2 sunDir, float dayness,
                             float bevelAmount, half3 bevelColor)
            {
                if (bevelAmount <= 0.001)
                    return float3(0.0, 0.0, 0.0);

                float qXm = step(lineXm, v);
                float qXp = step(lineXp, v);
                float qYm = step(edge, v - stepY);
                float qYp = step(edge, v + stepY);

                float anyEdge = max(max(qXm, qXp), max(qYm, qYp));

                if (anyEdge <= 0.0)
                    return float3(0.0, 0.0, 0.0);

                float2 gradShape = float2(qXp - qXm, qYp - qYm);

                float2 gradWorld = float2(gradShape.x * aspect, gradShape.y);

                float2 normalW = gradWorld / max(length(gradWorld), 0.0001);

                float facing = saturate(dot(normalW, sunDir));

                float bevel = anyEdge * facing * bevelAmount * saturate(dayness);

                return bevelColor * bevel;
            }

            float TreeMask(float2 p, float drift, float row)
            {
                if (_TreeDensity <= 0.001 || _TreeSectors <= 1.0)
                    return 0.0;

                float sectors = max(2.0, floor(_TreeSectors));
                float baseCol = floor(p.x * sectors);
                float mask = 0.0;

                for (int d = -1; d <= 1; d++)
                {
                    float colIdx = baseCol + (float)d;

                    float r0 = frac(sin(colIdx * 127.1 + _TreeSeed * 17.0) * 43758.5453);
                    float r1 = frac(sin(colIdx * 311.7 + _TreeSeed * 9.0) * 43758.5453);
                    float r2 = frac(sin(colIdx * 269.5 + _TreeSeed * 3.0) * 43758.5453);
                    float r3 = frac(sin(colIdx * 183.3 + _TreeSeed * 29.0) * 43758.5453);

                    if (r0 >= _TreeDensity)
                        continue;

                    float cx = (colIdx + 0.5 + (r1 - 0.5) * 0.6) / sectors;

                    float baseY = RidgeLine(float2(cx, 0.0), _TerrainBase, _HillNearHeight, _HillNearScale, _HillNearRough, _TerrainSeed + 9.0, drift, row) + _TreeOffset;

                    float halfW = max(_TreeWidth, 0.001) * 0.02 * (0.7 + r2 * 0.9);

                    float h = max(_TreeHeight, 0.001) * (0.75 + r3 * 0.5);

                    float t = (p.y - baseY) / h;

                    if (t <= 0.0 || t > 1.0)
                        continue;

                    float halfWAt = max(halfW * lerp(1.0, 0.05, t), 0.001);

                    mask = max(mask, step(abs(p.x - cx), halfWAt));
                }

                return mask;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {

                float2 uv = input.screenPos.xy / input.screenPos.w;

                float aspect = _ScreenParams.x / max(_ScreenParams.y, 1.0);

                float camRoll = atan2(UNITY_MATRIX_V._m01, UNITY_MATRIX_V._m00);

                if (camRoll != 0.0)
                {
                    float2 pr = (uv - 0.5) * float2(aspect, 1.0);
                    float s = sin(camRoll);
                    float c = cos(camRoll);
                    pr = float2(pr.x * c - pr.y * s, pr.x * s + pr.y * c);
                    uv = pr / float2(aspect, 1.0) + 0.5;
                }

                float time = _Time.y;

                float3 ro = float3(0.0, 0.0, 0.0);

                float2 pixelUV = uv;

                if (_PixelSize > 1.0)
                {
                    float2 grid = max(floor(_ScreenParams.xy / max(_PixelSize, 1.0)), float2(1.0, 1.0));
                    pixelUV = floor(uv * grid) / grid;
                }

                float3 rd = normalize(float3(
                    (pixelUV.x * 2.0 - 1.0) * aspect * 0.6,
                    pixelUV.y * 2.0 - 1.0 + _HorizonHeight,
                    1.0
                ));

                float sunAzimuth = _LightAngle;
                float sunElev = _SunElevation;

                bool globalSun = _UseGlobalSun > 0.5 && _SkySunAzEl.w > 0.5;

                if (globalSun)
                {
                    sunAzimuth = _SkySunAzEl.x;
                    sunElev = _SkySunAzEl.y;
                }

                float3 sunDir = SkySunDir(sunAzimuth, sunElev);

                float dayness = globalSun ? _SkySunAzEl.z : smoothstep(-0.25, 0.35, sunDir.y);
                float nightAmt = (1.0 - dayness) * _NightStrength;
                float3 nightTint = lerp(float3(1.0, 1.0, 1.0), _NightAmbient.rgb, nightAmt);

                float twilight = (1.0 - smoothstep(0.0, 0.30, abs(sunDir.y))) * _TwilightStrength;

                float2 sunUV = float2(sunDir.x, sunDir.y) * 0.5 + 0.5;

                if (_UseGlobalSun > 0.5)
                    sunUV = 0.5 + (sunUV - 0.5) * _SunScreenScale;

                float sunDist = length((pixelUV - sunUV) * float2(aspect, 1.0));

                float disk = 1.0 - smoothstep(_SunSize * 0.65, _SunSize, sunDist);
                float halo = exp(-sunDist / max(_SunGlowSize, 0.0001));

                float3 moonDir = -sunDir;

                float2 moonUV = float2(moonDir.x, moonDir.y) * 0.5 + 0.5;

                if (_UseGlobalSun > 0.5)
                    moonUV = 0.5 + (moonUV - 0.5) * _SunScreenScale;

                float moonDist = length((pixelUV - moonUV) * float2(aspect, 1.0));

                float moonDisk = 1.0 - smoothstep(_MoonSize * 0.65, _MoonSize, moonDist);
                float moonHalo = exp(-moonDist / max(_MoonGlowSize, 0.0001));

                float skyHeight = rd.y * 0.5 + 0.5;
                float3 col = skyGradient(skyHeight, lerp(1.0, dayness, _NightStrength));

                float horizonMask = 1.0 - smoothstep(0.0, 0.42, skyHeight - 0.5);
                col = lerp(col, col * _TwilightColor.rgb, twilight * horizonMask);

                float t0 = (_CloudHeight - ro.y) / rd.y;
                float t1 = (_CloudHeight + _CloudThickness - ro.y) / rd.y;

                if (rd.y <= 0.001)
                {
                    t0 = -1.0;
                    t1 = -1.0;
                }

                float transmittance = 1.0;

                if (t1 > 0.0 && t0 > 0.0)
                {
                    int steps = max((int)_MarchSteps, 1);
                    float stepLength = (t1 - t0) / (float)steps;

                    float3 scattered = float3(0.0, 0.0, 0.0);

                    for (int i = 0; i < 64; i++)
                    {
                        if (i >= steps)
                            break;

                        float t = t0 + stepLength * ((float)i + 0.5);
                        float3 pos = ro + rd * t;

                        float seed = rnd((float)i + 1.0);
                        float d = cloudDensity(pos, time, seed);

                        if (d <= 0.001)
                            continue;

                        float3 lightPos = pos + sunDir * (_CloudThickness * 0.8);
                        float lightDens = cloudDensity(lightPos, time, seed + 0.5);
                        float light = exp(-lightDens * _LightScatter);

                        float3 ambient = lerp(_CloudBase.rgb, _CloudTop.rgb, light);
                        float3 lit = ambient + _CloudLight.rgb * light * 1.5;
                        lit = lerp(_CloudBase.rgb, lit, 0.7);

                        float haze = smoothstep(_FadeStart, _FadeEnd, length(pos)) * _FarDarken;
                        lit = lerp(lit, _CloudFar.rgb, haze);

                        lit *= nightTint;

                        float a = 1.0 - exp(-d * stepLength * 1.5);
                        scattered += transmittance * a * lit;
                        transmittance *= 1.0 - a;

                        if (transmittance < 0.01)
                            break;
                    }

                    col = col * transmittance + scattered;
                }

                float sunVis = sunVisibility(sunDir, time);

                float belowDepth = max(_BelowFade, 0.001);
                float sunUp = 1.0 - smoothstep(0.0, belowDepth, -sunDir.y);
                float moonUp = 1.0 - smoothstep(0.0, belowDepth, -moonDir.y);

                col += _SunColor.rgb * (_SunIntensity * disk + _SunGlow * halo) * sunVis * sunUp;

                float moonVis = sunVisibility(moonDir, time);

                col += _MoonColor.rgb * (_MoonIntensity * moonDisk + _MoonGlow * moonHalo) * moonVis * moonUp;

                float2 hillPar = ParallaxOffset(_ParallaxHills);
                float2 tp = uv + hillPar;
                float2 tpx = SnapPixel(uv + hillPar, _TerrainPixel);
                float row = TerrainRow(_TerrainPixel);
                float2 grid = TerrainGrid(_TerrainPixel);

                float2 px = float2(
                    max(1.0 / max(_ScreenParams.x, 1.0), 1e-6),
                    max(1.0 / max(_ScreenParams.y, 1.0), 1e-6)
                );

                float speedScale = _TerrainSpeed * _HillNearSpeed;

                float drift = PixelDrift(time, _RidgeNoiseSpeed * speedScale, grid.x);

                float nearLine = RidgeLine(tp, _TerrainBase, _HillNearHeight, _HillNearScale, _HillNearRough, _TerrainSeed + 9.0, drift, row);
                float nearMask = BelowLine(tp.y, nearLine, 0.0);

                float hth = max(_HillThick, 1.0);

                float nearLineXm = RidgeLine(tp + float2(-hth * px.x, 0.0), _TerrainBase, _HillNearHeight, _HillNearScale, _HillNearRough, _TerrainSeed + 9.0, drift, row);
                float nearLineXp = RidgeLine(tp + float2(hth * px.x, 0.0), _TerrainBase, _HillNearHeight, _HillNearScale, _HillNearRough, _TerrainSeed + 9.0, drift, row);

                float fillX = PixelDrift(time, _FillNoiseSpeed * speedScale, grid.x);
                float fillY = PixelDrift(time, _FillNoiseDriftY * speedScale, grid.y);
                float3 nearFill = HillFill(tpx, _HillNearColor.rgb, _HillNearScale * _FillFreq, _TerrainSeed + 31.0, fillX, fillY);
                nearFill = FadeBelow(nearFill, tpx.y, nearLine, _HillFadeDepth);
                nearFill *= nightTint;

                float treeMask = TreeMask(tpx, drift, row);

                if (treeMask > 0.0)
                    col = lerp(col, nearFill * _TreeDark, treeMask);

                float tth = max(_TreeThick, 1.0);

                float qXm = 1.0 - TreeMask(tpx - float2(tth * px.x, 0.0), drift, row);
                float qXp = 1.0 - TreeMask(tpx + float2(tth * px.x, 0.0), drift, row);
                float qYm = 1.0 - TreeMask(tpx - float2(0.0, tth * px.y), drift, row);
                float qYp = 1.0 - TreeMask(tpx + float2(0.0, tth * px.y), drift, row);

                float treeEdge = max(max(qXm, qXp), max(qYm, qYp)) * treeMask;

                if (_TreeBevel > 0.001 && treeEdge > 0.0)
                {
                    float2 tg = float2(qXp - qXm, qYp - qYm);

                    float2 tw = float2(tg.x * aspect, tg.y);

                    float2 tn = tw / max(length(tw), 0.0001);

                    float tf = saturate(dot(tn, sunDir.xy));

                    col += _TreeBevelColor.rgb * treeEdge * tf * _TreeBevel * saturate(dayness);
                }

                nearFill += HillBevel(tp.y, hth * px.y, nearLine, nearLineXm, nearLineXp,
                                      aspect, sunDir.xy, dayness, _HillBevel, _HillBevelColor.rgb);

                col = lerp(col, nearFill, nearMask);

                float2 frontPar = ParallaxOffset(_ParallaxFront);
                float2 ftp = uv + frontPar;
                float2 ftpx = SnapPixel(uv + frontPar, _TerrainPixel);

                float frontLine = RidgeLine(ftp, _FrontBase, _FrontHeight, _FrontScale, _FrontRough, _TerrainSeed + 53.0, drift, row);
                float frontMask = BelowLine(ftp.y, frontLine, 0.0);

                float frontLineXm = RidgeLine(ftp + float2(-hth * px.x, 0.0), _FrontBase, _FrontHeight, _FrontScale, _FrontRough, _TerrainSeed + 53.0, drift, row);
                float frontLineXp = RidgeLine(ftp + float2(hth * px.x, 0.0), _FrontBase, _FrontHeight, _FrontScale, _FrontRough, _TerrainSeed + 53.0, drift, row);

                float3 frontFill = HillFill(ftpx, _FrontColor.rgb, _FrontScale * _FillFreq, _TerrainSeed + 61.0, fillX, fillY);
                frontFill = FadeBelow(frontFill, ftpx.y, frontLine, _HillFadeDepth);
                frontFill *= nightTint;

                frontFill += HillBevel(ftp.y, hth * px.y, frontLine, frontLineXm, frontLineXp,
                                       aspect, sunDir.xy, dayness, _HillBevel, _HillBevelColor.rgb);

                col = lerp(col, frontFill, frontMask);

                fragColor = half4(col, 1.0);
                return fragColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}