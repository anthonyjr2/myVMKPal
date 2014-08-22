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
    public partial class Settings : Form
    {
        Form1 form;
        bool justLoaded = true;

        public Settings(Form1 form)
        {
            InitializeComponent();
            this.form = form;
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            if (MyVMK_Pal.Properties.Settings.Default.isTester) { checkBox1.Checked = true; } else { checkBox1.Checked = false; }
            if (MyVMK_Pal.Properties.Settings.Default.creditPreview) { checkBox2.Checked = true; } else { checkBox2.Checked = false; }
            if (MyVMK_Pal.Properties.Settings.Default.startupupdate) { checkBox3.Checked = true; } else { checkBox3.Checked = false; }
            if (MyVMK_Pal.Properties.Settings.Default.icoVer == 0) { checkBox5.Checked = true;  } else { checkBox5.Checked = false; }

            checkBox3.CheckedChanged += checkBox3_CheckedChanged;

            //FS Background
            if (MyVMK_Pal.Properties.Settings.Default.fsDefault)
            {
                try { color.Image = null; }
                catch { }
                color.Image = MyVMK_Pal.Properties.Resources.blacknoise;
            }
            else if (MyVMK_Pal.Properties.Settings.Default.fsIsImage)
            {
                try { color.Image = null; }
                catch { }
                color.Image = Image.FromFile(MyVMK_Pal.Properties.Settings.Default.fsImage);
            }
            else
            {
                try { color.Image = null; } 
                catch { }
                color.BackColor = MyVMK_Pal.Properties.Settings.Default.fsColor;
            }

            //Check analytics
            if (!MyVMK_Pal.Properties.Settings.Default.analytics)
            {
                checkBox4.Checked = true;
            }
            else
            {
                checkBox4.Checked = false;
            }

            //Contrast radio buttons
            switch (MyVMK_Pal.Properties.Settings.Default.contrast)
            {
                case 0:
                    radioButton2.Checked = true;
                    break;
                case 1:
                    radioButton3.Checked = true;
                    break;
                case 2:
                    radioButton1.Checked = true;
                    break;
                case 3:
                    radioButton4.Checked = true;
                    break;
            }

            //Pirates game mode
            switch (MyVMK_Pal.Properties.Settings.Default.piratesType)
            {
                case 0:
                    radioButton5.Checked = true;
                    break;
                case 1:
                    radioButton6.Checked = true;
                    break;
                case 2:
                    radioButton7.Checked = true;
                    break;
            }

            //Gradient level
            numericUpDown1.Value = (decimal) MyVMK_Pal.Properties.Settings.Default.gradient;

            //Gradient angle
            numericUpDown3.Value = (decimal)MyVMK_Pal.Properties.Settings.Default.gradientAngle;

            //Rounding level
            numericUpDown2.Value = MyVMK_Pal.Properties.Settings.Default.rounding;

            //Galery location
            textBox1.Text = MyVMK_Pal.Properties.Settings.Default.gal;

            //Set verision number
            label2.Text = "version " + form.currentversion;

            //Disable analytics checkbox
            checkBox4.Visible = false;

            //Hide toolbar buttons button
            //button6.Visible = true;

            //Disable custom color
            radioButton4.Visible = false;

            //Disable credit preview checkbox
            checkBox2.Checked = false;
            checkBox2.Enabled = false;

            setupToolTips();

            justLoaded = false;
        }

        //Test server option
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            string value = "";
            if (checkBox1.Checked && (!justLoaded))
            {
                if (Prompt.ShowDialog("Verify Tester", "Please enter valid tester URL (Format: http://xxxx.xxxx.com/ [trailing slash is important])", ref value) != DialogResult.OK)
                {
                    checkBox1.Checked = false;
                    return;
                }

                MyVMK_Pal.Properties.Settings.Default.isTester = true;
                URL.updateUrl(value);
                MessageBox.Show("URL set. If URL is invalid, game will not function. Turn off to fix. Reload myVMKPal for change to take effect.");
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.isTester = false;
            }
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            if (MyVMK_Pal.Properties.Settings.Default.isTester) { checkBox1.Checked = true; }
        }

        //Hide cursor option
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                MyVMK_Pal.Properties.Settings.Default.creditPreview = true;
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.creditPreview = false;
            }
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        //Update on start option
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                MyVMK_Pal.Properties.Settings.Default.startupupdate = true;
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.startupupdate = false;
            }
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        //Open changelog
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.myvmkpal.com/changelog");
        }

        //Check for updates
        private void button1_Click(object sender, EventArgs e)
        {
            Updates u = new Updates(form, form.currentversion, form.updaterversion);
            if (!u.checkUpdates())
            {
                MessageBox.Show("No updates available.");
            }
        }

        //Background option (COLOR)
        private void button2_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            cd.AllowFullOpen = true;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                try { color.Image = null; }
                catch { }
                color.BackColor = cd.Color;

                MyVMK_Pal.Properties.Settings.Default.fsColor = cd.Color;
                MyVMK_Pal.Properties.Settings.Default.fsDefault = false;
                MyVMK_Pal.Properties.Settings.Default.fsIsImage = false;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
            }
        }

        //Background option (RESET)
        private void button3_Click(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.fsDefault = true;
            MyVMK_Pal.Properties.Settings.Default.fsIsImage = false;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();

            try { color.Image = null; }
            catch { }
            color.Image = MyVMK_Pal.Properties.Resources.blacknoise;
        }

        //Background option (IMAGE)
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();

            od.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            if (od.ShowDialog() == DialogResult.OK)
            {
                color.Image = null;

                MyVMK_Pal.Properties.Settings.Default.fsIsImage = true;
                MyVMK_Pal.Properties.Settings.Default.fsDefault = false;
                MyVMK_Pal.Properties.Settings.Default.fsImage = od.FileName;

                color.Image = Image.FromFile(od.FileName);
            }

        }

        //Analytics settings
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox4.Checked)
            {
                MyVMK_Pal.Properties.Settings.Default.analytics = true;
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.analytics = false;
            }

            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        //Contrast option (AUTO)
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.contrast = 2;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            form.controlColorsDynamic(form.borderColor);
        }

        //Contrast option (BLACK)
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.contrast = 0;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            form.controlColorsDynamic(form.borderColor);
        }

        //Contrast option (WHITE)
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.contrast = 1;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            form.controlColorsDynamic(form.borderColor);
        }

        //Save color gradient level
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.gradient = (float)numericUpDown1.Value;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            //Live update value
            form.Refresh();
        }

        //Set rounding level
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.rounding = (int) numericUpDown2.Value;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            //Live update value
            form.Refresh();
        }

        //Gradient angle
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.gradientAngle = (float)numericUpDown3.Value;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            //Live update value
            form.Refresh();

        }

        //Gallery location
        private void button5_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();
            fd.SelectedPath = MyVMK_Pal.Properties.Settings.Default.gal;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                MyVMK_Pal.Properties.Settings.Default.gal = fd.SelectedPath;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                textBox1.Text = MyVMK_Pal.Properties.Settings.Default.gal;
            }
        }

        //Custom control color
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked && !justLoaded)
            {
                ColorDialog cd = new ColorDialog();
                cd.AllowFullOpen = true;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    MyVMK_Pal.Properties.Settings.Default.iconColor = cd.Color;
                    MyVMK_Pal.Properties.Settings.Default.contrast = 3;
                    MyVMK_Pal.Properties.Settings.Default.Save();
                    MyVMK_Pal.Properties.Settings.Default.Reload();
                    form.controlColorsDynamic(form.borderColor);
                }
            }
            
        }

        private void label2_Click(object sender, EventArgs e)
        {
            
        }

        //Toolbar buttons button
        private void button6_Click(object sender, EventArgs e)
        {
            ToolbarButtons tb = new ToolbarButtons(form);
            tb.ShowDialog();
        }


        private void setupToolTips()
        {
            ToolTip t1 = new ToolTip(), t2 = new ToolTip(), t3 = new ToolTip();
            t1.SetToolTip(radioButton5, "Pirates appears in a window separate of the main Pal window.");
            t2.SetToolTip(radioButton6, "Pirates will completely replace MyVMK, exiting your current game session. If you are not already logged in, it will open Pirates in the same place that MyVMK would load.");
            t3.SetToolTip(radioButton7, "Pirates will be placed over MyVMK, leaving your current game session active. You can swap back to MyVMK at any time.");
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.piratesType = 0;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.piratesType = 1;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            MyVMK_Pal.Properties.Settings.Default.piratesType = 2;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                MyVMK_Pal.Properties.Settings.Default.icoVer = 0;
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.icoVer = 1;
            }
            form.controlColorsDynamic(Color.FromArgb(MyVMK_Pal.Properties.Settings.Default.bR, MyVMK_Pal.Properties.Settings.Default.bG, MyVMK_Pal.Properties.Settings.Default.bB));
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                MyVMK_Pal.Properties.Settings.Default.vertical = true;
                MessageBox.Show("Toolbar will be vertical upon restart of myVMKPal.");
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.vertical = false;
                MessageBox.Show("Toolbar will be horizontal upon restart of myVMKPal.");
            }
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();
            
        }
    }
}
