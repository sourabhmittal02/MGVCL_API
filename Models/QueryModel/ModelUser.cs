using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCenterCoreAPI.Models.QueryModel
{
    public class UserRequestQueryModel
    {
        public string LoginId { get; set; }

        public string Password { get; set; }
    }

    public class ModelUser : UserRequestQueryModel
    {

        public int User_id { get; set; }
        public string User_Name { get; set; }

        public string Confirm_Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public long Mobile_NO { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_deleted { get; set; }
        public List<ModelRoles> RolesCollection { get; set; }
        public int RoleID { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ModelMgvcluser
    {

        private string _username = "styra_user"; // field
        public string username   // property
        {
            get { return _username; }   // get method
            set { _username = value; }  // set method
        }

        private string _password = "Ajh#!ge^g!95L"; // field
        public string password   // property
        {
            get { return _password; }   // get method
            set { _password = value; }  // set method
        }

    }
    public class ModelMgvcluserResponse
    {
        public string status { get; set; }
        public string description { get; set; }
        public data data { get; set; }
    }

    public class data
    {
        public string jwt { get; set; }
        public user user { get; set; }

    }
    public class user
    {
        public string username { get; set; }
        public string fullName { get; set; }
    }

}