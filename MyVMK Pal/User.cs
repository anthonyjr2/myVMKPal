using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyVMK_Pal
{
    public class User
    {
        /*
         * User/player object.
         */ 

        public string username;
        public string character;
        public string password;

        public User(string character, string username, string password)
        {
            this.character = character;
            this.username = username;
            this.password = password;

        }
    }
}
