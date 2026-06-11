#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform sampler2D texture0;
uniform vec2  screenSize;
uniform float time;

uniform vec3  fogColor;
uniform float fogStrength;
uniform float fogSpeed;

uniform float aberrationStrength;

out vec4 finalColor;

float hash(vec2 p)
{
    p = fract(p * vec2(127.1, 311.7));
    p += dot(p, p + 19.19);
    return fract(p.x * p.y);
}

float noise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(
        mix(hash(i),                hash(i + vec2(1,0)), u.x),
        mix(hash(i + vec2(0,1)),    hash(i + vec2(1,1)), u.x),
        u.y
    );
}

float fbm(vec2 p)
{
    float v = 0.0, a = 0.5, f = 1.0;
    for (int i = 0; i < 5; i++)
    {
        v += a * noise(p * f);
        a *= 0.5;
        f *= 2.07;
    }
    return v;
}

void main()
{
    vec2 uv = fragTexCoord;

    // Хроматична аберація — радіальна від центру
    vec4 texel;
    if (aberrationStrength > 0.001)
    {
        vec2  center = uv - 0.5;
        float dist   = length(center);
        vec2  offset = normalize(center + vec2(0.001)) * dist * aberrationStrength;

        float r = texture(texture0, clamp(uv + offset, 0.0, 1.0)).r;
        float g = texture(texture0, uv).g;
        float b = texture(texture0, clamp(uv - offset, 0.0, 1.0)).b;
        texel   = vec4(r, g, b, texture(texture0, uv).a);
    }
    else
    {
        texel = texture(texture0, uv);
    }
    texel *= fragColor;

    // Туман — три шари з різною швидкістю і масштабом
    // Шар 1: великі повільні клуби
    vec2 uv1   = uv * vec2(2.5, 1.8) + vec2(time * fogSpeed, time * fogSpeed * 0.25);
    float fog1 = fbm(uv1);

    // Шар 2: дрібніші швидші завихрення
    vec2 uv2   = uv * vec2(4.0, 2.5) + vec2(-time * fogSpeed * 0.7, time * fogSpeed * 0.4);
    float fog2 = fbm(uv2);

    // Шар 3: дуже дрібна текстура для деталей
    vec2 uv3   = uv * vec2(8.0, 5.0) + vec2(time * fogSpeed * 1.2, -time * fogSpeed * 0.3);
    float fog3 = fbm(uv3) * 0.4;

    float fogRaw = fog1 * 0.55 + fog2 * 0.35 + fog3 * 0.10;

    // Туман щільніший у нижній частині екрану (земля)
    // і майже зникає вгорі (небо)
    float heightGrad = pow(clamp(uv.y, 0.0, 1.0), 1.2);

    // Туман появляється плавно від горизонту
    float horizonBand = smoothstep(0.35, 0.65, uv.y) * (1.0 - smoothstep(0.65, 1.0, uv.y));
    float groundFog   = smoothstep(0.55, 1.0, uv.y);

    float fogMask = mix(horizonBand * 0.4, groundFog, 0.7);

    float fogVal = fogRaw * fogStrength * fogMask;
    fogVal       = clamp(fogVal, 0.0, 0.68);

    // Колір туману трохи варіюється — темніший у западинах, світліший на гребенях
    vec3 deepFog  = fogColor * 0.7;
    vec3 lightFog = fogColor * 1.4;
    vec3 fogBlend = mix(deepFog, lightFog, clamp(fogRaw * 1.5, 0.0, 1.0));

    vec3 result = mix(texel.rgb, fogBlend, fogVal);

    finalColor = vec4(result, texel.a);
}
