using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Windows.Forms;
using Awesomium.Core;
using System.Windows.Forms;


namespace MyVMK_Pal
{
    public class MyVMK
    {
        /*
         * General MyVMK handler
         */

        WebControl client;
        Form1 form;
        
        //Default game location/URL
        string game = "http://play.myvmk.com/";
        bool itemIDS = false;

        public MyVMK(Form1 form, WebControl client)
        {
            this.client = client;
            this.form = form;

            //Check to see if user wants to use test server, set game url to Kirishiki if so
            if (MyVMK_Pal.Properties.Settings.Default.isTester)
            {
                this.game = URL.getGameURL();
                this.itemIDS = true;
            }
        }

        //Initial login script
        public void logIn(string user, string pass, bool pirates = false)
        {

            //Hide client during login
            client.Visible = false;

            //Set status
            form.setStatus("Logging in...");

            //Check to see if user stored password
            if (Base64.decode(pass) == "---NOSTOREDPASSWORD---")
            {
                //If not, display prompt asking for password
                string value = "";
                if (Prompt.ShowDialog("Password Entry", "Please enter the password for " + user, ref value) == DialogResult.OK)
                {
                    //Encode password
                    pass = Base64.encode(value);
                }
            }

            //Initate interceptor
            WebCore.ResourceInterceptor = new MyVMKResourceInterceptor(user, pass);

            //Send client to login check, and listen for URL change
            client.Source = new Uri(game + "security_check.php");
            if (!pirates)
            {
                client.AddressChanged += client_AddressChanged;
            }
            else
            {
                client.AddressChanged += client_AddressChanged_Pirates;
            }
            form.loggedInUser = user;
            form.isUserLoggedIn = true;
            form.doCredits(user, pass);
        }

        //Secondary login handler
        public void client_AddressChanged(object sender, UrlEventArgs e)
        {
            Console.WriteLine("Req-url: " + e.Url.AbsoluteUri);
            //Login correct
            if (e.Url == new Uri(game + "index.php"))
            {
                //Send client to game, show client, remove handler
                client.Source = new Uri(game + "client.php");
                client.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
                form.setStatus("Logged in");

            }
            //Login incorrect
            else if (e.Url == new Uri(game + "index.php?loginfailed=1"))
            {
                //Show error and remove handler
                MessageBox.Show("Could not log into your MyVMK account Are you sure your account information is correct?");
                //client.AddressChanged -= client_AddressChanged;
                form.loggedInUser = "";
                form.setStatus("MyVMK Pal");
            }
            //Logged in, possibly from clicking in user form
            else if (e.Url == new Uri(game + "client.php"))
            {
                client.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
                form.setStatus("Logged in");
            } 
            //Needs reauthentication
            else if(e.Url == new Uri(game + "index.php?module=authenticate")) {
                client.Visible = false;
                string value = "", user = form.loggedInUser, pass = "";
                if (Prompt.ShowDialog("Password Entry", "Reauthentication needed for " + user, ref value) == DialogResult.OK)
                {
                    //Encode password
                    pass = Base64.encode(value);
                }
                //Initate interceptor
                WebCore.ResourceInterceptor = new MyVMKResourceInterceptor(user, pass);

                //Send client to login check, and listen for URL change
                client.Source = new Uri(game + "security_check.php");
                client.AddressChanged += client_AddressChanged;
                Console.WriteLine("Trying to log in... user " + user + "; pass: " + Base64.decode(pass));
                form.loggedInUser = user;
                form.doCredits(user, pass);
            }
        }

        //Secondary login handler
        public void client_AddressChanged_Pirates(object sender, UrlEventArgs e)
        {
            Console.WriteLine("Req-url: " + e.Url.AbsoluteUri);
            //Login correct
            if (e.Url == new Uri(game + "index.php"))
            {
                //Send client to game, show client, remove handler
                client.Source = new Uri(game + "pirates.php");
                client.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
                form.setStatus("Logged in");

            }
            //Login incorrect
            else if (e.Url == new Uri(game + "index.php?loginfailed=1"))
            {
                //Show error and remove handler
                MessageBox.Show("Could not log into your MyVMK account Are you sure your account information is correct?");
                //client.AddressChanged -= client_AddressChanged;
                form.loggedInUser = "";
                form.setStatus("MyVMK Pal");
            }
            //Logged in, possibly from clicking in user form
            else if (e.Url == new Uri(game + "pirates.php"))
            {
                client.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
                form.setStatus("Logged in");
            }
            //Needs reauthentication
            else if (e.Url == new Uri(game + "index.php?module=authenticate"))
            {
                client.Visible = false;
                string value = "", user = form.loggedInUser, pass = "";
                if (Prompt.ShowDialog("Password Entry", "Reauthentication needed for " + user, ref value) == DialogResult.OK)
                {
                    //Encode password
                    pass = Base64.encode(value);
                }
                //Initate interceptor
                WebCore.ResourceInterceptor = new MyVMKResourceInterceptor(user, pass);

                //Send client to login check, and listen for URL change
                client.Source = new Uri(game + "security_check.php");
                client.AddressChanged += client_AddressChanged_Pirates;
                Console.WriteLine("Trying to log in... user " + user + "; pass: " + Base64.decode(pass));
                form.loggedInUser = user;
            }
        }
    }
}
