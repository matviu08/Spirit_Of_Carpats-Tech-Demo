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
        private Texture2D _treeTexture;

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

        private float GroundLevel => GetScreenHeight() * 0.85f;
        public LocationService()
        {
            _forestBackground = LoadTexture("./Resurses/Img/location.png");
            _treeTexture = LoadTexture("./Resurses/Img/TreeTest.png");

            for (int i = 0; i < 19; i++)
            {
                _walkAnimFrames[i] = LoadTexture($"./Resurses/Img/wolcking/walkingAnim{i + 1}.png");
            }
            _standingTexture = LoadTexture("./Resurses/Img/standing.png");
            //_cloud = LoadTexture("./Resurses/Img/cloud.png");

            _player = new Player
            {
                Position = new Vector2(400, 100), 
                Speed = 0,
                CanJump = false
            };

            _envItems = new EnvItem[]
            {
                new EnvItem(new Rectangle(-10000, GroundLevel, 20000, 1000), 1, Color.Blank),
                new EnvItem(new Rectangle(-50, -10000, 50, 20000), 1, Color.Blank),
            };

            _player.Position = new Vector2(100, GroundLevel - 160f);
            _envItems[0].Rect.Y = GetScreenHeight() * 0.85f;

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
            _envItems[0].Rect.Y = GetScreenHeight() * 0.75f;
            _envItems[0].Rect.X = -GetScreenWidth() * 10f;
            _envItems[0].Rect.Width = GetScreenWidth() * 20f;
            _envItems[1].Rect.X = -50;
            UpdatePlayer(ref _player, _envItems, dt);

            UpdateCameraPlatformer(ref _camera, ref _player, dt);

            
        }



        public void Draw(GameState state)
        {
            float screenW = GetScreenWidth();
            float screenH = GetScreenHeight();

            float bgScale = Math.Max(screenW / _forestBackground.Width, screenH / _forestBackground.Height);
            float bgW = _forestBackground.Width * bgScale;

            // Smooth parallax — no modulo snapping
            float parallaxOffset = -(_camera.Target.X * 0.75f);

            // How many tiles needed to cover screen
            int repeatCountX = (int)Math.Ceiling(screenW / bgW) + 4;

            // Find the starting tile index based on camera position
            int startTile = (int)Math.Floor(parallaxOffset / bgW);

            for (int i = startTile - 1; i < startTile + repeatCountX + 1; i++)
            {
                DrawTextureEx(_forestBackground,
                    new Vector2(parallaxOffset + i * bgW, 0),
                    0, bgScale, Color.White);
            }

            // 2. World objects — inside camera mode
            BeginMode2D(_camera);

            foreach (var env in _envItems)
                DrawRectangleRec(env.Rect, env.Color);

            float treeScale = screenH * 0.0008f; // smaller scale
            float treeH = _treeTexture.Height * treeScale;
            float treeY = GroundLevel - treeH; // sits on ground in world space

            // Only draw trees visible near camera (culling)
            float camLeft = _camera.Target.X - screenW;
            float camRight = _camera.Target.X + screenW;

            int treeSpacing = 600;
            int firstTree = (int)Math.Floor(camLeft / treeSpacing) - 1;
            int lastTree = (int)Math.Ceiling(camRight / treeSpacing) + 1;

            for (int i = firstTree; i <= lastTree; i++)
            {
                float treeX = i * treeSpacing;
                DrawTextureEx(_treeTexture, new Vector2(treeX, treeY), 0, treeScale, Color.White);
            }

            // 3. Player — inside camera mode
            Texture2D currentTex = (_isMoving && _player.CanJump) ? _walkAnimFrames[_currentFrame] : _standingTexture;

            Rectangle sourceRec = new Rectangle(0, 0, currentTex.Width, currentTex.Height);
            if (!_isFacingRight) sourceRec.Width *= -1;

            float targetHeight = screenH * 0.17f; 
            float scale = targetHeight / currentTex.Height;

            Rectangle destRec = new Rectangle(
                _player.Position.X,
                _player.Position.Y,
                currentTex.Width * scale,
                currentTex.Height * scale
            );

            DrawTexturePro(currentTex, sourceRec, destRec, new Vector2(destRec.Width / 2, destRec.Height), 0f, Color.White);

            EndMode2D();

            // 4. UI — screen space
            DrawRectangle(0, 0, (int)screenW, (int)screenH, Fade(Color.LightGray, 0.1f));
            DrawText("WASD / Arrows - Move", 10, 10, 20, Color.Black);
            DrawText("Space - Jump", 10, 40, 20, Color.DarkGray);
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
            if (player.Position.X < 0)
                player.Position.X = 0;

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
            float targetX;
            if (player.Position.X >= GetScreenWidth() / 2)
            {
                targetX = player.Position.X;

            }
            else
            {
                targetX = GetScreenWidth() / 2;

            }
            if (IsKeyDown(KeyboardKey.D) || IsKeyDown(KeyboardKey.Right)) targetX += lookAhead;
            if (IsKeyDown(KeyboardKey.A) || IsKeyDown(KeyboardKey.Left)) targetX -= lookAhead;

            camera.Target.X = Lerp(camera.Target.X, targetX, 0.1f);

            camera.Target.Y = GroundLevel;

            camera.Offset = new Vector2(GetScreenWidth() / 2f, GetScreenHeight());
        }

        public void Unload()
        {
            UnloadTexture(_forestBackground);
            UnloadTexture(_cloud);
            UnloadTexture(_treeTexture);
            for (int i = 0; i < 19; i++)
            {
                UnloadTexture(_walkAnimFrames[i]);
            }
            UnloadTexture(_standingTexture);
        }
    }
}