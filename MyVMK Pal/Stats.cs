using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyVMK_Pal
{
    public partial class Stats : Form
    {
        string url = "http://www.myvmkcard.com/card/";
        string playername;
        string charname;
        string sig;

        public Stats()
        {
            InitializeComponent();
        }

        private void Stats_Load(object sender, EventArgs e)
        {
            label3.Text = "";
            label4.Text = "";

            label4.MaximumSize = new Size(200, 0);
            label4.AutoSize = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            playername = textBox2.Text;
            StatDecode();
            label3.Text = charname;
            label4.Text = sig;
        }

        private void StatDecode()
        {
            //Load userdata from my script.

            string finurl = url + playername + "/raw";
            string stat = new WebClient().DownloadString(finurl);

            if(stat == null || stat == "") {
                charname = "Player not found";
                return;
            }

            var token = JObject.Parse(stat);

            charname = (string)token["character"];
            sig = (string)token["signature"];

            JArray badges = (JArray)token["badges"];
            JArray pins = (JArray)token["pins"];

            if (sig == null || sig == "")
            {
                charname = "Player not found";
                sig = "";
                foreach (PictureBox pb in groupBox1.Controls.OfType<PictureBox>())
                {
                    pb.Image = null;
                    pb.Invalidate();
                }

                foreach (PictureBox pb in groupBox2.Controls.OfType<PictureBox>())
                {
                    pb.Image = null;
                    pb.Invalidate();
                }
            }
            else
            {
                try
                {
                    string[] pinarray = pins.Select(kv => kv.ToString()).ToArray();
                    int n = 0;
                    foreach (PictureBox pb in groupBox1.Controls.OfType<PictureBox>())
                    {
                        pb.Image = null;
                        pb.Invalidate();
                    }
                    foreach (PictureBox pb in groupBox1.Controls.OfType<PictureBox>().Reverse())
                    {
                        if (n < pinarray.Length)
                        {
                            pb.Load(pinarray[n]);
                            pb.SizeMode = PictureBoxSizeMode.StretchImage;
                            n++;
                        }
                    }

                    if (token["badges"] == null)
                    {
                        foreach (PictureBox pb in groupBox2.Controls.OfType<PictureBox>())
                        {
                            pb.Image = null;
                            pb.Invalidate();
                        }
                        return;
                    }
                    string[] badgearray = badges.Select(dv => dv.ToString()).ToArray();

                    n = 0;
                    foreach (PictureBox pb in groupBox2.Controls.OfType<PictureBox>())
                    {
                        pb.Image = null;
                        pb.Invalidate();
                    }
                    foreach (PictureBox pb in groupBox2.Controls.OfType<PictureBox>().Reverse())
                    {
                        if (n < badgearray.Length)
                        {
                            pb.Load(badgearray[n]);
                            pb.SizeMode = PictureBoxSizeMode.StretchImage;
                            n++;
                        }
                    }
                }
                catch
                {
                    charname = "Player not found";
                    sig = "";
                    foreach (PictureBox pb in groupBox1.Controls.OfType<PictureBox>())
                    {
                        pb.Image = null;
                        pb.Invalidate();
                    }

                    foreach (PictureBox pb in groupBox2.Controls.OfType<PictureBox>())
                    {
                        pb.Image = null;
                        pb.Invalidate();
                    }
                }
                
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
    }
}
