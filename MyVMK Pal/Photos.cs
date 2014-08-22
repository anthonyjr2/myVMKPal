using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Net;

namespace MyVMK_Pal
{
    public partial class Photos : Form
    {
        /*
         * MyVMK Photo Gallery
         */
        List<String> imgLoc = new List<String>();

        //Image crop stuff
        int cropX;
        int cropY;
        int cropWidth;
        int cropHeight;
        public Pen cropPen;
        public DashStyle cropDashStyle = DashStyle.DashDot;

        //I'm not giving you my imgur app id, silly ;P
        string ClientId = "[IMGUR_CLIENT_ID]";

        string openImage = "";

        public Photos(Form1 form)
        {
            InitializeComponent();
        }

        private void Photos_Load(object sender, EventArgs e)
        {
            //Static height and width
            this.Height = 658;
            this.Width = 1081;
            {
                //Load files
                DirectoryInfo dir = new DirectoryInfo(MyVMK_Pal.Properties.Settings.Default.gal);
                foreach (FileInfo file in dir.GetFiles())
                {
                    try
                    {
                        //Attempt to add to list box as image
                        Image img = Image.FromFile(file.FullName);
                        imgLoc.Add(file.FullName);
                        this.imageList1.Images.Add(img);

                    }
                    catch
                    {
                        Console.WriteLine("This is not an image file");
                    }
                }
                //List box settings
                this.listView1.View = View.LargeIcon;
                this.listView1.LargeImageList = this.imageList1;

                //Image list settings
                for (int j = 0; j < this.imageList1.Images.Count; j++)
                {
                    ListViewItem item = new ListViewItem();
                    item.ImageIndex = j;
                    this.listView1.Items.Add(item);
                }
                this.listView1.Height = 594;
            }

            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;

            //Set location of buttons and text
            button1.Location = new Point(this.Width - button1.Width - 20, this.Height - (button1.Height * 2));
            button2.Location = new Point(pictureBox1.Location.X, this.Height - (button1.Height * 2));
            label2.Location = new Point(this.Width - button1.Width - 20 - label2.Width - 20, this.Height - (button1.Height * 2) + (label2.Height /2));
            this.Height = this.Height + button1.Height;
            
        }

        //Selection rectangle
        void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (pictureBox1.Image == null)
                return;


            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                pictureBox1.Refresh();
                cropWidth = e.X - cropX;
                cropHeight = e.Y - cropY;
                pictureBox1.CreateGraphics().DrawRectangle(cropPen, cropX, cropY, cropWidth, cropHeight);
            }
        }

        //Start selection rectangle
        void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Cursor = Cursors.Cross;
                cropX = e.X;
                cropY = e.Y;

                cropPen = new Pen(Color.Red, 1);
                cropPen.DashStyle = DashStyle.DashDotDot;

            }
            pictureBox1.Refresh();
        }

        //Crop image
        private void crop()
        {
            Cursor = Cursors.Default;

            if (cropWidth < 1)
            {
                return;
            }
            Rectangle rect = new Rectangle(cropX, cropY, cropWidth, cropHeight);
            //First we define a rectangle with the help of already calculated points
            Bitmap OriginalImage = new Bitmap(pictureBox1.Image, pictureBox1.Width, pictureBox1.Height);
            //Original image
            Bitmap _img = new Bitmap(cropWidth, cropHeight);
            // for cropinf image
            Graphics g = Graphics.FromImage(_img);
            // create graphics
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //set image attributes
            g.DrawImage(OriginalImage, 0, 0, rect, GraphicsUnit.Pixel);


            Cropped c = new Cropped(_img);
            c.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        //Updates picturebox
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Update picture box with image upon select changed
            int i = this.listView1.FocusedItem.Index;
            string tag = imgLoc[i];
            this.pictureBox1.Image = Image.FromFile(tag);
            openImage = tag;
        }

        //Opens on disk
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Open up images path upon double click
            int i = this.listView1.FocusedItem.Index;
            string filePath = imgLoc[i];
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", filePath));
        }

        //Crop button
        private void button1_Click(object sender, EventArgs e)
        {
            crop();
        }

        //Imgur upload
        public string UploadImage(string image)
        {
            WebClient w = new WebClient();
            w.Headers.Add("Authorization", "Client-ID " + ClientId);
            System.Collections.Specialized.NameValueCollection Keys = new System.Collections.Specialized.NameValueCollection();
            try
            {
                Keys.Add("image", Convert.ToBase64String(File.ReadAllBytes(image)));
                byte[] responseArray = w.UploadValues("https://api.imgur.com/3/image", Keys);
                dynamic result = Encoding.ASCII.GetString(responseArray);
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("link\":\"(.*?)\"");
                Match match = reg.Match(result);
                string url = match.ToString().Replace("link\":\"", "").Replace("\"", "").Replace("\\/", "/");
                return url;
            }
            catch (Exception s)
            {
                Console.WriteLine("Something went wrong. " + s.Message);
                return "Failed!";
            }
        }

        //Upload to imgur button
        private void button2_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                string url = UploadImage(openImage);
                if (url != "Failed!")
                {
                    string[] s = url.Split('/');
                    string[] i = s[s.Length - 1].Split('.');
                    if (MessageBox.Show("Uploaded to www.Imgur.com/" + i[0] + "\n\n Open in browser?", "Uploaded!", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("http://imgur.com/" + i[0]);
                    }
                }
            }
        }

    }
}
