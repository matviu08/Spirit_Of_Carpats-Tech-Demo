using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using Spirit_Of_Carpats_Remake.Services;
using static Raylib_cs.Raylib;

namespace ForestGame;

class Program
{
    static GameState currentState = GameState.MainMenu;
    static GameState targetState = GameState.MainMenu;

    static float fadeAlpha = 0f;
    static bool isTransitioning = false;
    static bool isClosing = false;
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
            while (!WindowShouldClose() && !isClosing)
            {
                UpdateMusicStream(ambientMusic);

                if (!isTransitioning)
                {
                    GameState oldState = currentState;
                    mainMenu.Update(ref currentState);

                    if (currentState != oldState)
                    {
                        if (currentState == GameState.Closing)
                        {
                            isTransitioning = true;
                        }
                        else
                        {
                            targetState = currentState;
                            currentState = oldState; 
                            isTransitioning = true;   
                        }
                    }
                }

                if (isTransitioning)
                {
                    fadeAlpha += 5f; 
                    if (fadeAlpha >= 255)
                    {
                        fadeAlpha = 255;
                        if (targetState == GameState.Closing || currentState == GameState.Closing)
                        {
                            isClosing = true; 
                        }
                        else
                        {
                            currentState = targetState; 
                            isTransitioning = false;
                        }
                    }
                }
                else if (fadeAlpha > 0)
                {
                    fadeAlpha -= 5f; 
                    if (fadeAlpha < 0) fadeAlpha = 0;
                }

                BeginDrawing();
                ClearBackground(Color.Black);

                if (currentState == GameState.MainMenu)
                    DrawTexture(menuBackgroundTexture, 0, 0, Color.White);
                else if (currentState == GameState.Settings)
                    DrawTexture(optionBackgroundTexture, 0, 0, Color.White);

                mainMenu.Draw(currentState);

                if (fadeAlpha > 0)
                {
                    DrawRectangle(0, 0, screenWidth, screenHeight, Fade(Color.Black, fadeAlpha / 255f));
                }

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