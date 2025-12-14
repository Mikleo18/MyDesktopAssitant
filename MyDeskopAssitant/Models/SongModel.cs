using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyDeskopAssitant.Models
{
    public class SongModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Composer { get; set; }
        public string FilePath { get; set; }
        public string Duration { get; set; }
        public TimeSpan TimeSpanDuration { get; set; }
        public ImageSource AlbumArt { get; set; }
    }
}
