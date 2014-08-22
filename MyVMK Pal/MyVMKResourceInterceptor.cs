using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core;

namespace MyVMK_Pal
{
    public class MyVMKResourceInterceptor : IResourceInterceptor
    {
        /*
         * Awesomium Resource Interceptor for login script
         */
        string user, pass;

        public MyVMKResourceInterceptor(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }

        //Needed, but not used (don't ask)
        public bool OnFilterNavigation(NavigationRequest request)
        {
            return false;
        }

        public ResourceResponse OnRequest(ResourceRequest request)
        {
            if (request.Url.ToString().Contains("security_check.php"))
            {
                //Format data to be posted
                var data = "username=" + user + "&password=" + Base64.decode(pass);
                request.Method = "POST";
                request.AppendUploadBytes(data, (uint)data.Length);
                request.AppendExtraHeader("Content-Type", "application/x-www-form-urlencoded");

                //Dispose of username and pass, to be sent to GC
                data = null;
                user = null;
                pass = null;
            }
            return null;
        }
    }
}
