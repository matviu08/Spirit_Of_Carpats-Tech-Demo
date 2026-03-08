using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using Spirit_Of_Carpats_Remake.Services;
using System.Numerics;
using static Raylib_cs.Raylib;

namespace ForestGame;

class Program
{
    static GameState currentState = GameState.MainMenu;
    static GameState targetState = GameState.MainMenu; 
    static GameSettings settings = new GameSettings();
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
        LocationService locationService = new LocationService();

        try
        {
            while (!WindowShouldClose() && !isClosing)
            {
                UpdateMusicStream(ambientMusic);

                if (!isTransitioning)
                {
                    GameState oldState = currentState;

                    if (currentState == GameState.MainMenu || currentState == GameState.Settings)
                        mainMenu.Update(ref currentState, settings, ambientMusic);
                    else if (currentState == GameState.Chapters || currentState == GameState.InGame)
                        locationService.Update(ref currentState);

                    if (currentState != oldState)
                    {
                        targetState = currentState;
                        currentState = oldState;
                        isTransitioning = true;
                    }
                }

                HandleFadeLogic();

                BeginDrawing();
                ClearBackground(Color.Black);

                DrawBackground(menuBackgroundTexture, optionBackgroundTexture);

                if (currentState == GameState.MainMenu || currentState == GameState.Settings)
                    mainMenu.Draw(currentState, settings);
                else if (currentState == GameState.Chapters || currentState == GameState.InGame)
                    locationService.Draw(currentState);

                if (fadeAlpha > 0)
                {
                    DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(), Fade(Color.Black, fadeAlpha / 255f));
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
        locationService.Unload();
        CloseAudioDevice();
        CloseWindow();
    }

    static void HandleFadeLogic()
    {
        if (isTransitioning)
        {
            fadeAlpha += 5f;
            if (fadeAlpha >= 255)
            {
                fadeAlpha = 255;
                if (targetState == GameState.Closing) isClosing = true;
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
    }

    static void DrawBackground(Texture2D menu, Texture2D options)
    {
        int width = GetScreenWidth();
        int height = GetScreenHeight();

        Texture2D textureToDraw;
        if (currentState == GameState.MainMenu)
            textureToDraw = menu;
        else if (currentState == GameState.Settings)
            textureToDraw = options;
        else
            return; 

        Rectangle source = new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height);
        Rectangle dest = new Rectangle(0, 0, width, height);
        DrawTexturePro(textureToDraw, source, dest, new Vector2(0, 0), 0.0f, Color.White);
    }
}