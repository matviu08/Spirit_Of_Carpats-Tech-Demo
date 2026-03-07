using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Services;
using static Raylib_cs.Raylib;

namespace ForestGame;

class Program
{
    static void Main(string[] args)
    {
        AutoInsertReaurses.Sync();
        const int screenWidth = 1377;
        const int screenHeight = 768;

        InitWindow(screenWidth, screenHeight, "Дух Карпат: Забута Варта");
        InitAudioDevice(); 

        SetTargetFPS(60);

        Texture2D backgroundTexture = LoadTexture(".\\Resurses\\Img\\meinMenuBac.png");
        Music ambientMusic = LoadMusicStream(".\\Resurses\\Music\\meinMusicCapter1.mp3");
        PlayMusicStream(ambientMusic);
        SetMusicVolume(ambientMusic, 1f);
        IMenu mainMenu = new MenuService();

        while (!WindowShouldClose())
        {
            mainMenu.Update();

            BeginDrawing();
            ClearBackground(Color.Black);

            UpdateMusicStream(ambientMusic);
            DrawTexture(backgroundTexture, 0, 0, Color.White);

            mainMenu.Draw();

            EndDrawing();
        }

        UnloadMusicStream(ambientMusic);
        UnloadTexture(backgroundTexture);
        CloseAudioDevice();
        CloseWindow();
    }
}