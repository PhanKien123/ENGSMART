using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models.Musics
{
    public class MusicItemModel
    {
        public int ID { get; set; }
        public string content { get; set; }
        public string link { get; set; }
        public string desription { get; set; }
        public string singer { get; set; }
    }
}