using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;

namespace MyVMK_Pal
{
    class URL
    {
        public static string getGameURL()
        {
            /*
             * Loads tester URL from a per-user xml file.
             * Not storing this one in the binary either!
             */ 


            XmlDocument doc;
            string _path = String.Format("{0}\\MyVMK_Pal\\UserSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            //Load or create XML
            doc = new XmlDocument();
            string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = appdatapath + "\\MyVMK_Pal";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (System.IO.File.Exists(_path))
            {
                doc.Load(_path);
            }
            else
            {
                XmlNode settings = doc.CreateElement("settings");
                doc.AppendChild(settings);
                doc.Save(_path);
            }

            //Get user settings
            XmlTextReader reader = new XmlTextReader(_path);
            int i = 0;
            string gameurl = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        i++;
                        if (i == 1)
                        {
                            gameurl = reader.Value;
                        }
                        else if (i == 2)
                        {
                            i = 0;
                        }
                        break;
                }
            }
            reader.Close();

            return gameurl;
        }

        public static void updateUrl(String url)
        {
            XmlDocument doc = new XmlDocument();
            string _path = String.Format("{0}\\MyVMK_Pal\\UserSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            File.Delete(_path);

            XmlNode settings = doc.CreateElement("settings");
            doc.AppendChild(settings);
            doc.Save(_path);

            doc.Load(_path);

            XmlNode newUser = doc.CreateElement("user");

            XmlNode uri = doc.CreateElement("gameurl");

            uri.InnerText = url;

            newUser.AppendChild(uri);

            doc.SelectSingleNode("//settings").AppendChild(newUser);
            doc.Save(_path);
        }
    }
}
