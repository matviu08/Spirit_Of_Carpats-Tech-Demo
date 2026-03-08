using Raylib_cs;
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirit_Of_Carpats_Remake.Interfaces
{
    public interface IMenu
    {
        void Update(ref GameState state, Music ambientMusic);
        void Draw(GameState state);
    }
}
