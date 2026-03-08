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
    public class LocationService : ILocationService
    {
        private Texture2D _forestBackground;
        
        public LocationService()
        {
            _forestBackground = LoadTexture(".\\Resurses\\Img\\WoodBackground.png");
        }
        public void Draw(GameState state)
        {
            float scaleX = (float)GetScreenWidth() / _forestBackground.Width;
            float scaleY = (float)GetScreenHeight() / _forestBackground.Height;

            float scale = Math.Max(scaleX, scaleY);

            float width = _forestBackground.Width * scale;
            float height = _forestBackground.Height * scale;

            float posX = (GetScreenWidth() - width) / 2;
            float posY = (GetScreenHeight() - height) / 2;

            DrawTexturePro(
                _forestBackground,
                new Rectangle(0, 0, _forestBackground.Width, _forestBackground.Height),
                new Rectangle(posX, posY, width, height),
                new Vector2(0, 0),
                0,
                Color.White
            );
        }

        public void Update(ref GameState state)
        {
            if (IsKeyPressed(KeyboardKey.Escape))
            {
                state = GameState.MainMenu;
            }
        }
        public void Unload()
        {
            UnloadTexture(_forestBackground);

        }
    }
}
