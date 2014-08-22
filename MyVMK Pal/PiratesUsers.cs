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
    public partial class PiratesUsers : Form
    {
        /*
         * Login selector for Pirates
         */ 

        XmlDocument doc;
        Dictionary<string, User> users = new Dictionary<string, User>();
        Pirates pirates = null;
        Form1 form;
        _3xSX.File f = new _3xSX.File();
        _3xSX.Security s = new _3xSX.Security();

        string _path = String.Format("{0}\\MyVMK_Pal\\UserData.dat", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public PiratesUsers(Pirates pirates)
        {
            InitializeComponent();
            this.pirates = pirates;
            runOnLoadStuff();
        }

        public PiratesUsers(Form1 form)
        {
            InitializeComponent();
            this.form = form;
            runOnLoadStuff();
        }

        private void runOnLoadStuff()
        {
            this.doc = new XmlDocument();
            
            string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = appdatapath + "\\MyVMK_Pal";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            if (System.IO.File.Exists(_path))
            {
                string docData = s.decrypt(f.readDAT(_path));
                doc.LoadXml(docData);
            }
            else
            {
                XmlNode users = doc.CreateElement("users");
                doc.AppendChild(users);
                string docData = doc.OuterXml;
                docData = s.encrypt(docData);
                f.saveDAT(_path, docData);
                docData = null;
            }
        }

        private void PiratesUsers_Load(object sender, EventArgs e)
        {
            reloadUsers();
        }

        private void reloadUsers()
        {
            listBox1.Items.Clear();
            users.Clear();
            string docData = f.readDAT(_path);
            docData = s.decrypt(docData);
            XmlTextReader reader = new XmlTextReader(docData, XmlNodeType.Document, new XmlParserContext(null, null, null, XmlSpace.None));
            int i = 0;
            string character = "";
            string username = "";
            string pass = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        i++;
                        if (i == 1)
                        {
                            character = reader.Value;
                            listBox1.Items.Add(reader.Value);
                        }
                        else if (i == 2)
                        {
                            username = reader.Value;
                        }
                        else if (i == 3)
                        {
                            pass = reader.Value;
                            users.Add(character, new User(character, username, pass));
                            character = "";
                            username = "";
                            pass = "";
                            i = 0;
                        }
                        break;
                }
            }
            reader.Close();
            docData = null;
            reader = null;
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string selected = listBox1.GetItemText(listBox1.SelectedItem);
                User user = users[selected];
                //MessageBox.Show(user.character + " - " + user.username + " - " + user.password);
                if (pirates != null)
                {
                    pirates.doLogIn(user.username, user.password);
                }
                else
                {
                    form.doLogInPirates(user.username, user.password);
                }
                this.Close();
            }
            catch { }
        }
    }
}
