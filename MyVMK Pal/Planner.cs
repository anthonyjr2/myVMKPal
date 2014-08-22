using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;

namespace MyVMK_Pal
{
    public partial class Planner : Form
    {
        /*
         * Loads and handles event planner data
         */ 

        //Dictionary<string, string> events = new Dictionary<string, string>();
        XmlDocument doc;
        string _path = String.Format("{0}\\MyVMK_Pal\\Planner.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        Form1 form;

        public Planner(Form1 form)
        {
            InitializeComponent();
            this.form = form;
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

        private void Planner_Load(object sender, EventArgs e)
        {
            listBox1.Location = new Point(0,0);
            listBox1.Height = this.Height;
            listBox1.Width = this.Width / 2;
            reloadEvents();
            button1.Enabled = false;
            label1.Text = "";
            label2.Text = "";

            if (MyVMK_Pal.Properties.Settings.Default.eventAlertsIP)
            {
                checkBox1.Checked = true;
            }

        }

        //Loads events from planner.xml
        private void reloadEvents()
        {
            listBox1.Items.Clear();
            form.events.Clear();
            XmlTextReader reader = new XmlTextReader(_path);
            int i = 0;
            string name = "";
            string time = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        i++;
                        if (i == 1)
                        {
                            name = reader.Value;
                            listBox1.Items.Add(reader.Value);
                        }
                        else if (i == 2)
                        {
                            time = reader.Value;
                            form.events.Add(name, time);
                            name = "";
                            time = "";
                            i = 0;
                        }
                        break;
                }
            }
            reader.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Load data from selected event
            label1.Font = new Font("Arial", label1.Font.Size, FontStyle.Bold);
            label1.Text = listBox1.SelectedItem.ToString();

            string time = "";
            form.events.TryGetValue(label1.Text, out time);
            DateTime timx = DateTime.Parse(time);
            string ampm = (timx.Hour > 12 ? "PM" : "AM");
            int hour = (timx.Hour > 12 ? (timx.Hour - 12) : timx.Hour);
            time = timx.Month + "/" + timx.Day + "/" + timx.Year + " " + hour + ampm;
            label2.Text = "Time: " + time;

            this.Width = (label1.Width > label2.Width ? label1.Location.X + label1.Width + 20 : label2.Location.X + label2.Width + 20);

            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selected = listBox1.GetItemText(listBox1.SelectedItem);
            XmlNodeList nodes = doc.SelectNodes("//event/name");
            foreach (XmlNode xn in nodes)
            {
                if (xn.InnerText == selected)
                {
                    xn.ParentNode.ParentNode.RemoveChild(xn.ParentNode);

                }
            }

            label1.Text = "";
            label2.Text = "";

            doc.Save(_path);
            reloadEvents();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.eventAlertsIP = checkBox1.Checked;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Save();

            if (checkBox1.Checked == true)
            {
                form.event_timer.Start();
            }
            else
            {
                form.event_timer.Stop();
            }
        }
    }
}
