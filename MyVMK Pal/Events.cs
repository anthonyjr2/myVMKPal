using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Awesomium.Core;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;

namespace MyVMK_Pal
{
    public partial class Events : Form
    {
        Awesomium.Windows.Forms.WebControl wc = new Awesomium.Windows.Forms.WebControl();
        XmlDocument doc;
        Dictionary<string, User> users = new Dictionary<string, User>();
        string _path = String.Format("{0}\\MyVMK_Pal\\Planner.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public Events(Form1 form)
        {
            InitializeComponent();
            this.doc = new XmlDocument();
            string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string apath = appdatapath + "\\MyVMK_Pal";
            if (!Directory.Exists(apath))
            {
                DirectoryInfo di = Directory.CreateDirectory(apath);
            }
            if (System.IO.File.Exists(_path))
            {
                doc.Load(_path);
            }
            else
            {
                XmlNode events = doc.CreateElement("events");
                doc.AppendChild(events);
                doc.Save(_path);
            }
        }

        Dictionary<string, DateTime> events = new Dictionary<string, DateTime>();

        private void Events_Load(object sender, EventArgs e)
        {
            //Resize form, and add web browser to load interactive calendar
            string path = "http://mvmk.enx3s.com/events/cal.php";

            int height = 720;
            int width = 1280;

            this.ClientSize = new Size(width, height);

            wc.Width = width;
            wc.Height = height;

            wc.Location = new Point(0, 0);

            this.Controls.Add(wc);

            wc.LoadingFrameComplete += wc_LoadingFrameComplete;

            using (JSObject myGlobalObject = wc.CreateGlobalJavascriptObject("app"))
            {
                myGlobalObject.Bind("createEventNotif", true, JSHandler);
                myGlobalObject.Bind("doneDoAsk", true, handleEvents);
            }

            wc.Source = new Uri(path);

        }

        private void JSHandler(object sender, JavascriptMethodEventArgs args)
        {
            //Figure out proper time, converted to local time zone from EST
            string startTimeE = args.Arguments[1];
            int mod = 0;
            startTimeE = startTimeE.Split(new char []{'-'})[0];
            if (startTimeE.Substring(startTimeE.Length - 3) == "PM ")
            {
                mod = 12;
            }
            startTimeE = startTimeE.Substring(0,startTimeE.Length - 3);
            int startTime = int.Parse(startTimeE);
            startTime += mod;
            DateTime time = TimeZoneInfo.ConvertTime(new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(args.Arguments[2]), startTime, 0, 0), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Local);
            events.Add(args.Arguments[0], time);
            
        }

        private void handleEvents(object sender, JavascriptMethodEventArgs args)
        {
            //Detect if user clicked on event
            int count = events.Keys.Count;
            DateTime time;
            foreach (string key in events.Keys)
            {
                DialogResult dialogResult = MessageBox.Show("Add \"" + key + "\" to planner?", "Add event to planner", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    events.TryGetValue(key, out time);
                    addToPlanner(key, time);
                }
            }

            events.Clear();
            
        }

        public void addToPlanner(string title, DateTime datetime)
        {
            //Add XML node to planner.xml
            XmlNode newEvent = doc.CreateElement("event");

            XmlNode name = doc.CreateElement("name");
            XmlNode time = doc.CreateElement("time");

            name.InnerText = title;
            time.InnerText = datetime.ToString();

            newEvent.AppendChild(name);
            newEvent.AppendChild(time);

            doc.SelectSingleNode("//events").AppendChild(newEvent);
            doc.Save(_path);
        }

        void wc_LoadingFrameComplete(object sender, Awesomium.Core.UrlEventArgs e)
        {
            //Make sure formatting works
            string scr = @"
                var s = document.createElement('style');
                s.innerHTML = 'html, body { overflow: hidden; padding:0; margin:0; }';
                document.body.appendChild(s);
            ";

            wc.ExecuteJavascript(scr);
        }

    }

}
