#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec2  screenSize;

uniform float ambientLight;
uniform vec3  ambientColor;

uniform int   lightCount;
uniform vec2  lightPos[8];
uniform vec3  lightColor[8];
uniform float lightRadius[8];
uniform float lightIntensity[8];

uniform int   flashlightOn;
uniform vec2  flashlightPos;
uniform vec2  flashlightDir;
uniform float flashlightAngle;
uniform float flashlightRadius;
uniform vec3  flashlightColor;

uniform float vignetteStrength;
uniform float time;

out vec4 finalColor;

// Плавний шум для мерехтіння
float hash(float n) { return fract(sin(n) * 43758.5453); }

float flicker(float t, float seed)
{
    float a = hash(floor(t * 12.0) + seed);
    float b = hash(floor(t * 12.0) + seed + 1.0);
    float f = fract(t * 12.0);
    f = f * f * (3.0 - 2.0 * f);
    return 0.88 + 0.12 * mix(a, b, f);
}

// Квадратичне затухання з м'яким краєм
float pointLight(vec2 pixel, vec2 pos, float radius)
{
    float d = length(pixel - pos);
    if (d >= radius) return 0.0;
    float t = 1.0 - (d / radius);
    // кубічне — більш "теплий" центр і різкіший край
    return t * t * t;
}

// Більш реалістичне затухання на основі фізики
float pointLightPhysical(vec2 pixel, vec2 pos, float radius, float intensity)
{
    float d = length(pixel - pos);
    if (d >= radius) return 0.0;
    // Зворотна квадратична + soft cutoff
    float atten = intensity / (1.0 + 0.01 * d + 0.0003 * d * d);
    float edge  = smoothstep(radius, radius * 0.7, d);
    return atten * edge;
}

// Конус ліхтаря з м'якими краями і внутрішнім ядром
float coneLight(vec2 pixel, vec2 pos, vec2 dir, float halfAngle, float radius)
{
    vec2  toPixel = pixel - pos;
    float dist    = length(toPixel);
    if (dist > radius) return 0.0;

    vec2  normDir   = normalize(dir);
    vec2  normPixel = toPixel / max(dist, 0.001);
    float cosAngle  = dot(normPixel, normDir);

    float outerCos = cos(halfAngle);
    float innerCos = cos(halfAngle * 0.4);

    if (cosAngle < outerCos) return 0.0;

    float coneFade = smoothstep(outerCos, innerCos, cosAngle);
    // Кубічне — більш виразне ядро
    coneFade = coneFade * coneFade;

    // Фізичне затухання по дистанції
    float distFade = 1.0 / (1.0 + 0.004 * dist + 0.00001 * dist * dist);
    distFade *= smoothstep(radius, radius * 0.1, dist);

    return coneFade * distFade;
}

// Перетворення в теплий колір вогню
vec3 fireColor(vec3 baseColor, float heat)
{
    // Додаємо трохи жовтизни в центрі полум'я
    vec3 hotSpot = vec3(1.0, 0.95, 0.4);
    return mix(baseColor, hotSpot, heat * 0.3);
}

void main()
{
    vec2 pixel = fragTexCoord * screenSize;
    vec4 texel = texture(texture0, fragTexCoord) * fragColor;

    // Місячне ambient — холодне синьо-фіолетове
    vec3 totalLight = ambientColor * ambientLight;

    // Точкові джерела (вогні, факели)
    for (int i = 0; i < lightCount; i++)
    {
        // Мерехтіння для кожного вогню з різним seed
        float flick = flicker(time, float(i) * 7.3);

        float att = pointLightPhysical(pixel, lightPos[i], lightRadius[i], lightIntensity[i]);

        // Близько до джерела — тепліший колір
        float dist    = length(pixel - lightPos[i]);
        float heat    = 1.0 - clamp(dist / (lightRadius[i] * 0.3), 0.0, 1.0);
        vec3  fColor  = fireColor(lightColor[i], heat);

        totalLight += fColor * att * flick;
    }

    // Ліхтар гравця
    if (flashlightOn != 0)
    {
        float flick = flicker(time, 99.0) * 0.04 + 0.96; // дуже слабке мерехтіння
        float cone  = coneLight(pixel, flashlightPos, flashlightDir,
                                flashlightAngle, flashlightRadius);

        // Невелике розсіяне світло навколо самого гравця (ореол)
        float halo  = pointLight(pixel, flashlightPos, flashlightRadius * 0.18) * 0.35;

        totalLight += flashlightColor * (cone + halo) * flick;
    }

    // Vignette з плавним градієнтом
    vec2  uv      = fragTexCoord * 2.0 - 1.0;
    float vigDist = dot(uv * vec2(0.9, 1.1), uv * vec2(0.9, 1.1)); // еліпс
    float vig     = 1.0 - vignetteStrength * vigDist;
    vig           = smoothstep(0.0, 1.0, vig);
    totalLight   *= vig;

    // Clamp перед tone-mapping
    totalLight = max(totalLight, vec3(0.0));

    // Filmic tone-mapping (Reinhard розширений)
    // Краще зберігає кольори при пересвіті
    vec3 mapped = (totalLight * (1.0 + totalLight / 1.8)) / (totalLight + 1.0);

    // Легка корекція гами для тепліших тіней
    mapped = pow(mapped, vec3(0.95, 0.97, 1.02));

    // Підйом тіней — щоб темні ділянки не були чисто чорними
    float darkness = 1.0 - clamp(length(totalLight) / 1.73, 0.0, 1.0);
    mapped += vec3(0.012, 0.018, 0.028) * darkness;

    finalColor = vec4(texel.rgb * mapped, texel.a);
}
