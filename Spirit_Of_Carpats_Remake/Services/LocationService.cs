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
        private List<Tree> _trees;
        private Random _random;

        // Початкові розміри вікна при створенні позицій
        private int _initialScreenWidth;
        private int _initialScreenHeight;

        public LocationService()
        {
            _treesTexture = LoadTexture("./Resurses/Img/TreeTest.png");
            _forestBackground = LoadTexture("./Resurses/Img/WoodBackground.png");
            _bushTexture = LoadTexture("./Resurses/Img/Bush.png");

            _random = new Random();
            _trees = new List<Tree>();

            // Зберігаємо початкові розміри екрану, щоб масштабувати координати при зміні вікна
            _initialScreenWidth = GetScreenWidth();
            _initialScreenHeight = GetScreenHeight();

            int treeCount = 15;

            for (int i = 0; i < treeCount; i++)
            {
                // Розміри дерев НЕ змінюємо — Scale розраховується один раз на старті
                float scaleX = (float)_initialScreenWidth / _treesTexture.Width / 5f;
                float scaleY = (float)_initialScreenHeight / _treesTexture.Height / 5f;
                float scale = Math.Min(scaleX, scaleY);

                // Позиція X у пікселях відносно початкового розміру вікна
                float percentX = (float)_random.NextDouble(); // 0..1
                float posX = percentX * _initialScreenWidth;

                // Y ставимо на низ початкового вікна (щоб зберегти "нижню" позицію)
                float posY = _initialScreenHeight / 2f;

                Tree tree = new Tree
                {
                    Position = new Vector2(posX, posY), // зберігаємо у пікселях від початкового розміру
                    Scale = scale,
                    Rotation = 0f
                };

                _trees.Add(tree);
            }
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

            DrawTexturePro(
                _forestBackground,
                new Rectangle(0, 0, _forestBackground.Width, _forestBackground.Height),
                new Rectangle(posX, posY, width, height),
                new Vector2(0, 0),
                0,
                Color.White
            );

            // Малюємо дерева: адаптуємо X під поточну ширину вікна, Y — завжди низ вікна (нижній край дерева = низ вікна)
            foreach (var tree in _trees)
            {
                Rectangle source = new Rectangle(0, 0, _treesTexture.Width, _treesTexture.Height);

                float destWidth = _treesTexture.Width * tree.Scale;
                float destHeight = _treesTexture.Height * tree.Scale;

                // Масштабування X від початкового розміру до поточного
                float scaledX = tree.Position.X * (float)GetScreenWidth() / _initialScreenWidth;

                // Обмежуємо X, щоб дерево не вийшло за межі екрану
                scaledX = MathF.Max(0f, MathF.Min(scaledX, GetScreenWidth() - destWidth));

                // Встановлюємо верхній лівий кут dest так, щоб низ дерева точно співпадав з низом вікна
                Rectangle dest = new Rectangle(
                    scaledX,
                    GetScreenHeight(), // встановлюємо нижню межу дерева на низ екрану
                    destWidth,
                    destHeight
                );

                // origin: центр по X і низ по Y
                Vector2 origin = new Vector2(destWidth / 2f, 0f); // нижній край дерева

                DrawTexturePro(_treesTexture, source, dest, origin, tree.Rotation, Color.White);
            }
            // кущ (залишаємо як було, стоїть на низу вікна)
            float bushPosX = GetScreenWidth() * 0.05f; // 5% від ширини
            float bushPosY = GetScreenHeight() - _bushTexture.Height;
            DrawTexture(_bushTexture, (int)bushPosX, (int)bushPosY, Color.White);
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
            UnloadTexture(_treesTexture);
            UnloadTexture(_bushTexture);
        }
    }
}
