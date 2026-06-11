using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spirit_Of_Carpats_Remake.Interfaces
{
    public interface ILocationService
    {
        void Update(ref GameState state);
        void CaptureScene();
        void Draw(GameState state);
        void Unload();
    }
}
