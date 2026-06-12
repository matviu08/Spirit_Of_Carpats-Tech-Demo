using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System.Numerics;
using System.Collections.Generic;
using static Raylib_cs.Raylib;

namespace Spirit_Of_Carpats_Remake.Services
{
    public class MenuService : IMenu
    {
        private Dictionary<string, bool> _buttonHoverStates = new();
        private bool _waitingForKey = false;
        private string _bindingTarget = "";

        private Texture2D[] _lampTextures = new Texture2D[3];
        private int _displayMode = 0;
        private Rectangle _lampRect;

        private Texture2D[] _musicTextures = new Texture2D[2];
        private Rectangle _musicRect;

        // ── ТЕКСТУРИ ──────────────────────────────────────────────────────────
        private Texture2D _panelTexture;  // Велика дошка (empty_button2)
        private Texture2D _buttonTexture; // Маленька кнопка (empty_button)
        private Sound _hover;
        private Font _myFont;

        private bool _texturesLoaded = false;

        // Налаштування для динамічних кнопок
        private const int FontSize = 22;
        private const float PaddingX = 100f; // Було 60, зробив більше для довгих слів
        private const float PaddingY = 42f; // Було 24, зробив вище

        private void LoadTextures()
        {
            if (_texturesLoaded) return;

            _lampTextures[0] = LoadTexture(".\\Resurses\\Img\\displayWindow.png");
            _lampTextures[1] = LoadTexture(".\\Resurses\\Img\\displayWindowWithBorder.png");
            _lampTextures[2] = LoadTexture(".\\Resurses\\Img\\displayFullScrin.png");
            _musicTextures[0] = LoadTexture(".\\Resurses\\Img\\musik_OFF.png");
            _musicTextures[1] = LoadTexture(".\\Resurses\\Img\\musik_ON.png");

            // Завантажуємо ОБИДВІ дошки
            _panelTexture = LoadTexture(".\\Resurses\\Img\\empty_button2.png");
            _buttonTexture = LoadTexture(".\\Resurses\\Img\\empty_button.png");

            _hover = LoadSound(".\\Resurses\\Music\\hover_sound.wav");
            _myFont = LoadFontEx(".\\Resurses\\Font\\PressStart2P-Regular.ttf", 32, null, 1200);

            _musicRect = new Rectangle(1000, 300, 85, 85);
            _lampRect = new Rectangle(1000, 450, 85, 85);

            _texturesLoaded = true;
        }

        public void Update(ref GameState state, GameSettings settings, Music ambientMusic)
        {
            LoadTextures();

            if (state == GameState.MainMenu)
            {
                if (IsButtonClicked(settings.IsEnglish ? "New Game" : "Нова Гра", 100, 320)) state = GameState.InGame;
                if (IsButtonClicked(settings.IsEnglish ? "Load Game" : "Загрузити", 100, 400)) state = GameState.Chapters;
                if (IsButtonClicked(settings.IsEnglish ? "Options" : "Налаштовка", 100, 480)) state = GameState.Settings;
                if (IsButtonClicked(settings.IsEnglish ? "Exit" : "Вихід", 100, 560)) state = GameState.Closing;
            }
            else if (state == GameState.Settings)
            {
                if (_waitingForKey)
                {
                    HandleBindingUpdate(settings);
                }
                else
                {
                    if (IsButtonClicked(settings.IsEnglish ? "Language: English" : "Мова: Українська", 100, 150))
                        settings.IsEnglish = !settings.IsEnglish;

                    CheckBindingClick(settings);

                    if (CheckCollisionPointRec(GetMousePosition(), _musicRect) && IsMouseButtonPressed(MouseButton.Left))
                    {
                        settings.MusicEnabled = !settings.MusicEnabled;
                        if (settings.MusicEnabled) ResumeMusicStream(ambientMusic);
                        else PauseMusicStream(ambientMusic);
                    }

                    if (CheckCollisionPointRec(GetMousePosition(), _lampRect) && IsMouseButtonPressed(MouseButton.Left))
                    {
                        _displayMode = (_displayMode + 1) % 3;
                        ApplyResolution();
                    }

                    // Змінив координату Y на 560, щоб кнопка не вилазила за велику дошку
                    if (IsButtonClicked(settings.IsEnglish ? "Back" : "Назад", 100, 560))
                        state = GameState.MainMenu;
                }
            }
            else if (state == GameState.Chapters)
            {
                if (IsButtonClicked(settings.IsEnglish ? "Back" : "Назад", 100, 560))
                    state = GameState.MainMenu;
            }
        }

        public void Draw(GameState state, GameSettings settings)
        {
            if (!_texturesLoaded) return;

            // 1. МАЛЮЄМО ВЕЛИКУ ДОШКУ ТІЛЬКИ ДЛЯ НАЛАШТУВАНЬ
            if (state == GameState.Settings)
            {
                // Збільшив розміри (Width: 550, Height: 580) щоб усе ідеально помістилося
                Rectangle panelRect = new Rectangle(60, 100, 550, 580);
                DrawTexturePro(_panelTexture,
                    new Rectangle(0, 0, _panelTexture.Width, _panelTexture.Height),
                    panelRect, Vector2.Zero, 0f, Color.White);
            }

            // 2. МАЛЮЄМО КНОПКИ ПОВЕРХ УСЬОГО
            if (state == GameState.MainMenu)
            {
                DrawMenuButton(settings.IsEnglish ? "New Game" : "Нова Гра", 100, 320);
                DrawMenuButton(settings.IsEnglish ? "Load Game" : "Загрузити", 100, 400);
                DrawMenuButton(settings.IsEnglish ? "Options" : "Налаштовка", 100, 480);
                DrawMenuButton(settings.IsEnglish ? "Exit" : "Вихід", 100, 560);
            }
            else if (state == GameState.Settings)
            {
                DrawMenuButton(settings.IsEnglish ? "Language: English" : "Мова: Українська", 100, 150);

                DrawBindingButton(settings.IsEnglish ? "Left" : "Вліво", settings.LeftKey, "Left", 230);
                DrawBindingButton(settings.IsEnglish ? "Right" : "Вправо", settings.RightKey, "Right", 310);
                DrawBindingButton(settings.IsEnglish ? "Jump" : "Стрибок", settings.JumpKey, "Jump", 390);
                DrawBindingButton(settings.IsEnglish ? "Use" : "Дія", settings.InteractionKey, "Interact", 470);

                int mIdx = settings.MusicEnabled ? 1 : 0;
                DrawTexturePro(_musicTextures[mIdx],
                    new Rectangle(0, 0, _musicTextures[mIdx].Width, _musicTextures[mIdx].Height),
                    _musicRect, Vector2.Zero, 0f, Color.White);

                DrawTexturePro(_lampTextures[_displayMode],
                    new Rectangle(0, 0, _lampTextures[_displayMode].Width, _lampTextures[_displayMode].Height),
                    _lampRect, Vector2.Zero, 0f, Color.White);

                DrawMenuButton(settings.IsEnglish ? "Back" : "Назад", 100, 560);
            }
            else if (state == GameState.Chapters)
            {
                DrawMenuButton(settings.IsEnglish ? "Back" : "Назад", 100, 560);
            }
        }

        private void DrawBindingButton(string prefix, KeyboardKey key, string target, float y)
        {
            string text = (_waitingForKey && _bindingTarget == target)
                ? $"{prefix}: ..."
                : $"{prefix}: {key}";
            DrawMenuButton(text, 100, y);
        }

        private void DrawMenuButton(string text, float x, float y)
        {
            // Вимірюємо текст і створюємо площу під МАЛЕНЬКУ кнопку
            Vector2 textSize = MeasureTextEx(_myFont, text, FontSize, 2);
            Rectangle rect = new Rectangle(x, y, textSize.X + PaddingX, textSize.Y + PaddingY);

            bool isHovered = CheckCollisionPointRec(GetMousePosition(), rect);

            if (isHovered && !_buttonHoverStates.GetValueOrDefault(text))
                PlaySound(_hover);
            _buttonHoverStates[text] = isHovered;

            // Малюємо текстуру маленької кнопки (empty_button.png)
            DrawTexturePro(_buttonTexture,
                new Rectangle(0, 0, _buttonTexture.Width, _buttonTexture.Height),
                rect, Vector2.Zero, 0f, isHovered ? Color.LightGray : Color.White);

            // Текст тепер автоматично відцентрований з урахуванням розширеного Padding
            Vector2 textPos = new Vector2(
                rect.X + (rect.Width - textSize.X) / 2f,
                rect.Y + (rect.Height - textSize.Y) / 2f
            );
            DrawTextEx(_myFont, text, textPos, FontSize, 2, isHovered ? Color.Gold : Color.Beige);
        }

        private bool IsButtonClicked(string text, float x, float y)
        {
            Vector2 textSize = MeasureTextEx(_myFont, text, FontSize, 2);
            Rectangle rect = new Rectangle(x, y, textSize.X + PaddingX, textSize.Y + PaddingY);

            return CheckCollisionPointRec(GetMousePosition(), rect) && IsMouseButtonPressed(MouseButton.Left);
        }

        private void CheckBindingClick(GameSettings s)
        {
            string left = s.IsEnglish ? "Left" : "Вліво";
            string right = s.IsEnglish ? "Right" : "Вправо";
            string jump = s.IsEnglish ? "Jump" : "Стрибок";
            string interact = s.IsEnglish ? "Use" : "Дія";

            if (IsButtonClicked($"{left}: {s.LeftKey}", 100, 230)) { _waitingForKey = true; _bindingTarget = "Left"; }
            if (IsButtonClicked($"{right}: {s.RightKey}", 100, 310)) { _waitingForKey = true; _bindingTarget = "Right"; }
            if (IsButtonClicked($"{jump}: {s.JumpKey}", 100, 390)) { _waitingForKey = true; _bindingTarget = "Jump"; }
            if (IsButtonClicked($"{interact}: {s.InteractionKey}", 100, 470)) { _waitingForKey = true; _bindingTarget = "Interact"; }
        }

        private void HandleBindingUpdate(GameSettings s)
        {
            int key = GetKeyPressed();
            if (key == 0) return;

            KeyboardKey k = (KeyboardKey)key;
            switch (_bindingTarget)
            {
                case "Left": s.LeftKey = k; break;
                case "Right": s.RightKey = k; break;
                case "Jump": s.JumpKey = k; break;
                case "Interact": s.InteractionKey = k; break;
            }
            _waitingForKey = false;
            _bindingTarget = "";
        }

        private void ApplyResolution()
        {
            switch (_displayMode)
            {
                case 0:
                    if (IsWindowFullscreen()) ToggleFullscreen();
                    ClearWindowState(ConfigFlags.BorderlessWindowMode);
                    SetWindowSize(1377, 768);
                    break;
                case 1:
                    if (IsWindowFullscreen()) ToggleFullscreen();
                    ClearWindowState(ConfigFlags.BorderlessWindowMode);
                    SetWindowSize(GetMonitorWidth(GetCurrentMonitor()), GetMonitorHeight(GetCurrentMonitor()) - 40);
                    break;
                case 2:
                    if (!IsWindowFullscreen())
                    {
                        SetWindowState(ConfigFlags.BorderlessWindowMode);
                        ToggleFullscreen();
                    }
                    break;
            }
        }
    }
}