using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Numerics;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class LocationService : ILocationService
    {
        private Texture2D _forestBackground;
        private Texture2D _cloud;

        const int G = 800;
        const float PlayerJumpSpeed = 450f;
        const float PlayerHorSpeed = 300f;

        private struct Player
        {
            public Vector2 Position;
            public float Speed;
            public bool CanJump;
        }

        private struct EnvItem
        {
            public Rectangle Rect;
            public int Blocking;
            public Color Color;

            public EnvItem(Rectangle rect, int blocking, Color color)
            {
                Rect = rect;
                Blocking = blocking;
                Color = color;
            }
        }

        private Player _player;
        private Camera2D _camera;
        private EnvItem[] _envItems;

        private int MAP_WIDTH;
        private int MAP_HEIGHT;

        public LocationService()
        {
            _forestBackground = LoadTexture("./Resurses/Img/location.png");
            _cloud = LoadTexture("./Resurses/Img/cloud.png");

            _player = new Player
            {
                Position = new Vector2(400, 0),
                Speed = 0,
                CanJump = false
            };

            _envItems = new EnvItem[]
            {
                new EnvItem(
                    new Rectangle(-200, GetScreenHeight()/2f, GetScreenWidth()*2f,100),
                    1,
                    Color.Blank)
            };

            MAP_WIDTH = _forestBackground.Width;
            MAP_HEIGHT = _forestBackground.Height;

            _camera = new Camera2D
            {
                Target = new Vector2(_player.Position.X, _player.Position.Y),
                Offset = new Vector2(GetScreenWidth() / 2f, GetScreenHeight() / 2f),
                Rotation = 0,
                Zoom = 1
            };
        }

        public void Update(ref GameState state)
        {
            float dt = GetFrameTime();

            if (IsKeyPressed(KeyboardKey.Escape))
            {
                state = GameState.MainMenu;
                return;
            }

            UpdatePlayer(ref _player, _envItems, dt);

            UpdateCameraPlatformer(ref _camera, ref _player, dt);

            ClampCamera();
        }

        private void ClampCamera()
        {
            float halfW = GetScreenWidth() / 2f;
            float halfH = GetScreenHeight() / 2f;

            _camera.Target.X = Math.Clamp(_camera.Target.X, halfW, MAP_WIDTH - halfW);
            _camera.Target.Y = Math.Clamp(_camera.Target.Y, halfH, MAP_HEIGHT - halfH);
        }

        public void Draw(GameState state)
        {
            float bgScale = Math.Max(
                (float)GetScreenWidth() / _forestBackground.Width,
                (float)GetScreenHeight() / _forestBackground.Height);

            Vector2 bgPos = new Vector2(
                -_camera.Target.X * 0.2f,
                -_camera.Target.Y * 0.2f
            );

            DrawTextureEx(_forestBackground, bgPos, 0, bgScale, Color.White);

            DrawTextureEx(_cloud,
                new Vector2(-_camera.Target.X * 0.3f + 100, 50),
                0,
                0.5f,
                Color.White);

            BeginMode2D(_camera);

            foreach (var env in _envItems)
                DrawRectangleRec(env.Rect, env.Color);

            Rectangle playerRect =
                new Rectangle(_player.Position.X - 20, _player.Position.Y - 40, 40, 40);

            DrawRectangleRec(playerRect, Color.Red);

            EndMode2D();

            DrawText("WASD / Arrows - Move", 10, 10, 20, Color.Black);
            DrawText("Space - Jump", 10, 40, 20, Color.DarkGray);
        }

        private void UpdatePlayer(ref Player player, EnvItem[] envItems, float delta)
        {
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left))
                player.Position.X -= PlayerHorSpeed * delta;

            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right))
                player.Position.X += PlayerHorSpeed * delta;

            if ((IsKeyPressed(KeyboardKey.Space) || IsKeyPressed(KeyboardKey.W)) && player.CanJump)
            {
                player.Speed = -PlayerJumpSpeed;
                player.CanJump = false;
            }

            bool hitObstacle = false;

            foreach (var ei in envItems)
            {
                if (ei.Blocking != 0 &&
                    ei.Rect.X <= player.Position.X &&
                    ei.Rect.X + ei.Rect.Width >= player.Position.X &&
                    ei.Rect.Y >= player.Position.Y &&
                    ei.Rect.Y <= player.Position.Y + player.Speed * delta)
                {
                    hitObstacle = true;
                    player.Speed = 0;
                    player.Position.Y = ei.Rect.Y;
                    break;
                }
            }

            if (!hitObstacle)
            {
                player.Position.Y += player.Speed * delta;
                player.Speed += G * delta;
                player.CanJump = false;
            }
            else
            {
                player.CanJump = true;
            }
        }

        private void UpdateCameraPlatformer(ref Camera2D camera, ref Player player, float dt)
        {
            float lookAhead = 120;

            float targetX = player.Position.X;

            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right))
                targetX += lookAhead;

            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left))
                targetX -= lookAhead;

            camera.Target.X = Lerp(camera.Target.X, targetX, 0.1f);

            if (player.CanJump)
                camera.Target.Y = Lerp(camera.Target.Y, player.Position.Y, 0.08f);
        }

        public void Unload()
        {
            UnloadTexture(_forestBackground);
            UnloadTexture(_cloud);
        }
    }
}