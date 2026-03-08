
using Spirit_Of_Carpats_Remake.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SaveLayer.Services
{
    public class SaveService
    {
        public static void SavePlayer(int posX, int posY, Chapter chapter, string location)
        {
            JsonSerializer.Serialize();
        }
    }
}
