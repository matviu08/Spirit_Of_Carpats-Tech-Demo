using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Numerics;
using System.Collections.Generic;
using static Raylib_cs.Raylib;
using static Raylib_cs.Raymath;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class LocationService : ILocationService
    {
        private Texture2D _forestBackground;
        private Texture2D _cloud;

        const int G = 800; 
        const float PlayerJumpSpeed = 450.0f;
        const float PlayerHorSpeed = 300.0f;

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
                this.Rect = rect;
                this.Blocking = blocking;
                this.Color = color;
            }
        }

        private Player _player;
        private Camera2D _camera;
        private EnvItem[] _envItems;
        private int _cameraOption = 4; 

        public LocationService()
        {
            _forestBackground = LoadTexture("./Resurses/Img/location.png");
            _cloud = LoadTexture("./Resurses/Img/cloud.png");

            // Ініціалізація гравця
            _player = new Player();
            _player.Position = new Vector2(400, 200);
            _player.Speed = 0;
            _player.CanJump = false;

            // Ініціалізація світу (платформи)
            _envItems = new EnvItem[]
            {
                new EnvItem(new Rectangle(0, 400, 1000, 200), 1, Color.Gray),
                //new EnvItem(new Rectangle(300, 200, 400, 10), 1, Color.DarkGray),
                //new EnvItem(new Rectangle(250, 300, 100, 10), 1, Color.DarkGray),
                //new EnvItem(new Rectangle(650, 300, 100, 10), 1, Color.DarkGray)
            };

            // Налаштування камери
            _camera = new Camera2D();
            _camera.Target = _player.Position;
            _camera.Offset = new Vector2(GetScreenWidth() / 2, GetScreenHeight() / 2);
            _camera.Rotation = 0.0f;
            _camera.Zoom = 1.0f;
        }

        public void Update(ref GameState state)
        {
            float deltaTime = GetFrameTime();

            if (IsKeyPressed(KeyboardKey.Escape))
            {
                state = GameState.MainMenu;
                return;
            }

            UpdatePlayer(ref _player, _envItems, deltaTime);

            _camera.Zoom += (GetMouseWheelMove() * 0.05f);
            if (_camera.Zoom > 3.0f) _camera.Zoom = 3.0f;
            else if (_camera.Zoom < 0.25f) _camera.Zoom = 0.25f;

            if (IsKeyPressed(KeyboardKey.R))
            {
                _camera.Zoom = 1.0f;
                _player.Position = new Vector2(400, 200);
            }


            if (IsKeyPressed(KeyboardKey.C)) _cameraOption = (_cameraOption + 1) % 5;


            switch (_cameraOption)
            {
                case 0: UpdateCameraCenter(ref _camera, ref _player, GetScreenWidth(), GetScreenHeight()); break;
                case 1: UpdateCameraCenterInsideMap(ref _camera, ref _player, _envItems, GetScreenWidth(), GetScreenHeight()); break;
                case 2: UpdateCameraCenterSmoothFollow(ref _camera, ref _player, deltaTime, GetScreenWidth(), GetScreenHeight()); break;
                case 3: UpdateCameraEvenOutOnLanding(ref _camera, ref _player, deltaTime, GetScreenWidth(), GetScreenHeight()); break;
                case 4: UpdateCameraPlayerBoundsPush(ref _camera, ref _player, GetScreenWidth(), GetScreenHeight()); break;
            }
        }

        public void Draw(GameState state)
        {

            float bgScale = Math.Max((float)GetScreenWidth() / _forestBackground.Width, (float)GetScreenHeight() / _forestBackground.Height);
            DrawTextureEx(_forestBackground, Vector2.Zero, 0, bgScale, Color.White);


            DrawTextureEx(_cloud, new Vector2(100, 50), 0, 0.5f, Color.White);


            BeginMode2D(_camera);


            for (int i = 0; i < _envItems.Length; i++)
            {
                DrawRectangleRec(_envItems[i].Rect, _envItems[i].Color);
            }

            Rectangle playerRect = new(_player.Position.X - 20, _player.Position.Y - 40, 40, 40);
            DrawRectangleRec(playerRect, Color.Red);

            EndMode2D();

            DrawText($"Camera Mode: {_cameraOption}", 10, 10, 20, Color.Black);
            DrawText("Use WASD/Arrows to move, Space to Jump, C to change Camera", 10, 40, 20, Color.DarkGray);
        }

        private void UpdatePlayer(ref Player player, EnvItem[] envItems, float delta)
        {
            if (IsKeyDown(KeyboardKey.Left) || IsKeyDown(KeyboardKey.A)) player.Position.X -= PlayerHorSpeed * delta;
            if (IsKeyDown(KeyboardKey.Right) || IsKeyDown(KeyboardKey.D)) player.Position.X += PlayerHorSpeed * delta;

            if ((IsKeyDown(KeyboardKey.Space) || IsKeyDown(KeyboardKey.W)) && player.CanJump)
            {
                player.Speed = -PlayerJumpSpeed;
                player.CanJump = false;
            }

            bool hitObstacle = false;
            for (int i = 0; i < envItems.Length; i++)
            {
                EnvItem ei = envItems[i];
                if (ei.Blocking != 0 &&
                    ei.Rect.X <= player.Position.X &&
                    ei.Rect.X + ei.Rect.Width >= player.Position.X &&
                    ei.Rect.Y >= player.Position.Y &&
                    ei.Rect.Y <= player.Position.Y + player.Speed * delta)
                {
                    hitObstacle = true;
                    player.Speed = 0.0f;
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


        private void UpdateCameraCenter(ref Camera2D camera, ref Player player, int width, int height)
        {
            camera.Offset = new Vector2(width / 2.0f, height / 2.0f);
            camera.Target = player.Position;
        }

        private void UpdateCameraPlayerBoundsPush(ref Camera2D camera, ref Player player, int width, int height)
        {

            Vector2 bbox = new(0.2f, 0.2f);

            Vector2 bboxWorldMin = GetScreenToWorld2D(new Vector2((1 - bbox.X) * 0.5f * width, (1 - bbox.Y) * 0.5f * height), camera);
            Vector2 bboxWorldMax = GetScreenToWorld2D(new Vector2((1 + bbox.X) * 0.5f * width, (1 + bbox.Y) * 0.5f * height), camera);

            camera.Offset = new Vector2((1 - bbox.X) * 0.5f * width, (1 - bbox.Y) * 0.5f * height);

            if (player.Position.X < bboxWorldMin.X) camera.Target.X = player.Position.X;
            if (player.Position.Y < bboxWorldMin.Y) camera.Target.Y = player.Position.Y;
            if (player.Position.X > bboxWorldMax.X) camera.Target.X = bboxWorldMin.X + (player.Position.X - bboxWorldMax.X);
            if (player.Position.Y > bboxWorldMax.Y) camera.Target.Y = bboxWorldMin.Y + (player.Position.Y - bboxWorldMax.Y);
        }


        private void UpdateCameraCenterInsideMap(ref Camera2D camera, ref Player player, EnvItem[] envItems, int width, int height)
        {
            camera.Target = player.Position;
            camera.Offset = new Vector2(width / 2.0f, height / 2.0f);
            // Тут можна додати логіку обмеження краями мапи
        }

        private void UpdateCameraCenterSmoothFollow(ref Camera2D camera, ref Player player, float delta, int width, int height)
        {
            float minSpeed = 30;
            float fractionSpeed = 0.8f;
            camera.Offset = new Vector2(width / 2.0f, height / 2.0f);
            Vector2 diff = Vector2Subtract(player.Position, camera.Target);
            float length = Vector2Length(diff);
            if (length > 10)
            {
                float speed = Math.Max(fractionSpeed * length, minSpeed);
                camera.Target = Vector2Add(camera.Target, Vector2Scale(diff, speed * delta / length));
            }
        }

        private void UpdateCameraEvenOutOnLanding(ref Camera2D camera, ref Player player, float delta, int width, int height)
        {
            camera.Target.X = player.Position.X;
            camera.Offset = new Vector2(width / 2.0f, height / 2.0f);
            camera.Target.Y = Lerp(camera.Target.Y, player.Position.Y, 0.1f);
        }

        public void Unload()
        {
            UnloadTexture(_forestBackground);
            UnloadTexture(_cloud);
        }
    }
}