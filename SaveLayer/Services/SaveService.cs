
using BLL.Models;
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
        private readonly static string path = "Save.json";
        public static async Task SavePlayer(int posX, int posY, Chapter chapter, string location, int playerHp)
        {
            var save = new Save()
            {
                ChapterNum = chapter,
                LocationName = location,
                PlayerHp = playerHp,
                PosX = posX,
                PosY = posY
            };
            string jsonData = JsonSerializer.Serialize(save);
            await File.WriteAllTextAsync(path, jsonData);
        }
        public static async Task<Save> GetSave()
        {
            string readData = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<Save>(readData);
        }
    }
}
