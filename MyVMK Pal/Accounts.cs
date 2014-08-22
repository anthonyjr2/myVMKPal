using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Windows.Forms;

namespace MyVMK_Pal
{
    class Accounts
    {
        /*
         * MyVMK Account Handler
         */

        WebControl client;
        MyVMK mv;
        _3xSX.Security s = new _3xSX.Security();

        public Accounts(WebControl client, MyVMK mv)
        {
            this.client = client;
            this.mv = mv;
        }
        
        //Handles user login
        public void tryAutoLogin()
        {
            if (MyVMK_Pal.Properties.Settings.Default.doAutoLogin)
            {
                //Decrypt user data and attempt login
                string alg = MyVMK_Pal.Properties.Settings.Default.autoLoginDetail;
                alg = s.decrypt(alg);
                string[] data = alg.Split(':');
                mv.logIn(data[1], data[2]);
            }
        }
    }
}
