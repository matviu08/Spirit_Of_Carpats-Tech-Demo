using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spirit_Of_Carpats_Remake.Models;
namespace BLL.Models
{
    [Serializable]
    public class Save
    {
        public int PlayerHp { get; set; }
        public Chapter ChapterNum { get; set; }
        public string LocationName { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
    }
}
