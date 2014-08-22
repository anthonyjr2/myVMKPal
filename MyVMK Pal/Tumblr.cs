using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;

namespace MyVMK_Pal
{
    class Tumblr
    {
        /*
         * RSS handler to load tumblr data
         */ 

        private static string rssFeed = "http://myvmk-official.tumblr.com/rss";
        public Dictionary<string, string> posts;

        public Tumblr() {
            posts = getPosts();
        }

        private Dictionary<string, string> getPosts() {
            //Load all post URLs and titles into a dictionary 
            Dictionary<string, string> posts = new Dictionary<string, string>();

            XmlDocument rss = GetXmlDataFromUrl(rssFeed);

            bool ignore;
            string e_title = "";
            string e_link = "";

            foreach (XmlElement element in rss.GetElementsByTagName("item"))
            {
                ignore = false;
                e_title = "";
                foreach (XmlElement title in element.GetElementsByTagName("title"))
                {
                    if (!title.InnerText.ToLower().Contains("newsletter"))
                    {
                        ignore = true;
                    }
                    else
                    {
                        e_title = title.InnerText;
                    }
                }
                if (!ignore)
                {
                    foreach (XmlElement link in element.GetElementsByTagName("link"))
                    {
                        e_link = link.InnerText;
                    }
                }

                Console.WriteLine(e_title + "=");

                if (!ignore)
                {
                    posts.Add(e_title, e_link);
                }
            }

            return posts;
        }

        //Get data of specific post
        public string getPost(string postURL)
        {
            string postData = "";

            XmlDocument rss = GetXmlDataFromUrl(postURL + "/rss");

            foreach (XmlElement element in rss.GetElementsByTagName("item"))
            {
                foreach (XmlElement desc in element.GetElementsByTagName("description"))
                {
                    postData = desc.InnerText;
                }
            }

            return postData;
        }

        private static XmlDocument GetXmlDataFromUrl(string url)
        {

            //requesting the particular web page
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            //geting the response from the request url
            var response = (HttpWebResponse)httpRequest.GetResponse();

            //create a stream to hold the contents of the response (in this case it is the contents of the XML file
            var receiveStream = response.GetResponseStream();

            //creating XML document
            var mySourceDoc = new XmlDocument();

            //load the file from the stream
            mySourceDoc.Load(receiveStream);

            //close the stream
            receiveStream.Close();

            return mySourceDoc;
        }
    }
}
