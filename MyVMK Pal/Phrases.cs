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
using System.IO;

namespace MyVMK_Pal
{
    public partial class Phrases : Form
    {
        /*
         * Load/edit phrases
         */ 
        XmlDocument doc;
        Form1 form;
        //bool fileexists;

        string _path = String.Format("{0}\\MyVMK_Pal\\Phrases.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public Phrases(Form1 form)
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
            
            //ID text boxes
            int id = 0;
            foreach (var tB in AllControls(this).OfType<TextBox>())
            {
                tB.Tag = id;
                id++;
            }
        }

        public IEnumerable<Control> AllControls(Control container)
        {
            foreach (Control control in container.Controls)
            {
                yield return control;

                foreach (var innerControl in AllControls(control))
                    yield return innerControl;
            }
        }

        private void Phrases_Load(object sender, EventArgs e)
        {
            reloadPhrases();
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        //Save Phrases
        private void button1_Click(object sender, EventArgs e)
        {
            //Save phrases to phrases.xml
            List<string> phrases = new List<string>();
            int i = 0;
            foreach (var tB in AllControls(this).OfType<TextBox>().Reverse())
            {
                phrases.Add(tB.Text);
            }
            
            File.WriteAllLines(_path, phrases);

            form.loadPhrases();

            this.Close();
        }

        //Cancel
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Refresh text boxes with saved phrases
        private void reloadPhrases()
        {
            List<TextBox> tBs = new List<TextBox>();
            foreach (var tB in AllControls(this).OfType<TextBox>().Reverse())
            {
                tB.Clear();
                tB.MaxLength = 53;
                tBs.Add(tB);
            }

            if (File.Exists(_path))
            {

                string[] phrases = File.ReadAllLines(_path);

                int t = 0;

                foreach (string phrase in phrases)
                {
                    if (t < tBs.Count)
                    {
                        tBs[t].Text = phrase;
                        t++;
                    }
                }
            }
            form.loadPhrases();
        }
    }
}
