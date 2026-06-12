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

    public const int VIRTUAL_WIDTH = 1377;
    public const int VIRTUAL_HEIGHT = 768;

    static void Main(string[] args)
    {
        // ВИПРАВЛЕНО: Використання правильних флагів
        SetConfigFlags(ConfigFlags.ResizableWindow); 
        InitWindow(VIRTUAL_WIDTH, VIRTUAL_HEIGHT, "Дух Карпат: Забута Варта");
        InitAudioDevice();

        SetTargetFPS(60);

        RenderTexture2D target = LoadRenderTexture(VIRTUAL_WIDTH, VIRTUAL_HEIGHT);
        SetTextureFilter(target.Texture, TextureFilter.Point);

        Texture2D menuBackgroundTexture = LoadTexture(".\\Resurses\\Img\\meinMenuBac.jpg");
        Texture2D optionBackgroundTexture = LoadTexture(".\\Resurses\\Img\\optionBac.jpg");
        Texture2D loadedBackgroundTexture = LoadTexture(".\\Resurses\\Img\\loadBack.jpg");

        Music ambientMusic = LoadMusicStream(".\\Resurses\\Music\\meinMusicCapter1.mp3");
        PlayMusicStream(ambientMusic);

        IMenu mainMenu = new MenuService();
        LocationService locationService = new LocationService();

        while (!WindowShouldClose() && !isClosing)
        {
            UpdateMusicStream(ambientMusic);

            float scale = MathF.Min((float)GetScreenWidth() / VIRTUAL_WIDTH, (float)GetScreenHeight() / VIRTUAL_HEIGHT);

            SetMouseOffset((int)-((GetScreenWidth() - (VIRTUAL_WIDTH * scale)) * 0.5f), (int)-((GetScreenHeight() - (VIRTUAL_HEIGHT * scale)) * 0.5f));
            SetMouseScale(1f / scale, 1f / scale);

            if (!isTransitioning)
            {
                GameState oldState = currentState;
                if (currentState == GameState.MainMenu || currentState == GameState.Settings || currentState == GameState.Chapters)
                    mainMenu.Update(ref currentState, settings, ambientMusic);
                else if (currentState == GameState.InGame)
                    locationService.Update(ref currentState);

                if (currentState != oldState) { targetState = currentState; currentState = oldState; isTransitioning = true; }
            }

            HandleFadeLogic();

            if (currentState == GameState.InGame)
            {
                locationService.PrepareGraphics();
            }

            // Тепер відкриваємо головне полотно
            BeginTextureMode(target);
            ClearBackground(Color.Black);

            DrawBackground(menuBackgroundTexture, optionBackgroundTexture, loadedBackgroundTexture);

            if (currentState == GameState.MainMenu || currentState == GameState.Settings || currentState == GameState.Chapters)
                mainMenu.Draw(currentState, settings);
            else if (currentState == GameState.InGame)
                locationService.Draw(currentState);

            if (fadeAlpha > 0) DrawRectangle(0, 0, VIRTUAL_WIDTH, VIRTUAL_HEIGHT, Fade(Color.Black, fadeAlpha / 255f));
            EndTextureMode();

            BeginDrawing();
            ClearBackground(Color.Black);
            Rectangle sourceRec = new Rectangle(0.0f, 0.0f, (float)target.Texture.Width, (float)-target.Texture.Height);
            Rectangle destRec = new Rectangle((GetScreenWidth() - ((float)VIRTUAL_WIDTH * scale)) * 0.5f, (GetScreenHeight() - ((float)VIRTUAL_HEIGHT * scale)) * 0.5f, (float)VIRTUAL_WIDTH * scale, (float)VIRTUAL_HEIGHT * scale);
            DrawTexturePro(target.Texture, sourceRec, destRec, new Vector2(0, 0), 0.0f, Color.White);
            EndDrawing();
        }

        UnloadRenderTexture(target);
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

    static void DrawBackground(Texture2D menu, Texture2D options, Texture2D loaded)
    {
        int width = GetScreenWidth();
        int height = GetScreenHeight();

        Texture2D textureToDraw;
        if (currentState == GameState.MainMenu)
            textureToDraw = menu;
        else if (currentState == GameState.Settings)
            textureToDraw = options;
        else if (currentState == GameState.Chapters)
            textureToDraw = loaded;
        else
            return; 

        Rectangle source = new Rectangle(0, 0, textureToDraw.Width, textureToDraw.Height);
        Rectangle dest = new Rectangle(0, 0, width, height);
        DrawTexturePro(textureToDraw, source, dest, new Vector2(0, 0), 0.0f, Color.White);
    }
}