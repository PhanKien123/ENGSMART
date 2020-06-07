using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EngSmart.Models.User
{
    public class AddUserModel
    {
        public string Name { get; set; }
        public string Pass { get; set; }
        public string Phone { get; set; }
        public Boolean Gender { get; set;}
    }

    public class UserItemModel
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public string Pass { get; set; }
        public int Active { get; set; }
        public System.DateTime CreateDate { get; set; }
        public Nullable<int> Rank { get; set; }
        public string Phone { get; set; }
        public Nullable<bool> Gender { get; set; }
    }
}