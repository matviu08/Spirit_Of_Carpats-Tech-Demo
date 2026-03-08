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

        private Texture2D[] _lampTextures = new Texture2D[3];
        private int _displayMode = 0; 
        private Rectangle _lampRect; 

        private Texture2D[] _musicTextures = new Texture2D[2];
        private bool _isMusicOn = true;
        private Rectangle _musicRect;
        Sound hover;

        private bool _texturesLoaded = false;

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
                _texturesLoaded = true;
            }
        }

        public void Update(ref GameState state, Music ambientMusic)
        {
            LoadTextures();
            UpdateElementsBounds();

            if (state == GameState.MainMenu)
            {
                if (IsButtonClicked("New Game", 0)) state = GameState.Chapters;
                if (IsButtonClicked("Options", 120)) state = GameState.Settings;
                if (IsButtonClicked("Exit", 180)) state = GameState.Closing; 
            }
            else if (state == GameState.Settings)
            {
                if (CheckCollisionPointRec(GetMousePosition(), _musicRect) && IsMouseButtonPressed(MouseButton.Left))
                {
                    _isMusicOn = !_isMusicOn;
                    if (_isMusicOn)
                        ResumeMusicStream(ambientMusic);
                    else
                        PauseMusicStream(ambientMusic);
                }

                if (CheckCollisionPointRec(GetMousePosition(), _lampRect) && IsMouseButtonPressed(MouseButton.Left))
                {
                    _displayMode = (_displayMode + 1) % 3;
                    ApplyResolution();
                }

                if (IsButtonClicked("Back", 250)) state = GameState.MainMenu;
            }
        }
        

        public void Draw(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                DrawMenuButton("New Game", 0);
                DrawMenuButton("Options", 120);
                DrawMenuButton("Exit", 180);
            }
            else if (state == GameState.Settings)
            {
                if (_texturesLoaded)
                {
                    int mIdx = _isMusicOn ? 1 : 0;
                    DrawTexturePro(_musicTextures[mIdx],
                        new Rectangle(0, 0, _musicTextures[mIdx].Width, _musicTextures[mIdx].Height),
                        _musicRect, new Vector2(0, 0), 0.0f, Color.White);

                    DrawTexturePro(_lampTextures[_displayMode],
                        new Rectangle(0, 0, _lampTextures[_displayMode].Width, _lampTextures[_displayMode].Height),
                        _lampRect, new Vector2(0, 0), 0.0f, Color.White);
                }

                DrawMenuButton("Back", 250);
            }
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
        private void DrawMenuButton(string text, float yOffset)
        {
            float scale = GetScreenHeight() / 768f;
            int fontSize = (int)(30 * scale);
            int textWidth = MeasureText(text, fontSize);

            float panelWidth = 340 * scale;
            float xPos = (GetScreenWidth() - panelWidth) + (panelWidth / 2) - (20 * scale);
            float yPos = (GetScreenHeight() * 0.41f) + (yOffset * scale);

            Rectangle rect = new Rectangle(xPos, yPos, textWidth, fontSize);
            bool isHovered = CheckCollisionPointRec(GetMousePosition(), rect);

            if (isHovered && !_buttonHoverStates.GetValueOrDefault(text)) PlaySound(hover);
            _buttonHoverStates[text] = isHovered;

            DrawText(text, (int)xPos, (int)yPos, fontSize, isHovered ? Color.Gold : Color.Beige);
        }

        private bool IsButtonClicked(string text, float yOffset)
        {
            float scale = GetScreenHeight() / 768f;
            int fontSize = (int)(30 * scale);
            int textWidth = MeasureText(text, fontSize);

            float panelWidth = 340 * scale;
            float xPos = (GetScreenWidth() - panelWidth) + (panelWidth / 2) - (20 * scale);
            float yPos = (GetScreenHeight() * 0.41f) + (yOffset * scale);

            Rectangle rect = new Rectangle(xPos, yPos, textWidth, fontSize);
            return CheckCollisionPointRec(GetMousePosition(), rect) && IsMouseButtonPressed(MouseButton.Left);
        }

        private void HandleBindingUpdate()
        {
            if (_waitingForKey)
            {
                int key = GetKeyPressed();
                if (key != 0)
                {
                    _waitingForKey = false;
                }
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
