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
        private MenuScreen _currentScreen = MenuScreen.Main;
        private Dictionary<string, bool> _buttonHoverStates = new();
        private GameSettings _settings = new();
        private bool _waitingForKey = false;
        private string _activeBinding = "";
        Sound hover = LoadSound(".\\Resurses\\Music\\hover_sound.wav");
        public void Update()
        {
            Vector2 mousePos = GetMousePosition();

            if (_currentScreen == MenuScreen.Main)
            {
                if (IsButtonClicked("New Game", 0)) _currentScreen = MenuScreen.Chapters;
                if (IsButtonClicked("Quit", 200)) CloseWindow();
            }
            //else if (_currentScreen == MenuScreen.Settings)
            //{
            //    if (IsButtonClicked(_settings.IsEnglish ? "Language: EN" : "Мова: УА", 0))
            //        _settings.IsEnglish = !_settings.IsEnglish;

            //    if (IsButtonClicked("Binds", 100)) _currentScreen = MenuScreen.Binds;
            //    if (IsButtonClicked("Back", 250)) _currentScreen = MenuScreen.Main;
            //}
            //else if (_currentScreen == MenuScreen.Binds)
            //{
            //    HandleBindingUpdate();
            //    if (IsButtonClicked("Back", 300)) _currentScreen = MenuScreen.Settings;
            //}
        }

        public void Draw()
        {
            ClearBackground(Color.Black);

            if (_currentScreen == MenuScreen.Main)
            {
                DrawMenuButton("New Game", 0);
                DrawMenuButton("Load Game", 60);
                DrawMenuButton("Options", 120);
                DrawMenuButton("Exit", 180);
            }
            else if (_currentScreen == MenuScreen.Settings)
            {
                DrawMenuButton(_settings.IsEnglish ? "Language: English" : "Мова: Українська", 0);
                DrawMenuButton("Binds", 100);
                DrawMenuButton("Back", 250);
            }
        }

        private void DrawMenuButton(string text, float yOffset)
        {
            int fontSize = 30;
            int screenWidth = GetScreenWidth();
            int screenHeight = GetScreenHeight();

            int textWidth = MeasureText(text, fontSize);
            float panelWidth = 200;
            float xPos = screenWidth - panelWidth;
            float yPos = (screenHeight * 0.41f) + yOffset;

            Vector2 pos = new Vector2(xPos, yPos);
            Rectangle buttonRect = new Rectangle(pos.X, pos.Y, textWidth, fontSize);

            bool isHovered = CheckCollisionPointRec(GetMousePosition(), buttonRect);

            if (!_buttonHoverStates.ContainsKey(text))
            {
                _buttonHoverStates[text] = false;
            }

            if (isHovered && !_buttonHoverStates[text])
            {
                PlaySound(hover);
            }

            _buttonHoverStates[text] = isHovered;

            DrawText(text, (int)pos.X, (int)pos.Y, fontSize, isHovered ? Color.Gold : Color.Beige);
        }

        private bool IsButtonClicked(string text, float yOffset)
        {
            int fontSize = 30; // Має збігатися з Draw
            int textWidth = MeasureText(text, fontSize);
            int screenWidth = GetScreenWidth();
            int screenHeight = GetScreenHeight();

            float panelWidth = 340;
            float xPos = (screenWidth - panelWidth) + (panelWidth / 2) - (textWidth / 2);
            float yPos = (screenHeight * 0.55f) + yOffset;

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
    }
}
