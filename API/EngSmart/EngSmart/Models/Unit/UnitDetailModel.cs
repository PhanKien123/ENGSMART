using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models.Unit
{
    public class UnitDetailModel
    {
        public int ID { get; set; }
        public int unitID { get; set; }
        public string content { get; set; }
        public List<ListOptionByUnitModel> ListOption { get; set;}
    }

    public class ListOptionByUnitModel
    {
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public string Content { get; set; }
        public bool Status { get; set; }
        public int Order { get; set; }
    }
}