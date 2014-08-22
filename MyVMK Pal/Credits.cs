using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace MyVMK_Pal
{

    public class CookieAware : WebClient // Set up cookie browser for credits
    {
        public CookieContainer CookieContainer { get; set; }
        public Uri Uri { get; set; }

        public CookieAware() : this(new CookieContainer()) { }

        public CookieAware(CookieContainer cookies)
        {
            this.CookieContainer = cookies;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = this.CookieContainer;
            }
            HttpWebRequest httpRequest = (HttpWebRequest)request;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return httpRequest;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            String setCookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            if (setCookieHeader != null)
            {
                //do something if needed to parse out the cookie.
                if (setCookieHeader != null)
                {
                    Cookie cookie = new Cookie(); //create cookie
                    this.CookieContainer.Add(cookie);
                }
            }
            return response;
        }
    }

    class Credits
    {
        string user;
        string pass;
        string serverUrl = "http://play.myvmk.com/";
        CookieContainer cookie;
        bool creditsInit = false;

        string credits = "";

        public Credits(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
            if (MyVMK_Pal.Properties.Settings.Default.useTestServer)
            {
                this.serverUrl = URL.getGameURL();
            }
            
        }

        public void initiateCredits()
        {
            if (MyVMK_Pal.Properties.Settings.Default.useTestServer)
            {
                serverUrl = URL.getGameURL();
            }
            WebBrowser wb1 = new WebBrowser();
            wb1.ScriptErrorsSuppressed = true;
            wb1.Navigate(serverUrl + "security_check.php");
            string webData = wb1.DocumentText;
            //(?<=You have: )(.*)(?=credits)
            Regex check = new Regex("(?<=You have: )(.*)(?=credits)", RegexOptions.Singleline);
            bool checkmatch = check.IsMatch(webData);
            if (checkmatch == true)
            {

                System.Diagnostics.Debug.WriteLine("True");
                Console.WriteLine("Credits initiated");
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("False");

                ServicePointManager.Expect100Continue = false;

                string url = serverUrl + "security_check.php";
                string passTemp = Base64.decode(pass);
                string postData = String.Format("username=" + user + "&password=" + passTemp);

                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                    req.ServicePoint.Expect100Continue = false;
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";
                    cookie = new CookieContainer();
                    req.CookieContainer = cookie;
                    byte[] bytes = Encoding.UTF8.GetBytes(postData);
                    req.ContentLength = bytes.Length;
                    using (Stream os = req.GetRequestStream())
                    {
                        os.Write(bytes, 0, bytes.Length);
                    }
                    System.Diagnostics.Debug.WriteLine("posted");

                    using (WebResponse resp = req.GetResponse())
                    {
                        System.Diagnostics.Debug.WriteLine("got response");
                        //cookieHeader = resp.Headers["Set-Cookie"];
                        creditsInit = true;
                    }

                    Console.WriteLine("Credits initiated");
                }
                catch
                {
                    MessageBox.Show("Unable to post data.");
                }
            }
        }

        public string getCredits()
        {
            if (creditsInit)
            {
                //Log in to myVMK (if have not already) and load index.
                string getUrl = serverUrl + "security_check.php";

                CookieAwareWebClient client = new CookieAwareWebClient(cookie);

                client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(DownloadStringCallback);

                //Get credits
                client.DownloadStringAsync(new Uri(getUrl));
                return this.credits;
            }
            else
            {
                return "err";
            }
        }

        private void DownloadStringCallback(Object sender, DownloadStringCompletedEventArgs e)
        {
            // If the request was not canceled and did not throw 
            // an exception, display the resource. 
            if (!e.Cancelled && e.Error == null)
            {
                string re1 = ".*?";	// Non-greedy match on filler
                string re2 = "You have (\\d+)";	// Integer Number 1

                Regex r = new Regex(re1 + re2, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                Match m = r.Match((string)e.Result);
                if (m.Success)
                {
                    String int1 = m.Groups[1].ToString();
                    this.credits = int1;
                }
            }
        }
    }

   
}
