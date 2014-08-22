using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyVMK_Pal
{
    public partial class Newsletter : Form
    {
        /*
         * Loads newsletter list
         */ 

        Tumblr tumblr;
        public Newsletter()
        {
            InitializeComponent();
        }

        private void Newsletter_Load(object sender, EventArgs e)
        {
            tumblr = new Tumblr();

            listBox1.Location = new Point(0,0);
            listBox1.Height = this.Height;
            listBox1.Width = this.Width;

            addEvents();
        }

        private void addEvents()
        {
            //Add all newsletters to list
            listBox1.Items.Clear();
            foreach (KeyValuePair<string, string> post in tumblr.posts)
            {
                listBox1.Items.Add(post.Key);
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            //Load newsletter on double click
            try
            {
                string selected = listBox1.GetItemText(listBox1.SelectedItem);
                string url = "";
                tumblr.posts.TryGetValue(selected, out url);

                string data = tumblr.getPost(url);

                createNewsletter(data);
            }
            catch { }
        }

        private void createNewsletter(string data)
        {
            NewsEntry ne = new NewsEntry(data);
            ne.ShowDialog();
        }

    }
}
