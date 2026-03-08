using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
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
        private bool _isMusicOn = true;
        private Rectangle _musicRect;
        private Sound hover;
        private Font _myFont;

        private bool _texturesLoaded = false;
        private bool _fontLoaded = false;
        private void LoadTextures()
        {
            if (!_texturesLoaded)
            {
                _lampTextures[0] = LoadTexture(".\\Resurses\\Img\\displayWindow.png");
                _lampTextures[1] = LoadTexture(".\\Resurses\\Img\\displayWindowWithBorder.png");
                _lampTextures[2] = LoadTexture(".\\Resurses\\Img\\displayFullScrin.png");
                _musicTextures[0] = LoadTexture(".\\Resurses\\Img\\musik_OFF.png");
                _musicTextures[1] = LoadTexture(".\\Resurses\\Img\\musik_ON.png");

                hover = LoadSound(".\\Resurses\\Music\\hover_sound.wav");
                _myFont = LoadFontEx(".\\Resurses\\Font\\PressStart2P-Regular.ttf", 32, null, 1200);
                _fontLoaded = true;
                _texturesLoaded = true;
            }
        }

        public void Update(ref GameState state, GameSettings settings, Music ambientMusic)
        {
            LoadTextures();
            UpdateElementsBounds();

            if (state == GameState.MainMenu)
            {
                if (IsButtonClicked(settings.IsEnglish ? "New Game" : "Нова Гра", 10, 40)) state = GameState.Chapters;
                if (IsButtonClicked(settings.IsEnglish ? "Load Game" : "Загрузити", 60, 40)) state = GameState.Chapters;
                if (IsButtonClicked(settings.IsEnglish ? "Options" : "Налаштовка", 130, 40)) state = GameState.Settings;
                if (IsButtonClicked(settings.IsEnglish ? "Exit" : "Вихід", 190, 40)) state = GameState.Closing; 
            }
            else if (state == GameState.Settings)
            {
                if (_waitingForKey)
                {
                    HandleBindingUpdate(settings);
                }
                else
                {
                    string langLabel = settings.IsEnglish ? "Language: English" : "Мова: Українська";
                    if (IsButtonClicked(langLabel, 70, 660)) settings.IsEnglish = !settings.IsEnglish;

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

                    string backText = settings.IsEnglish ? "Back" : "Назад";
                    if (IsButtonClicked(backText, 270, 350)) state = GameState.MainMenu;
                }
            }
        }
        
        public void Draw(GameState state, GameSettings settings)
        {
            if (state == GameState.MainMenu)
            {
                DrawMenuButton(settings.IsEnglish ? "New Game" : "Нова Гра", 10, 40);
                DrawMenuButton(settings.IsEnglish ? "Load Game" : "Загрузити", 65, 40);
                DrawMenuButton(settings.IsEnglish ? "Options" : "Налаштовка", 125, 40);
                DrawMenuButton(settings.IsEnglish ? "Exit" : "Вихід", 185, 40);
            }
            else if (state == GameState.Settings)
            {
                if (_texturesLoaded)
                {
                    string langLabel = settings.IsEnglish ? "Language: English" : $"Мова: Українська";
                    DrawMenuButton(langLabel, 70, 660);

                    DrawBindingButton(settings.IsEnglish ? "Left" : $"Вліво", settings.LeftKey, "Left", 100);
                    DrawBindingButton(settings.IsEnglish ? "Right" : "Вправо", settings.RightKey, "Right", 130);
                    DrawBindingButton(settings.IsEnglish ? "Jump" : "Стрибок", settings.JumpKey, "Jump", 160);
                    DrawBindingButton(settings.IsEnglish ? "Use" : "Дія", settings.InteractionKey, "Interact", 190);

                    int mIdx = _isMusicOn ? 1 : 0;
                    DrawTexturePro(_musicTextures[mIdx],
                        new Rectangle(0, 0, _musicTextures[mIdx].Width, _musicTextures[mIdx].Height),
                        _musicRect, new Vector2(0, 0), 0.0f, Color.White);

                    DrawTexturePro(_lampTextures[_displayMode],
                        new Rectangle(0, 0, _lampTextures[_displayMode].Width, _lampTextures[_displayMode].Height),
                        _lampRect, new Vector2(0, 0), 0.0f, Color.White);
                }

                DrawMenuButton(settings.IsEnglish ? "Back" : "Назад", 270, 350);
            }
        }

        private void DrawBindingButton(string prefix, KeyboardKey key, string target, float yOffset)
        {
            string text = (_waitingForKey && _bindingTarget == target) ? $"{prefix}: Take" : $"{prefix}: {key}";
            DrawMenuButton(text, yOffset, 660);
        }

        private void UpdateElementsBounds()
        {
            float screenW = GetScreenWidth();
            float screenH = GetScreenHeight();
            float scale = screenH / 768f; 

            float iconSize = 85f * scale;
            float panelWidth = 340 * scale;
            float centerX = (screenW - panelWidth) + (panelWidth / 2) - (760 * scale);

            float audioY = (screenH * 0.41f);
            _musicRect = new Rectangle(centerX - iconSize / 2, audioY - iconSize + (125 * scale), iconSize, iconSize);

            float displayY = audioY + (120 * scale);
            _lampRect = new Rectangle(centerX - iconSize / 2, displayY - iconSize + (160 * scale), iconSize, iconSize);
        }

        private void DrawMenuButton(string text, float yOffset, float yOffset2)
        {
            float scale = GetScreenHeight() / 768f;
            int fontSize = (int)(16 * scale);
            Vector2 textSize = MeasureTextEx(_myFont, text, fontSize, 2);
            float panelWidth = 340 * scale;
            float xPos = (GetScreenWidth() - panelWidth) + (panelWidth / 2) - (yOffset2 * scale);
            float yPos = (GetScreenHeight() * 0.41f) + (yOffset * scale);

            Rectangle rect = new Rectangle(xPos, yPos, textSize.X, textSize.Y);
            bool isHovered = CheckCollisionPointRec(GetMousePosition(), rect);

            if (isHovered && !_buttonHoverStates.GetValueOrDefault(text)) PlaySound(hover);
            _buttonHoverStates[text] = isHovered;

            DrawTextEx(_myFont, text, new Vector2(xPos, yPos), fontSize, 2, isHovered ? Color.Gold : Color.Beige);
        }

        private bool IsButtonClicked(string text, float yOffset, float yOffset2)
        {
            float scale = GetScreenHeight() / 768f;
            int fontSize = (int)(30 * scale);
            int textWidth = MeasureText(text, fontSize);

            float panelWidth = 340 * scale;
            float xPos = (GetScreenWidth() - panelWidth) + (panelWidth / 2) - (yOffset2 * scale);
            float yPos = (GetScreenHeight() * 0.41f) + (yOffset * scale);

            Rectangle rect = new Rectangle(xPos, yPos, textWidth, fontSize);
            return CheckCollisionPointRec(GetMousePosition(), rect) && IsMouseButtonPressed(MouseButton.Left);
        }

        private void CheckBindingClick(GameSettings s)
        {
            string left = s.IsEnglish ? "Left" : "Вліво";
            string right = s.IsEnglish ? "Right" : "Вправо";
            string jump = s.IsEnglish ? "Jump" : "Стрибок";
            string interact = s.IsEnglish ? "Use" : "Дія";

            if (IsButtonClicked($"{left}: {s.LeftKey}", -120, 20)) { _waitingForKey = true; _bindingTarget = "Left"; }
            if (IsButtonClicked($"{right}: {s.RightKey}", -60, 20)) { _waitingForKey = true; _bindingTarget = "Right"; }
            if (IsButtonClicked($"{jump}: {s.JumpKey}", 0, 20)) { _waitingForKey = true; _bindingTarget = "Jump"; }
            if (IsButtonClicked($"{interact}: {s.InteractionKey}", 60, 20)) { _waitingForKey = true; _bindingTarget = "Interact"; }
        }

        private void HandleBindingUpdate(GameSettings s)
        {
            int key = GetKeyPressed();
            if (key != 0)
            {
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
