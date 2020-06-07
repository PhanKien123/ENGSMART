using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models
{
    public class ResultModal
    {
        public int Code { get; set; }
        public int Status { get; set; }
        public string Token { get; set; }
        public object Data { get; set; }
        public string Mess { get; set; }
    }
}