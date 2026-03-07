using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using Spirit_Of_Carpats_Remake.Services;
using static Raylib_cs.Raylib;

namespace ForestGame;

class Program
{
    static GameState currentState = GameState.MainMenu;
    static void Main(string[] args)
    {
        AutoInsertReaurses.Sync();
        const int screenWidth = 1377;
        const int screenHeight = 768;

        InitWindow(screenWidth, screenHeight, "Дух Карпат: Забута Варта");
        InitAudioDevice();

        SetTargetFPS(60);

        Texture2D menuBackgroundTexture = LoadTexture(".\\Resurses\\Img\\meinMenuBac.png");
        Texture2D optionBackgroundTexture = LoadTexture(".\\Resurses\\Img\\optionBac.png");
        Music ambientMusic = LoadMusicStream(".\\Resurses\\Music\\meinMusicCapter1.mp3");
        PlayMusicStream(ambientMusic);
        SetMusicVolume(ambientMusic, 1f);
        IMenu mainMenu = new MenuService();

        try
        {
            while (!WindowShouldClose())
            {
                mainMenu.Update(ref currentState);

                BeginDrawing();
                ClearBackground(Color.Black);

                UpdateMusicStream(ambientMusic);
                if (currentState == GameState.MainMenu)
                {
                    DrawTexture(menuBackgroundTexture, 0, 0, Color.White);
                }
                else if (currentState == GameState.Settings)
                {
                    DrawTexture(optionBackgroundTexture, 0, 0, Color.White);
                }
                else if (currentState == GameState.Closing)
                {
                    CloseWindow();
                }


                mainMenu.Draw(currentState);

                EndDrawing();
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        UnloadTexture(menuBackgroundTexture);
        UnloadTexture(optionBackgroundTexture);
        UnloadMusicStream(ambientMusic);
        CloseAudioDevice();
        CloseWindow();
    }
}