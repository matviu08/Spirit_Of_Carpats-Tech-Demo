using Spirit_Of_Carpats_Remake.Models;
using Raylib_cs;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace Spirit_Of_Carpats_Remake.Graphics
{
    public struct PointLight
    {
        public Vector2 Position;
        public Vector3 Color;
        public float   Radius;
        public float   Intensity;
        public bool    Active;

        public static PointLight Create(Vector2 pos, Vector3 color, float radius, float intensity = 1f)
            => new PointLight { Position = pos, Color = color, Radius = radius, Intensity = intensity, Active = true };
    }

    public class LightingSystem : IDisposable
    {
        private Shader _lightShader;
        private Shader _fogShader;

        private RenderTexture2D _sceneTarget;
        private RenderTexture2D _fogTarget;

        // Lighting uniforms
        private int _uScreenSize;
        private int _uAmbientLight;
        private int _uAmbientColor;
        private int _uLightCount;
        private int _uLightPos;
        private int _uLightColor;
        private int _uLightRadius;
        private int _uLightIntensity;
        private int _uFlashlightOn;
        private int _uFlashlightPos;
        private int _uFlashlightDir;
        private int _uFlashlightAngle;
        private int _uFlashlightRadius;
        private int _uFlashlightColor;
        private int _uVignetteStrength;
        private int _uLightTime;

        // Fog uniforms
        private int _uFogScreenSize;
        private int _uFogTime;
        private int _uFogColor;
        private int _uFogStrength;
        private int _uFogSpeed;
        private int _uAberration;

        private const int MaxLights = 8;
        private PointLight[] _lights = new PointLight[MaxLights];
        private int _lightCount = 0;

        // Flashlight
        public bool    FlashlightOn     = false;
        public Vector2 FlashlightPos    = Vector2.Zero;
        public Vector2 FlashlightDir    = new Vector2(1, 0);
        public float   FlashlightAngle  = 0.42f;
        public float   FlashlightRadius = 380f;
        public Vector3 FlashlightColor  = new Vector3(0.92f, 0.88f, 0.72f);

        // Ambient — місячне нічне освітлення
        public float   AmbientLight     = 0.42f;
        public Vector3 AmbientColor     = new Vector3(0.48f, 0.67f, 0.62f);
        public float   VignetteStrength = 0.48f;

        // Fog
        public float   FogStrength  = 0.55f;
        public float   FogSpeed     = 0.022f;
        public Vector3 FogColor     = new Vector3(0.06f, 0.09f, 0.16f);
        public float   Aberration   = 0.0f;

        private float _ambientTarget;
        private float _time     = 0f;
        private bool  _disposed = false;
        private bool  _shadersLoaded = false;

        private Camera2D _camera;

        public LightingSystem()
        {
            _ambientTarget = AmbientLight;
            Reload();
        }

        public void Reload()
        {
            int w = GetScreenWidth();
            int h = GetScreenHeight();

            if (_sceneTarget.Id != 0) UnloadRenderTexture(_sceneTarget);
            if (_fogTarget.Id   != 0) UnloadRenderTexture(_fogTarget);

            _sceneTarget = LoadRenderTexture(w, h);
            _fogTarget   = LoadRenderTexture(w, h);

            string baseDir    = AppDomain.CurrentDomain.BaseDirectory;
            string shadersDir = Path.Combine(baseDir, "Resurses", "Shaders") + Path.DirectorySeparatorChar;

            string vertPath  = shadersDir + "lighting.vert";
            string lightFrag = shadersDir + "lighting.frag";
            string fogFrag   = shadersDir + "fog.frag";

            bool shadersExist = File.Exists(vertPath) && File.Exists(lightFrag) && File.Exists(fogFrag);
            Console.WriteLine($"[LightingSystem] Shaders path: {shadersDir}");
            Console.WriteLine($"[LightingSystem] Shaders found: {shadersExist}");

            if (shadersExist)
            {
                if (_shadersLoaded)
                {
                    UnloadShader(_lightShader);
                    UnloadShader(_fogShader);
                }
                _lightShader   = LoadShader(vertPath, lightFrag);
                _fogShader     = LoadShader(vertPath, fogFrag);
                _shadersLoaded = true;
                CacheUniformLocations();
            }
            else
            {
                Console.WriteLine("[LightingSystem] WARNING: Shaders not found, running without lighting effects.");
                _shadersLoaded = false;
            }
        }

        public int AddLight(Vector2 worldPos, Vector3 color, float radius, float intensity = 1f)
        {
            if (_lightCount >= MaxLights) return -1;
            _lights[_lightCount] = PointLight.Create(worldPos, color, radius, intensity);
            return _lightCount++;
        }

        public void UpdateLight(int index, Vector2 worldPos)
        {
            if (index >= 0 && index < _lightCount)
                _lights[index].Position = worldPos;
        }

        public void ClearLights() => _lightCount = 0;

        public void BeginSceneCapture()
        {
            BeginTextureMode(_sceneTarget);
            ClearBackground(Color.Black);
        }

        public void EndSceneCapture() => EndTextureMode();

        public void RenderToScreen(Camera2D camera)
        {
            _camera = camera;
            _time  += GetFrameTime();

            int sw = GetScreenWidth();
            int sh = GetScreenHeight();

            if (!_shadersLoaded)
            {
                DrawTextureRec(_sceneTarget.Texture,
                    new Rectangle(0, 0, sw, -sh), Vector2.Zero, Color.White);
                return;
            }

            // Pass 1: Lighting → _fogTarget
            BeginTextureMode(_fogTarget);
            ClearBackground(Color.Black);
            SetLightingUniforms(sw, sh);
            BeginShaderMode(_lightShader);
            DrawTextureRec(_sceneTarget.Texture,
                new Rectangle(0, 0, sw, -sh), Vector2.Zero, Color.White);
            EndShaderMode();
            EndTextureMode();

            // Pass 2: Fog → screen
            SetFogUniforms(sw, sh);
            BeginShaderMode(_fogShader);
            DrawTextureRec(_fogTarget.Texture,
                new Rectangle(0, 0, sw, -sh), Vector2.Zero, Color.White);
            EndShaderMode();
        }

        public void TriggerHorrorPulse()
        {
            Aberration   = 0.007f;
            AmbientLight = MathF.Max(0.02f, AmbientLight - 0.10f);
        }

        public void Update()
        {
            float dt = GetFrameTime();
            Aberration   = MathHelper.Lerp(Aberration,   0f,             dt * 3.5f);
            AmbientLight = MathHelper.Lerp(AmbientLight, _ambientTarget, dt * 0.4f);
        }

        public void SetAmbientTarget(float value) => _ambientTarget = Math.Clamp(value, 0f, 1f);

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (_shadersLoaded)
            {
                UnloadShader(_lightShader);
                UnloadShader(_fogShader);
            }
            UnloadRenderTexture(_sceneTarget);
            UnloadRenderTexture(_fogTarget);
        }

        private void CacheUniformLocations()
        {
            _uScreenSize       = GetShaderLocation(_lightShader, "screenSize");
            _uAmbientLight     = GetShaderLocation(_lightShader, "ambientLight");
            _uAmbientColor     = GetShaderLocation(_lightShader, "ambientColor");
            _uLightCount       = GetShaderLocation(_lightShader, "lightCount");
            _uLightPos         = GetShaderLocation(_lightShader, "lightPos");
            _uLightColor       = GetShaderLocation(_lightShader, "lightColor");
            _uLightRadius      = GetShaderLocation(_lightShader, "lightRadius");
            _uLightIntensity   = GetShaderLocation(_lightShader, "lightIntensity");
            _uFlashlightOn     = GetShaderLocation(_lightShader, "flashlightOn");
            _uFlashlightPos    = GetShaderLocation(_lightShader, "flashlightPos");
            _uFlashlightDir    = GetShaderLocation(_lightShader, "flashlightDir");
            _uFlashlightAngle  = GetShaderLocation(_lightShader, "flashlightAngle");
            _uFlashlightRadius = GetShaderLocation(_lightShader, "flashlightRadius");
            _uFlashlightColor  = GetShaderLocation(_lightShader, "flashlightColor");
            _uVignetteStrength = GetShaderLocation(_lightShader, "vignetteStrength");
            _uLightTime        = GetShaderLocation(_lightShader, "time");

            _uFogScreenSize = GetShaderLocation(_fogShader, "screenSize");
            _uFogTime       = GetShaderLocation(_fogShader, "time");
            _uFogColor      = GetShaderLocation(_fogShader, "fogColor");
            _uFogStrength   = GetShaderLocation(_fogShader, "fogStrength");
            _uFogSpeed      = GetShaderLocation(_fogShader, "fogSpeed");
            _uAberration    = GetShaderLocation(_fogShader, "aberrationStrength");
        }

        private void SetLightingUniforms(int sw, int sh)
        {
            SetShaderValue(_lightShader, _uScreenSize,
                new Vector2(sw, sh), ShaderUniformDataType.Vec2);
            SetShaderValue(_lightShader, _uAmbientLight,
                AmbientLight, ShaderUniformDataType.Float);
            SetShaderValue(_lightShader, _uAmbientColor,
                AmbientColor, ShaderUniformDataType.Vec3);
            SetShaderValue(_lightShader, _uVignetteStrength,
                VignetteStrength, ShaderUniformDataType.Float);
            SetShaderValue(_lightShader, _uLightCount,
                _lightCount, ShaderUniformDataType.Int);
            SetShaderValue(_lightShader, _uLightTime,
                _time, ShaderUniformDataType.Float);

            if (_lightCount > 0)
            {
                var positions   = new float[MaxLights * 2];
                var colors      = new float[MaxLights * 3];
                var radii       = new float[MaxLights];
                var intensities = new float[MaxLights];

                for (int i = 0; i < _lightCount; i++)
                {
                    Vector2 screen       = GetWorldToScreen2D(_lights[i].Position, _camera);
                    positions[i * 2]     = screen.X;
                    positions[i * 2] = sh - screen.Y;  // фліп Y: RenderTexture зберігається знизу вгору
                    colors[i * 3]        = _lights[i].Color.X;
                    colors[i * 3 + 1]    = _lights[i].Color.Y;
                    colors[i * 3 + 2]    = _lights[i].Color.Z;
                    radii[i]             = _lights[i].Radius * _camera.Zoom;
                    intensities[i]       = _lights[i].Intensity;
                }

                unsafe
                {
                    fixed (float* p = positions)
                        SetShaderValueV(_lightShader, _uLightPos, p, ShaderUniformDataType.Vec2, _lightCount);
                    fixed (float* p = colors)
                        SetShaderValueV(_lightShader, _uLightColor, p, ShaderUniformDataType.Vec3, _lightCount);
                    fixed (float* p = radii)
                        SetShaderValueV(_lightShader, _uLightRadius, p, ShaderUniformDataType.Float, _lightCount);
                    fixed (float* p = intensities)
                        SetShaderValueV(_lightShader, _uLightIntensity, p, ShaderUniformDataType.Float, _lightCount);
                }
            }

            int flashInt = FlashlightOn ? 1 : 0;
            SetShaderValue(_lightShader, _uFlashlightOn, flashInt, ShaderUniformDataType.Int);

            if (FlashlightOn)
            {
                Vector2 screenPos = GetWorldToScreen2D(FlashlightPos, _camera);
                // RenderTexture зберігається знизу вгору — фліпуємо Y
                Vector2 screenPosFlipped = new Vector2(screenPos.X, sh - screenPos.Y);
                // Напрямок: Y також треба інвертувати для відповідності системі координат шейдера
                Vector2 dirFlipped = new Vector2(FlashlightDir.X, -FlashlightDir.Y);
                SetShaderValue(_lightShader, _uFlashlightPos,   screenPosFlipped, ShaderUniformDataType.Vec2);
                SetShaderValue(_lightShader, _uFlashlightDir,   dirFlipped,       ShaderUniformDataType.Vec2);
                SetShaderValue(_lightShader, _uFlashlightAngle, FlashlightAngle,  ShaderUniformDataType.Float);
                SetShaderValue(_lightShader, _uFlashlightRadius,
                    FlashlightRadius * _camera.Zoom, ShaderUniformDataType.Float);
                SetShaderValue(_lightShader, _uFlashlightColor, FlashlightColor,  ShaderUniformDataType.Vec3);
            }
        }

        private void SetFogUniforms(int sw, int sh)
        {
            SetShaderValue(_fogShader, _uFogScreenSize, new Vector2(sw, sh), ShaderUniformDataType.Vec2);
            SetShaderValue(_fogShader, _uFogTime,       _time,               ShaderUniformDataType.Float);
            SetShaderValue(_fogShader, _uFogColor,      FogColor,            ShaderUniformDataType.Vec3);
            SetShaderValue(_fogShader, _uFogStrength,   FogStrength,         ShaderUniformDataType.Float);
            SetShaderValue(_fogShader, _uFogSpeed,      FogSpeed,            ShaderUniformDataType.Float);
            SetShaderValue(_fogShader, _uAberration,    Aberration,          ShaderUniformDataType.Float);
        }
    }
}
