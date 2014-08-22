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
    public partial class AddUser : Form
    {
        Users users;
        public AddUser(Users users)
        {
            this.users = users;
            InitializeComponent();
        }

        private void AddUser_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Validate form
            if (textBox1.Text == "" || textBox1.Text == "" || textBox1.Text == null) { textBox1.Text = textBox2.Text; }
            if (textBox2.Text == "" || textBox2.Text == " " || textBox2.Text == null) { MessageBox.Show("You must provide at least a username."); return; }

            if (textBox3.Text == "" || textBox3.Text == " " || textBox3.Text == null)
            {
                //Store user no password
                users.addUser(new User(textBox1.Text, textBox2.Text, Base64.encode("---NOSTOREDPASSWORD---")));
            }
            else
            {
                //Store user with password
                users.addUser(new User(textBox1.Text, textBox2.Text, Base64.encode(textBox3.Text)));
            }
            this.Close();
        }
    }
}
