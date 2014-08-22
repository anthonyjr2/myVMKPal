using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace MyVMK_Pal
{
    public partial class Cropped : Form
    {
        Bitmap bmp;
        public Cropped(Bitmap bmp)
        {
            InitializeComponent();
            this.bmp = bmp;
        }

        private void Cropped_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = bmp;
            pictureBox1.Height = bmp.Height;
            pictureBox1.Width = bmp.Width;
            
            this.Width = bmp.Width;

            button1.Width = this.Width - 30;
            button1.Location = new Point(7, pictureBox1.Height + 10);
            this.Height = bmp.Height + 75;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images (*.png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(sfd.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                }
                pictureBox1.Image.Save(sfd.FileName, format);
                this.Close();
            }
        }
    }
}
