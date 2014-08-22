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
    public partial class ToolbarButtons : Form
    {
        /*
         * Enable/disable toolbar buttons
         */ 

        Form1 form;
        ToolStrip ts;

        string[] states = new string[6];

        public ToolbarButtons(Form1 form)
        {
            this.form = form;
            ts = form.toolStrip1;
            InitializeComponent();
        }

        private void ToolbarButtons_Load(object sender, EventArgs e)
        {
            
            //checkedListBox1.ItemCheck += checkedListBox1_ItemCheck;
            bool doShow = true;

            string[] values = MyVMK_Pal.Properties.Settings.Default.toolItems.Select(x => x.ToString()).ToArray();
            int i = 0;

            foreach (ToolStripButton tb in ts.Items)
            {
                doShow = (values[i] == "f" ? false : true);
                
                if (tb.Text != "Item ID Search")
                {
                    switch (tb.Text)
                    {
                        case "Home": case "Account Manager": case "Take Screenshot": case "Gallery": case "Settings": case "Pirates":
                            //checkedListBox1.Items.Add(tb.Text, CheckState.Indeterminate);
                            break;
                        default:
                            checkedListBox1.Items.Add(tb.Text, doShow);
                            states[i] = (doShow ? "t" : "f");
                            if (i < 5) { i++; }
                            break;
                    }
                    
                }
            }

            Console.WriteLine(MyVMK_Pal.Properties.Settings.Default.toolItems);
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkedListBox1_ItemCheck_1(object sender, ItemCheckEventArgs e)
        {

            states[e.Index] = (e.NewValue == CheckState.Checked ? "t" : "f");

            Console.WriteLine(string.Join("", states).Length);

            
                MyVMK_Pal.Properties.Settings.Default.toolItems = string.Join("", states);
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();

                Console.WriteLine(string.Join("", states));
            if (string.Join("", states).Length == 6)
            {
                form.setToolStrip();
            }

        }
    }
}
