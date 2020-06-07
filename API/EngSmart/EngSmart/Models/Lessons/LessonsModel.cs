using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models.Lessons
{
    public class LessonsItemModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public int Order { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }


    public class AddLessonItemModel
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public int Order { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}