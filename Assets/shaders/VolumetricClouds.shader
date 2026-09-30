Shader "Diesel/VolumetricClouds"
{
    /* Облака рейтрейсом по объёму, с затуханием по глубине и шумом на слой.
       Рисуется на полноэкранном кваде. */

    Properties
    {
        [Header(Geometry)]
        _CloudScale      ("Масштаб облаков", Float) = 1.0
        _CloudDetail     ("Детализация", Float) = 1.7
        _CloudSpeed      ("Скорость ветра", Float) = 1.0
        _CloudHeight     ("Высота слоя", Float) = 1.0
        _CloudThickness  ("Толщина слоя", Float) = 2.0

        [Header(Shape)]
        _CloudCoverage   ("Плотность покрытия", Range(0, 1)) = 0.45
        _CloudDensity    ("Плотность облаков", Float) = 1.0
        _MarchSteps      ("Шагов марча", Range(8, 64)) = 32

        [Header(Sun)]
        _SunColor       ("Цвет солнца", Color) = (1.00, 0.94, 0.78, 1)
        _SunIntensity   ("Яркость диска", Range(0, 8)) = 3.0
        _SunSize        ("Размер диска", Range(0.002, 0.30)) = 0.05
        _SunGlow        ("Свечение вокруг", Range(0, 3)) = 0.5
        _SunGlowSize    ("Ширина свечения", Range(0.02, 1.50)) = 0.40
        _SunElevation   ("Высота над горизонтом", Range(-0.5, 1.5)) = 0.6

        [Header(Light)]
        _LightAngle      ("Азимут солнца", Float) = 0.6
        _LightScatter    ("Рассеивание", Range(0, 4)) = 1.2

        [Header(Fade)]
        _FadeStart       ("Начало затухания", Float) = 12.0
        _FadeEnd         ("Конец затухания", Float) = 30.0

        [Header(Pixel)]
        _PixelSize       ("Размер пикселя, экранных px", Range(1, 16)) = 4.0
        _SkyBands        ("Полос неба", Range(2, 128)) = 128.0

        [Header(Sky)]
        _SkyZenith       ("Небо, зенит", Color) = (0.16, 0.35, 0.62, 1)
        _SkyHorizon      ("Небо, горизонт", Color) = (0.72, 0.82, 0.90, 1)
        _HorizonHeight   ("Высота горизонта", Range(-1, 1)) = 0.4

        [Header(Clouds)]
        _CloudBase       ("Облака, низ", Color) = (0.55, 0.60, 0.68, 1)
        _CloudTop        ("Облака, верх", Color) = (1.00, 1.00, 1.00, 1)
        _CloudLight      ("Облака, свет", Color) = (1.00, 0.95, 0.82, 1)
        _CloudFar        ("Облака вдали", Color) = (0.22, 0.25, 0.33, 1)
        _FarDarken       ("Затемнение вдали", Range(0, 1)) = 0.75

        /* UGUI требует у любого материала на Image текстуру с таким именем и без
           неё ругается "Material does not have a _MainTex or _BaseMap".
           Шейдер эту текстуру не читает: небо рисуется процедурно, спрайт нужен
           только как геометрия. Поле оставлено ради батчера канваса. */
        [HideInInspector] _MainTex ("MainTex (не используется)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            /* Transparent, а не Background. В Background материале с ZWrite Off
               небо рисовалось в непрозрачном проходе, то есть ДО скайбокса,
               и камера затирала его поверх: глубины шейдер не оставлял. */
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"          = "Transparent"
            "IgnoreProjector"= "True"
            "PreviewType"    = "Plane"
        }

        Pass
        {
            Name "CloudsUnlit"

            /* SRPDefaultUnlit, а не UniversalForward. UGUI рисует Image отдельным
               проходом DrawUI, и он берёт только объекты с этим тегом. С
               UniversalForward проход молча не выполнялся: шейдер не отвечал, и
               на экране не было вообще ничего, даже градиента неба. */
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

            float _CloudScale;
            float _CloudDetail;
            float _CloudSpeed;
            float _CloudCoverage;
            float _CloudDensity;
            float _CloudHeight;
            float _CloudThickness;
            float _MarchSteps;
            float _LightAngle;
            float _LightScatter;
            half4  _SunColor;
            float  _SunIntensity;
            float  _SunSize;
            float  _SunGlow;
            float  _SunGlowSize;
            float  _SunElevation;
            float _FadeStart;
            float _FadeEnd;
            half4  _SkyZenith;
            half4  _SkyHorizon;
            float  _HorizonHeight;
            float  _PixelSize;
            float  _SkyBands;
            half4  _CloudBase;
            half4  _CloudTop;
            half4  _CloudLight;
            half4  _CloudFar;
            float  _FarDarken;

            /* Объявление нужно не только ради свойства материала: батчер UGUI
               ищет сэмплер по имени и без объявления откажется группировать
               графику. Саму текстуру шейдер не читает. */
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

            /* Хеш на вектор, чтобы у градиентного шума не было сетки. */
            float2 hash2(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                p = frac(p * (p.yx + float2(9.1, 0.1)));
                return p * 2.0 - 1.0;
            }

            /* Градиентный шум: значения в углах шумные, между ними плавно. */
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

            /* Псевдослучайное из числа. Тоже используется как сид. */
            float rnd(float seed)
            {
                return frac(sin(seed * 12.9898) * 43758.5453);
            }

            /* Фрактальный шум. У каждой октавы свой сдвиг, иначе все октавы
               повторяли бы один и тот же узор и читались бы как полосы. */
            float fbm(float2 p, float seed)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < 4; i++)
                {
                    float fi = (float)i;
                    float2 shift = float2(
                        rnd(fi * 127.1 + seed * 311.7) * 100.0,
                        rnd(fi * 269.5 + seed * 183.3) * 100.0
                    );

                    value += amplitude * noise(p * frequency + shift);
                    frequency *= 2.02;
                    amplitude *= 0.5;
                }

                return value;
            }

            /* Плотность облака в точке: два шума, профиль по высоте и затухание. */
            float cloudDensity(float3 pos, float time, float seed)
            {
                float2 uv = pos.xz / _CloudScale;
                uv += float2(time * _CloudSpeed * 0.08, time * _CloudSpeed * 0.03);

                float n1 = fbm(uv, seed);
                float n2 = fbm(uv * _CloudDetail + float2(100.0, -50.0), seed * 3.7);
                float n = lerp(n1, n2, 0.35);

                // Профиль по вертикали: облака снизу и сверху разряжаются.
                float h = (pos.y - _CloudHeight) / _CloudThickness;
                float profile = smoothstep(0.0, 0.25, h) * (1.0 - smoothstep(0.7, 1.0, h));

                float dist = length(pos);
                float fade = 1.0 - smoothstep(_FadeStart, _FadeEnd, dist);

                float d = smoothstep(_CloudCoverage, 1.0, n) * profile * _CloudDensity * fade;
                return max(d, 0.0);
            }

            /* Облака перед солнцем. Идём от камеры вверх по направлению на солнце
               и копим плотность до верхней границы слоя: выше облаков пусто и идти
               дальше незачем.

               Считаем именно по лучу к солнцу, а не по направлению взгляда. Раньше
               диск гасился прозрачностью вдоль взгляда, но облако перед диском
               находится на другом луче и в пиксель самого солнца не попадает,
               из-за чего солнце просвечивало сквозь слой. */
            float sunVisibility(float3 sunDir, float time)
            {
                if (sunDir.y <= 0.001)
                    return 1.0;

                float toTop = (_CloudHeight + _CloudThickness) / sunDir.y;

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

                return visibility;
            }

            /* Небо градиентом от горизонта к зениту. */
            float3 skyGradient(float height)
            {
                float t = clamp(height, 0.0, 1.0);
                t = pow(t, 0.45);

                /* Ступени вместо плавного перехода. Сетка экрана даёт пиксельные
                   блоки, но если внутри блока цвет всё равно плавно меняется, небо
                   выходит обычным градиентом и пиксельным не читается. Поэтому
                   квантуем и градиент: _SkyBands ступеней на всю высоту неба. */
                float bands = max(2.0, floor(_SkyBands));
                t = floor(t * bands) / (bands - 1.0);

                return lerp(_SkyHorizon.rgb, _SkyZenith.rgb, clamp(t, 0.0, 1.0));
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

            /* Идём по лучу сквозь слой облаков и копим рассеянный свет.
               Работает в экранных координатах, поэтому квад не растягивается:
               пропорции берутся из самого экрана, а не из UV квада. */
            half4 Frag(Varyings input) : SV_Target
            {
                /* Экранные координаты берём из ComputeScreenPos, а не делим
                   SV_POSITION на _ScreenParams вручную. Начало отсчёта у
                   SV_POSITION зависит от бэкенда: на D3D оно от левого верхнего
                   угла, а когда URP рисует в перевёрнутый target — от нижнего.
                   Ручной переворот uv.y = 1 - uv.y был верен только на первом
                   случае и переворачивал небо на втором. ComputeScreenPos
                   учитывает и положение target'а, и знак проекции, поэтому
                   v = 1 всегда означает верх кадра на любой платформе. */
                float2 uv = input.screenPos.xy / input.screenPos.w;

                float aspect = _ScreenParams.x / _ScreenParams.y;

                float time = _Time.y;

                float3 ro = float3(0.0, 0.0, 0.0);

                /* Пиксельная сетка. Считаем блоки от размера экрана, а не от
                   uv, иначе блок плыл бы при смене разрешения. Сетка одна и та же
                   для неба и солнца, иначе они крошились бы в разные квадраты и
                   картинка рассыпалась бы. */
                float2 pixelUV = uv;

                if (_PixelSize > 1.0)
                {
                    float2 grid = max(floor(_ScreenParams.xy / max(_PixelSize, 1.0)), float2(1.0, 1.0));
                    pixelUV = floor(uv * grid) / grid;
                }

                // Умножение на aspect держит угол обзора по обеим осям
                // одинаковым. Без него картинка растянулась бы на широком экране.
                // _HorizonHeight сдвигает горизонт вверх или вниз кадра.
                float3 rd = normalize(float3(
                    (pixelUV.x * 2.0 - 1.0) * aspect * 0.6,
                    pixelUV.y * 2.0 - 1.0 + _HorizonHeight,
                    1.0
                ));

                /* Азимут и высота задают одно направление, из которого потом берутся и
                   подсветка облаков, и экранная позиция диска. Если считать их
                   отдельно, солнце будет светить не с той стороны, откуда
                   подсвечены облака. */
                float sunAzimuth = _LightAngle;
                float sunElev = _SunElevation;

                float3 sunDir = normalize(float3(
                    cos(sunElev) * cos(sunAzimuth),
                    sin(sunElev),
                    cos(sunElev) * sin(sunAzimuth)));

                // Экранная точка солнца: x и y того же направления в диапазон uv.
                float2 sunUV = float2(sunDir.x, sunDir.y) * 0.5 + 0.5;

                // По x домножаем на aspect, иначе на широком экране круглый диск
                // растягивается в овал.
                float sunDist = length((pixelUV - sunUV) * float2(aspect, 1.0));

                // Мягкий край вместо резкой границы: настоящий диск не обрывается
                // по контуру, и ступенька на нём сразу читается как круглая наклейка.
                float disk = 1.0 - smoothstep(_SunSize * 0.65, _SunSize, sunDist);
                float halo = exp(-sunDist / max(_SunGlowSize, 0.0001));

                float3 col = skyGradient(rd.y * 0.5 + 0.5);

                // Где луч входит в слой и где выходит.
                float t0 = (_CloudHeight - ro.y) / rd.y;
                float t1 = (_CloudHeight + _CloudThickness - ro.y) / rd.y;

                // Луч смотрит вниз или параллельно слою: пересечения нет.
                if (rd.y <= 0.001)
                {
                    t0 = -1.0;
                    t1 = -1.0;
                }

                /* Прозрачность слоя нужна и после марча, поэтому объявляем её здесь, а
                   не внутри блока: ниже ею притушивается диск солнца. */
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

                        // Шаг со сдвигом на половину, чтобы не попадать в границы слоя.
                        float t = t0 + stepLength * ((float)i + 0.5);
                        float3 pos = ro + rd * t;

                        float seed = rnd((float)i + 1.0);
                        float d = cloudDensity(pos, time, seed);

                        if (d <= 0.001)
                            continue;

                        // Пробуем дойти до солнца: если там плотно, свет гаснет.
                        float3 lightPos = pos + sunDir * (_CloudThickness * 0.8);
                        float lightDens = cloudDensity(lightPos, time, seed + 0.5);
                        float light = exp(-lightDens * _LightScatter);

                        float3 ambient = lerp(_CloudBase.rgb, _CloudTop.rgb, light);
                        float3 lit = ambient + _CloudLight.rgb * light * 1.5;
                        lit = lerp(_CloudBase.rgb, lit, 0.7);

                        // Дальние облака уходят в тень, а не растворяются.
                        // Затухание плотности само по себе делало дальний край
                        // светлее: облако исчезало, сквозь него просвечивало
                        // светлое небо, и горизонт выходил ярче ближних облаков.
                        float haze = smoothstep(_FadeStart, _FadeEnd, length(pos)) * _FarDarken;
                        lit = lerp(lit, _CloudFar.rgb, haze);

                        float a = 1.0 - exp(-d * stepLength * 1.5);
                        scattered += transmittance * a * lit;
                        transmittance *= 1.0 - a;

                        // Дальше идти бессмысленно: сквозь плотное облако уже не видно.
                        if (transmittance < 0.01)
                            break;
                    }

                    col = col * transmittance + scattered;
                }

                /* Солнце живёт под облаками, поэтому гасим и диск, и гало тем, что
                   оказалось между камерой и солнцем. Значение одно на весь экран:
                   направление на солнце от пикселя к пикселю не меняется. */
                float sunVis = sunVisibility(sunDir, time);

                col += _SunColor.rgb * (_SunIntensity * disk + _SunGlow * halo) * sunVis;

                fragColor = half4(col, 1.0);
                return fragColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}