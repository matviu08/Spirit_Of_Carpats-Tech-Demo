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
        Sound hover = LoadSound(".\\Resurses\\Music\\hover_sound.wav");
        public void Update(ref GameState state)
        {
            Vector2 mousePos = GetMousePosition();

            if (state == GameState.MainMenu)
            {
                if (IsButtonClicked("New Game", 0)) state = GameState.Chapters;
                if (IsButtonClicked("Options", 120)) state = GameState.Settings; 
                if (IsButtonClicked("Exit", 180)) state = GameState.Closing;
            }
            else if (state == GameState.Settings)
            {
                if (IsButtonClicked("Back", 250)) state = GameState.MainMenu;
            }
        }

        public void Draw(GameState state)
        {
            if (state == GameState.MainMenu)
            {
                DrawMenuButton("New Game", 0);
                DrawMenuButton("Load Game", 60);
                DrawMenuButton("Options", 120);
                DrawMenuButton("Exit", 180);
            }
            else if (state == GameState.Settings)
            {
                DrawMenuButton("Volume", 0);
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
            int fontSize = 30; 
            int textWidth = MeasureText(text, fontSize);
            int screenWidth = GetScreenWidth();
            int screenHeight = GetScreenHeight();

            float panelWidth = 340;
            float xPos = (screenWidth - panelWidth) + (panelWidth / 2) - (textWidth / 2);
            float yPos = (screenHeight * 0.41f) + yOffset;

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
