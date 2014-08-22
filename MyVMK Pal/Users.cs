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
    public partial class Users : Form
    {
        /*
         * Loads and handles account
         */ 

        XmlDocument doc;
        Dictionary<string, User> users = new Dictionary<string, User>();
        Form1 form;
        _3xSX.File f = new _3xSX.File();
        _3xSX.Security s = new _3xSX.Security();

        string _oldpath = String.Format("{0}\\MyVMK_Pal\\UserData.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        string _path = String.Format("{0}\\MyVMK_Pal\\UserData.dat", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public Users(Form1 form)
        {
            InitializeComponent();
            this.doc = new XmlDocument();
            this.form = form;
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

        //Load all users from UserData.dat
        private void Users_Load(object sender, EventArgs e)
        {
            reloadUsers();
            if (!MyVMK_Pal.Properties.Settings.Default.doAutoLogin)
            {
                label2.Text = "Not Set";
            }
            else
            {
                string alg = MyVMK_Pal.Properties.Settings.Default.autoLoginDetail;
                string[] data = alg.Split(':');
                label2.Text = data[0];
            }


            if (MyVMK_Pal.Properties.Settings.Default.hasRemovedOS)
            {
                button5.Hide();
            }
            else
            {
                MessageBox.Show("It is highly recommended that you remove your old user data using the provided button.");
            }
        }

        public void addUser(User user)
        {
            //Add user to .dat file
            XmlNode newUser = doc.CreateElement("user");

            XmlNode character = doc.CreateElement("character");
            XmlNode username = doc.CreateElement("username");
            XmlNode password = doc.CreateElement("password");

            character.InnerText = user.character;
            username.InnerText = user.username;
            password.InnerText = user.password;

            newUser.AppendChild(character);
            newUser.AppendChild(username);
            newUser.AppendChild(password);

            doc.SelectSingleNode("//users").AppendChild(newUser);
            string docData = doc.OuterXml;
            //Encrypt xml data
            docData = s.encrypt(docData);
            if (File.Exists(_path)) { File.Delete(_path); }
            f.saveDAT(_path, docData);
            docData = null;
            reloadUsers();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string selected = listBox1.GetItemText(listBox1.SelectedItem);
                User user = users[selected];
                form.doLogIn(user.username, user.password);
                this.Close();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddUser au = new AddUser(this);
            au.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (File.Exists(_path)) { File.Delete(_path);  }
            string selected = listBox1.GetItemText(listBox1.SelectedItem);
            XmlNodeList nodes = doc.SelectNodes("//user/character");
            foreach (XmlNode xn in nodes)
            {
                if (xn.InnerText == selected)
                {
                    xn.ParentNode.ParentNode.RemoveChild(xn.ParentNode);

                }
            }
            string docData = doc.OuterXml;
            docData = s.encrypt(docData);
            if (File.Exists(_path)) { File.Delete(_path); }
            f.saveDAT(_path, docData);
            docData = null;
            reloadUsers();
        }

        private void reloadUsers()
        {

            //Load users
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

        private void button3_Click(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.doAutoLogin = false;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            label2.Text = "Not Set";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                string selected = listBox1.GetItemText(listBox1.SelectedItem);
                MyVMK_Pal.Properties.Settings.Default.doAutoLogin = true;
                MyVMK_Pal.Properties.Settings.Default.autoLoginDetail = s.encrypt(selected + ":" + users[selected].username + ":" + users[selected].password);
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                label2.Text = selected;
            }
            catch { }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            File.Delete(_oldpath);
            MyVMK_Pal.Properties.Settings.Default.hasRemovedOS = true;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            button5.Hide();
            MessageBox.Show("Deleted old user account data.");
        }

    }
}
