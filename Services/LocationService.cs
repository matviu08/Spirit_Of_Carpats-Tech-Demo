using Raylib_cs;
using Spirit_Of_Carpats_Remake.Graphics;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System.Numerics;
using System;
using System.IO;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class LocationService : ILocationService
    {
        // ── Віртуальні розміри ────────────────────────────────────────────────
        private const int VW = 1377;
        private const int VH = 768;

        // ── Textures ──────────────────────────────────────────────────────────
        private Texture2D[] _walkAnimFrames = new Texture2D[19];
        private Texture2D _standingTexture;
        private Texture2D _jumpTexture;
        private Texture2D _forestBackground;
        private Texture2D _treeTexture;

        // ── Animation ─────────────────────────────────────────────────────────
        private enum AnimState { Idle, Walk, Jump, Fall }
        private AnimState _animState = AnimState.Idle;
        private AnimState _prevAnimState = AnimState.Idle;
        private int _currentFrame = 0;
        private float _animTimer = 0f;
        private const float AnimSpeed = 0.055f;
        private bool _isFacingRight = true;

        // ── Physics ───────────────────────────────────────────────────────────
        private PlayerPhysicsController _physics;
        private PhysicsSystem _physicsSystem = new PhysicsSystem();
        private Rectangle[] _solids;
        private Rectangle[] _platforms = Array.Empty<Rectangle>();

        // ── Camera ────────────────────────────────────────────────────────────
        private Camera2D _camera;
        private float _groundLevel;

        // ── Lighting ──────────────────────────────────────────────────────────
        private LightingSystem _lighting;
        private int _torchLightIdx = -1;

        public LocationService()
        {
            _forestBackground = LoadTexture("./Resurses/Img/location.png");
            _treeTexture = LoadTexture("./Resurses/Img/TreeTest.png");

            for (int i = 0; i < 19; i++)
                _walkAnimFrames[i] = LoadTexture($"./Resurses/Img/wolcking/walkingAnim{i + 1}.png");

            _standingTexture = LoadTexture("./Resurses/Img/standing.png");

            string jumpPath = "./Resurses/Img/jump.png";
            _jumpTexture = File.Exists(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resurses", "Img", "jump.png"))
                ? LoadTexture(jumpPath)
                : _standingTexture;

            _groundLevel = VH * 0.75f;

            _solids = new Rectangle[]
            {
                new Rectangle(-10000, _groundLevel, 20000, 1000),
                new Rectangle(-60, -10000, 60, 22000),
            };

            _physics = new PlayerPhysicsController(
                new Vector2(200, _groundLevel - 80f), 28f, 62f);

            _camera = new Camera2D
            {
                Target = new Vector2(_physics.Body.Position.X, _groundLevel),
                Offset = new Vector2(VW / 2f, VH),
                Rotation = 0f,
                Zoom = 1f,
            };

            _lighting = new LightingSystem();
            _lighting.SetAmbientTarget(0.42f);

            _torchLightIdx = _lighting.AddLight(
                new Vector2(VW / 2f, -VH * 0.6f),
                new Vector3(0.72f, 0.82f, 1.0f),
                radius: (float)VW * 1.8f,
                intensity: 1.2f
            );
        }

        public void Update(ref GameState state)
        {
            float dt = GetFrameTime();

            if (IsKeyPressed(KeyboardKey.Escape))
            {
                state = GameState.MainMenu;
                return;
            }

            _groundLevel = VH * 0.75f;
            _solids[0].Y = _groundLevel;
            _solids[0].X = -VW * 10f;
            _solids[0].Width = VW * 20f;

            int moveInput = 0;
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left)) { moveInput = -1; _isFacingRight = false; }
            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) { moveInput = 1; _isFacingRight = true; }

            bool jumpPressed = IsKeyPressed(KeyboardKey.Space) || IsKeyPressed(KeyboardKey.W);
            bool jumpHeld = IsKeyDown(KeyboardKey.Space) || IsKeyDown(KeyboardKey.W);

            _physics.Update(dt, moveInput, jumpPressed, jumpHeld,
                            _physicsSystem, _solids, _platforms);

            UpdateAnimation(moveInput, dt);
            _lighting.Update();

            if (IsKeyPressed(KeyboardKey.F))
                _lighting.FlashlightOn = !_lighting.FlashlightOn;

            float bodyHeight = _physics.Body.Height;
            _lighting.FlashlightPos = new Vector2(
                _physics.Body.Position.X - bodyHeight * 0.005f,
                _physics.Body.Position.Y - bodyHeight * 1f
            );
            _lighting.FlashlightDir = new Vector2(_isFacingRight ? 1f : -1f, 0.15f);

            float targetX = MathF.Max(_physics.Body.Position.X, VW / 2f);
            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) targetX += 110f;
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left)) targetX -= 110f;

            _camera.Target.X = Lerp(_camera.Target.X, targetX, 8f * dt);
            _camera.Target.Y = _groundLevel;
            _camera.Offset = new Vector2(VW / 2f, VH);
        }

        private void UpdateAnimation(int moveInput, float dt)
        {
            bool grounded = _physics.Body.IsGrounded;
            bool moving = moveInput != 0;
            bool falling = _physics.Body.Velocity.Y > 50f && !grounded;
            bool jumping = _physics.Body.Velocity.Y < -50f && !grounded;

            AnimState next;
            if (jumping) next = AnimState.Jump;
            else if (falling) next = AnimState.Fall;
            else if (moving && grounded) next = AnimState.Walk;
            else next = AnimState.Idle;

            if (next != _animState)
            {
                if (next == AnimState.Walk || next == AnimState.Idle)
                {
                    _currentFrame = 0;
                    _animTimer = 0f;
                }
                _animState = next;
            }

            if (_animState == AnimState.Walk)
            {
                _animTimer += dt;
                if (_animTimer >= AnimSpeed)
                {
                    _currentFrame = (_currentFrame + 1) % 19;
                    _animTimer = 0f;
                }
            }
        }

        public void CaptureScene()
        {
            _lighting.BeginSceneCapture();

            float bgScale = Math.Max((float)VW / _forestBackground.Width, (float)VH / _forestBackground.Height);
            float bgW = _forestBackground.Width * bgScale;
            float parallaxOffset = -(_camera.Target.X * 0.72f);
            int startTile = (int)Math.Floor(parallaxOffset / bgW);
            int repeatCount = (int)Math.Ceiling((float)VW / bgW) + 4;

            for (int i = startTile - 1; i < startTile + repeatCount + 1; i++)
                DrawTextureEx(_forestBackground,
                    new Vector2(parallaxOffset + i * bgW, 0), 0, bgScale, Color.White);

            BeginMode2D(_camera);

            float treeScale = VH * 0.00075f;
            float treeH = _treeTexture.Height * treeScale;
            float treeY = _groundLevel - treeH;
            int spacing = 580;
            float camLeft = _camera.Target.X - VW;
            float camRight = _camera.Target.X + VW;

            for (int i = (int)Math.Floor(camLeft / spacing) - 1;
                     i <= (int)Math.Ceiling(camRight / spacing) + 1; i++)
                DrawTextureEx(_treeTexture, new Vector2(i * spacing, treeY), 0, treeScale, Color.White);

            DrawPlayer((float)VH);

            EndMode2D();

            _lighting.EndSceneCapture();
        }

        // Цей метод ми викличемо ПЕРЕД головним рендером
        public void PrepareGraphics()
        {
            CaptureScene(); // Малюємо ліс і гравця
            _lighting.ProcessShaders(_camera); // Накладаємо світло
        }

        // А цей метод просто виведе готову картинку
        public void Draw(GameState state)
        {
            _lighting.DrawFinal();

            // Малюємо HUD поверх готового освітленого лісу
            DrawText("WASD — Move   Space — Jump   F — Flashlight", 10, 10, 18, Color.LightGray);
            DrawText(
                $"Grounded: {_physics.Body.IsGrounded}  " +
                $"Vel: {_physics.Body.Velocity.X:F0},{_physics.Body.Velocity.Y:F0}  " +
                $"Anim: {_animState}  FPS: {GetFPS()}  " +
                $"Light: {(_lighting.FlashlightOn ? "ON" : "OFF")}",
                10, 35, 16, Color.Yellow);
        }

        private void DrawPlayer(float sh)
        {
            Texture2D tex = _animState switch
            {
                AnimState.Walk => _walkAnimFrames[_currentFrame],
                AnimState.Jump => _jumpTexture,
                AnimState.Fall => _jumpTexture,
                _ => _standingTexture,
            };

            float targetH = sh * 0.17f;
            float sprScale = targetH / tex.Height;

            Rectangle src = new Rectangle(0, 0,
                _isFacingRight ? tex.Width : -tex.Width, tex.Height);
            Rectangle dst = new Rectangle(
                _physics.Body.Position.X,
                _physics.Body.Position.Y,
                tex.Width * sprScale,
                tex.Height * sprScale);

            DrawTexturePro(tex, src, dst,
                new Vector2(dst.Width / 2f, dst.Height), 0f, Color.White);
        }

        public void Unload()
        {
            _lighting.Dispose();
            UnloadTexture(_forestBackground);
            UnloadTexture(_treeTexture);
            UnloadTexture(_standingTexture);
            if (_jumpTexture.Id != _standingTexture.Id)
                UnloadTexture(_jumpTexture);
            for (int i = 0; i < 19; i++)
                UnloadTexture(_walkAnimFrames[i]);
        }
    }
}