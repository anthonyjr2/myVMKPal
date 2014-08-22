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
    public partial class NewsEntry : Form
    {
        /*
         * Loads newsletter, from the links on the newsletter form
         */ 
        string data = "";

        public NewsEntry(string data)
        {
            InitializeComponent();
            this.data = data;
        }

        private void NewsEntry_Load(object sender, EventArgs e)
        {
            webBrowser1.DocumentText = data;
        }

    }
}
