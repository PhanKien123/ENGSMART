using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models.Dictionaries
{
    public class DictionariesModel
    {
        public int Id { get; set; }
        public string Translate { get; set; }
        public string Word { get; set; }
        public string Description { get; set; }
        public Nullable<int> Active { get; set; }
        public string mp3 { get; set; }
    }


    public class AddDictionariesModel
    {
        public string Translate { get; set; }
        public string Word { get; set; }
        public string Description { get; set; }
        public string mp3 { get; set; }
    }
}