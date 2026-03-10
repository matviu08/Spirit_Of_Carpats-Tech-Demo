using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Numerics;
using System.Collections.Generic;
using static Raylib_cs.Raylib;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class LocationService : ILocationService
    {
        private Texture2D _forestBackground;
        private Texture2D _treesTexture;
        private Texture2D _bushTexture;
        private Texture2D _cloud;
        private List<Tree> _trees;
        private Random _random;

        // Початкові розміри вікна при створенні позицій
        private int _initialScreenWidth;
        private int _initialScreenHeight;

        public LocationService()
        {
            _treesTexture = LoadTexture("./Resurses/Img/TreeTest.png");
            _forestBackground = LoadTexture(".\\Resurses\\Img\\location.png");
            _bushTexture = LoadTexture("./Resurses/Img/Bush.png");
            _cloud = LoadTexture(".\\Resurses\\Img\\cloud.png");
            

            _random = new Random();
            _trees = new List<Tree>();

            // Зберігаємо початкові розміри екрану, щоб масштабувати координати при зміні вікна
            _initialScreenWidth = GetScreenWidth();
            _initialScreenHeight = GetScreenHeight();

            //int treeCount = 15;

            //for (int i = 0; i < treeCount; i++)
            //{
            //    // Розміри дерев НЕ змінюємо — Scale розраховується один раз на старті
            //    float scaleX = (float)_initialScreenWidth / _treesTexture.Width / 5f;
            //    float scaleY = (float)_initialScreenHeight / _treesTexture.Height / 5f;
            //    float scale = Math.Min(scaleX, scaleY);

            //    // Позиція X у пікселях відносно початкового розміру вікна
            //    float percentX = (float)_random.NextDouble(); // 0..1
            //    float posX = percentX * _initialScreenWidth;

            //    // Y ставимо на низ початкового вікна (щоб зберегти "нижню" позицію)
            //    float posY = _initialScreenHeight / 2f;

            //    Tree tree = new Tree
            //    {
            //        Position = new Vector2(posX, posY), // зберігаємо у пікселях від початкового розміру
            //        Scale = scale,
            //        Rotation = 0f
            //    };

            //    _trees.Add(tree);
            //}
        }

        public void Draw(GameState state)
        {
            // Малюємо фон (адаптивно під поточний розмір екрану)
            float bgScaleX = (float)GetScreenWidth() / _forestBackground.Width;
            float bgScaleY = (float)GetScreenHeight() / _forestBackground.Height;
            float bgScale = Math.Max(bgScaleX, bgScaleY);

            float width = _forestBackground.Width * bgScale;
            float height = _forestBackground.Height * bgScale;

            float posX = (GetScreenWidth() - width) / 2f;
            float posY = (GetScreenHeight() - height) / 2f;

            float cloudScaleX = (float)GetScreenWidth() / _cloud.Width / 2f;
            float cloudScaleY = (float)GetScreenHeight() / _cloud.Height / 2f;
            float widthCloud = _cloud.Width * cloudScaleX / 4f;
            float heightCloud = _cloud.Height * cloudScaleY / 4f;

            float posXCloud = (GetScreenWidth() - widthCloud) / 2f;
            float posYCloud = (GetScreenHeight() - heightCloud) / 4f;


            DrawTexturePro(
                _forestBackground,
                new Rectangle(0, 0, _forestBackground.Width, _forestBackground.Height),
                new Rectangle(posX, posY, width, height),
                new Vector2(0, 0),
                0,
                Color.White
            );

            DrawTexturePro(
                _cloud,
                new Rectangle(0, 0, _cloud.Width, _cloud.Height),
                new Rectangle(posXCloud, posYCloud, widthCloud, heightCloud),
                new Vector2(0, 0),
                0,
                Color.White
            );
        }

        public void Update(ref GameState state)
        {
            if (IsKeyPressed(KeyboardKey.Escape))
            {
                state = GameState.MainMenu;
            }
        }

        public void Unload()
        {
            UnloadTexture(_forestBackground);
            UnloadTexture(_cloud);
            //UnloadTexture(_treesTexture);
            //UnloadTexture(_bushTexture);
        }
    }
}
