using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

/// це тестовий код бля тому не вийобуйтесь
namespace SnakeGame
{
    // Використовуємо класи або структури для сутностей
    public struct Snake
    {
        public Vector2 Position;
        public Vector2 Size;
        public Vector2 Speed;
        public Color Color;
    }

    public struct Food
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool Active;
        public Color Color;
    }

    class Program
    {
        // Константи та налаштування
        const int SnakeLength = 256;
        const int SquareSize = 31;
        const int ScreenWidth = 800;
        const int ScreenHeight = 450;

        static int framesCounter = 0;
        static bool gameOver = false;
        static bool pause = false;

        static Food fruit = new Food();
        static Snake[] snake = new Snake[SnakeLength];
        static Vector2[] snakePosition = new Vector2[SnakeLength];
        static bool allowMove = false;
        static Vector2 offset = new Vector2();
        static int counterTail = 0;

        static void Main()
        {
            // Ініціалізація вікна
            InitWindow(ScreenWidth, ScreenHeight, "classic game: snake (C# version)");

            InitGame();

            SetTargetFPS(60);

            // Головний ігровий цикл
            while (!WindowShouldClose())
            {
                UpdateDrawFrame();
            }

            CloseWindow();
        }

        static void InitGame()
        {
            framesCounter = 0;
            gameOver = false;
            pause = false;
            counterTail = 1;
            allowMove = false;

            offset.X = ScreenWidth % SquareSize;
            offset.Y = ScreenHeight % SquareSize;

            for (int i = 0; i < SnakeLength; i++)
            {
                snake[i].Position = new Vector2(offset.X / 2, offset.Y / 2);
                snake[i].Size = new Vector2(SquareSize, SquareSize);
                snake[i].Speed = new Vector2(SquareSize, 0);

                if (i == 0) snake[i].Color = Color.DarkBlue;
                else snake[i].Color = Color.Blue;

                snakePosition[i] = new Vector2(0, 0);
            }

            fruit.Size = new Vector2(SquareSize, SquareSize);
            fruit.Color = Color.SkyBlue;
            fruit.Active = false;
        }

        static void UpdateGame()
        {
            if (!gameOver)
            {
                if (IsKeyPressed(KeyboardKey.P)) pause = !pause;

                if (!pause)
                {
                    // Керування гравцем
                    if (IsKeyPressed(KeyboardKey.Right) && (snake[0].Speed.X == 0) && allowMove)
                    {
                        snake[0].Speed = new Vector2(SquareSize, 0);
                        allowMove = false;
                    }
                    if (IsKeyPressed(KeyboardKey.Left) && (snake[0].Speed.X == 0) && allowMove)
                    {
                        snake[0].Speed = new Vector2(-SquareSize, 0);
                        allowMove = false;
                    }
                    if (IsKeyPressed(KeyboardKey.Up) && (snake[0].Speed.Y == 0) && allowMove)
                    {
                        snake[0].Speed = new Vector2(0, -SquareSize);
                        allowMove = false;
                    }
                    if (IsKeyPressed(KeyboardKey.Down) && (snake[0].Speed.Y == 0) && allowMove)
                    {
                        snake[0].Speed = new Vector2(0, SquareSize);
                        allowMove = false;
                    }

                    // Рух змійки
                    for (int i = 0; i < counterTail; i++) snakePosition[i] = snake[i].Position;

                    if ((framesCounter % 5) == 0)
                    {
                        for (int i = 0; i < counterTail; i++)
                        {
                            if (i == 0)
                            {
                                snake[0].Position.X += snake[0].Speed.X;
                                snake[0].Position.Y += snake[0].Speed.Y;
                                allowMove = true;
                            }
                            else snake[i].Position = snakePosition[i - 1];
                        }
                    }

                    // Перевірка зіткнення зі стінами
                    if ((snake[0].Position.X > (ScreenWidth - offset.X)) ||
                        (snake[0].Position.Y > (ScreenHeight - offset.Y)) ||
                        (snake[0].Position.X < 0) || (snake[0].Position.Y < 0))
                    {
                        gameOver = true;
                    }

                    // Перевірка зіткнення з самим собою
                    for (int i = 1; i < counterTail; i++)
                    {
                        if ((snake[0].Position.X == snake[i].Position.X) && (snake[0].Position.Y == snake[i].Position.Y)) gameOver = true;
                    }

                    // Логіка появи фруктів
                    if (!fruit.Active)
                    {
                        fruit.Active = true;
                        fruit.Position = new Vector2(
                            GetRandomValue(0, (ScreenWidth / SquareSize) - 1) * SquareSize + offset.X / 2,
                            GetRandomValue(0, (ScreenHeight / SquareSize) - 1) * SquareSize + offset.Y / 2
                        );

                        for (int i = 0; i < counterTail; i++)
                        {
                            while ((fruit.Position.X == snake[i].Position.X) && (fruit.Position.Y == snake[i].Position.Y))
                            {
                                fruit.Position = new Vector2(
                                    GetRandomValue(0, (ScreenWidth / SquareSize) - 1) * SquareSize + offset.X / 2,
                                    GetRandomValue(0, (ScreenHeight / SquareSize) - 1) * SquareSize + offset.Y / 2
                                );
                                i = 0;
                            }
                        }
                    }

                    // Колізія з фруктом
                    if (CheckCollisionRecs(
                        new Rectangle(snake[0].Position.X, snake[0].Position.Y, snake[0].Size.X, snake[0].Size.Y),
                        new Rectangle(fruit.Position.X, fruit.Position.Y, fruit.Size.X, fruit.Size.Y)))
                    {
                        snake[counterTail].Position = snakePosition[counterTail - 1];
                        counterTail += 1;
                        fruit.Active = false;
                    }

                    framesCounter++;
                }
            }
            else
            {
                if (IsKeyPressed(KeyboardKey.Enter))
                {
                    InitGame();
                }
            }
        }

        static void DrawGame()
        {
            BeginDrawing();
            ClearBackground(Color.RayWhite);

            if (!gameOver)
            {
                // Малюємо сітку
                for (int i = 0; i < ScreenWidth / SquareSize + 1; i++)
                {
                    DrawLineV(new Vector2(SquareSize * i + offset.X / 2, offset.Y / 2),
                              new Vector2(SquareSize * i + offset.X / 2, ScreenHeight - offset.Y / 2), Color.LightGray);
                }

                for (int i = 0; i < ScreenHeight / SquareSize + 1; i++)
                {
                    DrawLineV(new Vector2(offset.X / 2, SquareSize * i + offset.Y / 2),
                              new Vector2(ScreenWidth - offset.X / 2, SquareSize * i + offset.Y / 2), Color.LightGray);
                }

                // Малюємо змійку
                for (int i = 0; i < counterTail; i++)
                    DrawRectangleV(snake[i].Position, snake[i].Size, snake[i].Color);

                // Малюємо фрукт
                DrawRectangleV(fruit.Position, fruit.Size, fruit.Color);

                if (pause) DrawText("GAME PAUSED", ScreenWidth / 2 - MeasureText("GAME PAUSED", 40) / 2, ScreenHeight / 2 - 40, 40, Color.Gray);
            }
            else
            {
                DrawText("PRESS [ENTER] TO PLAY AGAIN", GetScreenWidth() / 2 - MeasureText("PRESS [ENTER] TO PLAY AGAIN", 20) / 2, GetScreenHeight() / 2 - 50, 20, Color.Gray);
            }

            EndDrawing();
        }

        static void UpdateDrawFrame()
        {
            UpdateGame();
            DrawGame();
        }
    }
}