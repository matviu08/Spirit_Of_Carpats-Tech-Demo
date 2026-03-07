using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Services;
using static Raylib_cs.Raylib;

namespace ForestGame;

class Program
{
    static void Main(string[] args)
    {
        const int screenWidth = 1376;
        const int screenHeight = 768;

        InitWindow(screenWidth, screenHeight, "Дух Карпат: Забута Варта");
        InitAudioDevice(); 

        SetTargetFPS(60);

        Texture2D backgroundTexture = LoadTexture(".\\Resurses\\Img\\meinMenuBac.png");

        IMenu mainMenu = new MenuService();

        while (!WindowShouldClose())
        {
            mainMenu.Update();

            BeginDrawing();
            ClearBackground(Color.Black);

            DrawTexture(backgroundTexture, 0, 0, Color.White);

            mainMenu.Draw();

            EndDrawing();
        }

        UnloadTexture(backgroundTexture);
        CloseAudioDevice();
        CloseWindow();
    }
}