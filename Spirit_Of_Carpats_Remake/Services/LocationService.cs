using Raylib_cs;
using Spirit_Of_Carpats_Remake.Interfaces;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            DrawTexture(_forestBackground, 0, 0, Color.White);
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
