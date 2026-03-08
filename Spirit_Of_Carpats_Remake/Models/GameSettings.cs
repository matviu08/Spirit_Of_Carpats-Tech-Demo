using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirit_Of_Carpats_Remake.Models
{
    public enum Chapter
    {
        Chapter1,
        Chapter2,
        Chapter3,
        Chapter4,
        Chapter5
    }

    public enum GameState
    {
        MainMenu,
        Settings,
        Closing,
        Chapters,
        InGame 
    }

    public class GameSettings
    {
        public bool IsEnglish = true;
        public bool MusicEnabled = true;
        public KeyboardKey LeftKey = KeyboardKey.A;
        public KeyboardKey RightKey = KeyboardKey.D;
        public KeyboardKey InteractionKey = KeyboardKey.E;
        public KeyboardKey JumpKey = KeyboardKey.Space;
    }
}
