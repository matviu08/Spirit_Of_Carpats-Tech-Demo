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
        private Texture2D[] _walkAnimFrames = new Texture2D[19];
        private int _currentFrame = 0;
        private float _animTimer = 0f;
        private const float AnimSpeed = 0.05f;
        private bool _isFacingRight = true;
        private Texture2D _standingTexture;
        private bool _isMoving = false;

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
            for (int i = 0; i < 19; i++)
            {
                _walkAnimFrames[i] = LoadTexture($"./Resurses/Img/wolcking/walkingAnim{i + 1}.png");
            }
            _standingTexture = LoadTexture("./Resurses/Img/standing.png");
            //_cloud = LoadTexture("./Resurses/Img/cloud.png");

            _player = new Player
            {
                Position = new Vector2(400, 0),
                Speed = 0,
                CanJump = false
            };

            _envItems = new EnvItem[]
            {
                new EnvItem(
                    new Rectangle(-200, GetScreenHeight()/4, GetScreenWidth()*2f,100), 1,
                    Color.Gray)
            };

            MAP_WIDTH = _forestBackground.Width;
            MAP_HEIGHT = _forestBackground.Height;

            _camera = new Camera2D
            {
                Target = new Vector2(_player.Position.X, _player.Position.Y * 1.5f),
                Offset = new Vector2(GetScreenWidth() / 2f, GetScreenHeight() / 2.0f),
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

            //float bgScale = Math.Max(
            //        (float)GetScreenWidth() / _forestBackground.Width,
            //        (float)GetScreenHeight() / _forestBackground.Height);

            //Vector2 bgPos = new Vector2(0, 0);

            //int repeatCount = (int)Math.Ceiling(GetScreenWidth() / (_forestBackground.Width * bgScale)) + 1;

            //for (int i = 0; i < repeatCount; i++)
            //{
            //    Vector2 pos = new Vector2(
            //        bgPos.X + i * _forestBackground.Width * bgScale,
            //        bgPos.Y
            //    );

            //    DrawTextureEx(_forestBackground, pos, 0, bgScale, Color.White);
            //}

            int repeatCountX = (int)Math.Ceiling(GetScreenWidth() / (_forestBackground.Width * bgScale)) + 2;

            for(int i =0; i< repeatCountX; ++i)
            {
                Vector2 pos = new Vector2(
                    bgPos.X + i * _forestBackground.Width * bgScale,
                    bgPos.Y
                );
                DrawTextureEx(_forestBackground, pos, 0, bgScale, Color.White);
            }


            //DrawTextureEx(_forestBackground, bgPos, 0, bgScale, Color.White);

            DrawTextureEx(_cloud,
                new Vector2(-_camera.Target.X * 0.3f + 100, 50),
                0,
                0.5f,
                Color.White);

            BeginMode2D(_camera);

            foreach (var env in _envItems)
            {
                DrawRectangleRec(env.Rect, env.Color);
            }

            Texture2D currentTex;

            if (_isMoving && _player.CanJump)
            {
                currentTex = _walkAnimFrames[_currentFrame];
            }
            else
            {
                currentTex = _standingTexture;
            }

            Rectangle sourceRec = new Rectangle(0, 0, currentTex.Width, currentTex.Height);

            if (!_isFacingRight)
            {
                sourceRec.Width *= -1;
            }

            float targetHeight = 160f;
            float scale = targetHeight / currentTex.Height;

            Rectangle destRec = new Rectangle(
                _player.Position.X,
                _player.Position.Y,
                currentTex.Width * scale,
                currentTex.Height * scale 
            );

            Vector2 origin = new Vector2(destRec.Width / 2, destRec.Height);

            DrawTexturePro(currentTex, sourceRec, destRec, origin, 0.0f, Color.White);

            EndMode2D();

            DrawText("WASD / Arrows - Move", 10, 10, 20, Color.Black);
            DrawText("Space - Jump", 10, 40, 20, Color.DarkGray);

            // туман!! Димка на екрані
            DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(Color.LightGray, 0.1f));
        }

        private void UpdatePlayer(ref Player player, EnvItem[] envItems, float delta)
        {
            _isMoving = false;

            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left))
            {
                player.Position.X -= PlayerHorSpeed * delta;
                _isFacingRight = false;
                _isMoving = true;
            }

            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right))
            {
                player.Position.X += PlayerHorSpeed * delta;
                _isFacingRight = true;
                _isMoving = true;
            }

            if ((IsKeyPressed(KeyboardKey.Space) || IsKeyPressed(KeyboardKey.W)) && player.CanJump)
            {
                player.Speed = -PlayerJumpSpeed;
                player.CanJump = false;
            }

            if (_isMoving && player.CanJump)
            {
                _animTimer += delta;
                if (_animTimer >= AnimSpeed)
                {
                    _currentFrame++;
                    if (_currentFrame >= 19) _currentFrame = 0;
                    _animTimer = 0f;
                }
            }
            else
            {
                _currentFrame = 0;
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
                    player.Position.Y = ei.Rect.Y ;
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
                camera.Target.Y = Lerp(camera.Target.Y , player.Position.Y / GetScreenHeight() / 153.6f, 0.08f);
        }

        public void Unload()
        {
            UnloadTexture(_forestBackground);
            UnloadTexture(_cloud);
            for (int i = 0; i < 19; i++)
            {
                UnloadTexture(_walkAnimFrames[i]);
            }
            UnloadTexture(_standingTexture);
        }
    }
}