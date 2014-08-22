using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Awesomium.Windows.Forms;
using Awesomium.Core;

namespace MyVMK_Pal
{
    public partial class Pirates : Form
    {
        /*
         * Handles only Pirates game.
         * It's more or less a very stripped down version of myVMKPal, actually.
         * Functions much the same way, and has similar methods
         */ 

        //DONT TOUCH ANY OF THIS
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private static int bR = MyVMK_Pal.Properties.Settings.Default.bR;
        private static int bG = MyVMK_Pal.Properties.Settings.Default.bG;
        private static int bB = MyVMK_Pal.Properties.Settings.Default.bB;

        public Color borderColor = Color.FromArgb(bR, bG, bB);
        private int borderWidth = 10;
        private int borderTopWidth = 20;
        private Rectangle formRegion;
        private Color strcolor1;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern System.IntPtr CreateRoundRectRgn
        (
         int nLeftRect, // x-coordinate of upper-left corner
         int nTopRect, // y-coordinate of upper-left corner
         int nRightRect, // x-coordinate of lower-right corner
         int nBottomRect, // y-coordinate of lower-right corner
         int nWidthEllipse, // height of ellipse
         int nHeightEllipse // width of ellipse
        );
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        private static extern bool DeleteObject(System.IntPtr hObject);
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //OKAY, YOU'RE GOOD

        Form1 form;
        string game = "http://play.myvmk.com/";
        string piratesUrl = "[PIRATES_URL]"; //I don't know if Amy wants me actively releasing the Pirates URL before it's released. Sorry! 

        public Pirates(Form1 form)
        {
            this.form = form;
            InitializeComponent();
        }

        private void Pirates_Load(object sender, EventArgs e)
        {
            gameBrowser.Size = new Size(800, 600);
            //gameBrowser.Source = new Uri("http://play.myvmk.com"); 

            checkLogInStatus();

            this.Paint += paint;
            setBorderSettings();
        }

        //Sets all form color settings
        private void setBorderSettings()
        {

            this.Width = 817 + (borderWidth * 2);
            this.Height = 650 + (borderTopWidth);

            this.FormBorderStyle = FormBorderStyle.None;
            formRegion = new Rectangle(0, 0, this.Width, this.Height);

            //Set form background color (Not used currently, because gradients)
            //this.BackColor = borderColor;

            Color tc = textColor(borderColor);

            //Move controls back to location
            foreach (Control ctl in this.Controls)
            {
                if (!(ctl is StatusStrip))
                {
                    ctl.Anchor = AnchorStyles.None;
                    ctl.Left += borderWidth;
                    ctl.Top += borderTopWidth;
                }
            }

            //Render close button
            Label closeButton = new Label();
            closeButton.AutoSize = true;
            closeButton.Text = "X";
            float currentSize = 12.0F;
            closeButton.BackColor = Color.Transparent;
            closeButton.Font = new Font(closeButton.Font, closeButton.Font.Style | FontStyle.Bold);
            closeButton.Font = new Font(closeButton.Font.Name, currentSize, closeButton.Font.Style, closeButton.Font.Unit);
            closeButton.ForeColor = tc;
            this.Controls.Add(closeButton);
            closeButton.Location = new Point((this.Width - (5 + closeButton.Width)), 0);
            closeButton.Click += closeButton_Click;

            //Render minimize button
            Label minButton = new Label();
            minButton.AutoSize = true;
            minButton.Text = "--";
            minButton.BackColor = Color.Transparent;
            minButton.Font = new Font(minButton.Font, minButton.Font.Style | FontStyle.Bold);
            minButton.Font = new Font(minButton.Font.Name, currentSize, minButton.Font.Style, minButton.Font.Unit);
            minButton.ForeColor = tc;
            this.Controls.Add(minButton);
            minButton.Location = new Point((this.Width - (5 + minButton.Width + closeButton.Width + 10)), 0);
            minButton.Click += minButton_Click;

            //Render Favicon
            PictureBox favicon = new PictureBox();
            favicon.Image = MyVMK_Pal.Properties.Resources.pal_icon_sleek_16;
            favicon.BackColor = Color.Transparent;
            favicon.Size = new Size(16, 16);
            favicon.SizeMode = PictureBoxSizeMode.CenterImage;
            favicon.Location = new Point(5, 3);
            this.Controls.Add(favicon);

            //Render Application Title
            Label applicationTitle = new Label();
            applicationTitle.AutoSize = true;
            applicationTitle.Text = this.Text;
            currentSize = 10.0F;
            applicationTitle.BackColor = Color.Transparent;
            applicationTitle.ForeColor = tc;
            applicationTitle.Font = new Font(applicationTitle.Font.Name, currentSize, applicationTitle.Font.Style, applicationTitle.Font.Unit);
            this.Controls.Add(applicationTitle);
            applicationTitle.Location = new Point(5 + favicon.Width + 5, 2);

            //Fix mouse settings
            this.MouseDown += Form1_MouseDown;

            //Re-render tool-strip buttons for contrast if needed
            controlColorsDynamic(borderColor);

            //Fix screenshot button tooltip
            //toolStripButton4.AutoToolTip = false;
            this.Refresh();


        }

        //Close and Min buttons
        private void closeButton_Click(object sender, EventArgs e) { this.Close(); }
        private void minButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //Fixes mouse settings
        void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        //Form paint handler
        private void paint(object sender, PaintEventArgs e)
        {
            //ControlPaint.DrawBorder(e.Graphics, formRegion, Color.Black, borderWidth, ButtonBorderStyle.Solid, Color.Black, borderTopWidth, ButtonBorderStyle.Solid, Color.Black, borderWidth, ButtonBorderStyle.Solid, Color.Black, borderWidth, ButtonBorderStyle.Solid);

            //Gradients
            try
            {
                strcolor1 = ControlPaint.Light(borderColor, MyVMK_Pal.Properties.Settings.Default.gradient);
                using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, strcolor1, borderColor, MyVMK_Pal.Properties.Settings.Default.gradientAngle))
                {
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //Rounded Corners
            int rounding = MyVMK_Pal.Properties.Settings.Default.rounding;
            System.IntPtr ptr = CreateRoundRectRgn(0, 0, this.Width, this.Height, rounding, rounding);
            this.Region = System.Drawing.Region.FromHrgn(ptr);
            DeleteObject(ptr);
        }

        //Fix form colors for dynamic contrast
        public void controlColorsDynamic(Color c)
        {
            this.borderColor = c;
            this.BackColor = c;
            this.Refresh();
            Color fontColor = textColor(borderColor);
            foreach (Control ctl in this.Controls)
            {
                if (ctl is Label)
                {
                    ctl.BackColor = Color.Transparent;
                    ctl.ForeColor = fontColor;
                }
                else if (ctl is ToolStrip)
                {
                    ctl.BackColor = Color.Transparent;
                }
                else if (ctl is StatusStrip)
                {
                    ctl.BackColor = Color.Transparent;
                }
            }

            //Check for custom color
            if (MyVMK_Pal.Properties.Settings.Default.contrast == 3)
            {
                //If user wants custom color, tint white images
                Color cC = MyVMK_Pal.Properties.Settings.Default.iconColor;
                foreach (Control ctl in this.Controls)
                {
                    if (ctl is Label)
                    {
                        ctl.BackColor = Color.Transparent;
                        ctl.ForeColor = cC;
                    }
                    else if (ctl is ToolStrip)
                    {
                        ctl.BackColor = Color.Transparent;
                    }
                    else if (ctl is StatusStrip)
                    {
                        ctl.BackColor = Color.Transparent;
                    }
                }
                return;
            }

            
        }

        //Picks best text color
        private Color textColor(Color bg)
        {
            switch (MyVMK_Pal.Properties.Settings.Default.contrast)
            {
                case 0:
                    return Color.Black;
                case 1:
                    return Color.White;
                case 2:
                    Console.WriteLine("bg.bright = " + bg.GetBrightness());
                    return (bg.GetBrightness() >= 0.5) ? Color.Black : Color.White;
                default:
                    return Color.Black;
            }
        }

        private void checkLogInStatus()
        {
            if (!form.isUserLoggedIn)
            {
                MessageBox.Show("No character currently logged in. Please select one.");
                PiratesUsers pu = new PiratesUsers(this);
                pu.ShowDialog();
            }
            else
            {
                gameBrowser.Source = new Uri(game + piratesUrl);
            }
        }

        public void doLogIn(string user, string pass)
        {
            //Hide client during login
            gameBrowser.Visible = false;

            //Set status
            form.setStatus("Logging in...");

            //Check to see if user stored password
            if (Base64.decode(pass) == "---NOSTOREDPASSWORD---")
            {
                //If not, display prompt asking for password
                string value = "";
                if (Prompt.ShowDialog("Password Entry", "Please enter the password for " + user, ref value) == DialogResult.OK)
                {
                    //Encode password
                    pass = Base64.encode(value);
                }
            }

            //Initate interceptor
            WebCore.ResourceInterceptor = new MyVMKResourceInterceptor(user, pass);

            //Send client to login check, and listen for URL change
            gameBrowser.Source = new Uri(game + "security_check.php");
            gameBrowser.AddressChanged += client_AddressChanged;
            form.loggedInUser = user;
            form.isUserLoggedIn = true;
            form.doCredits(user, pass);

        }

        //Secondary login handler
        public void client_AddressChanged(object sender, UrlEventArgs e)
        {
            Console.WriteLine("Req-url: " + e.Url.AbsoluteUri);
            //Login correct
            if (e.Url == new Uri(game + "index.php"))
            {
                //Send client to game, show client, remove handler
                gameBrowser.Source = new Uri(game + piratesUrl);
                gameBrowser.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
                form.setStatus("Logged in (Pirates)");

            }
            //Login incorrect
            else if (e.Url == new Uri(game + "index.php?loginfailed=1"))
            {
                //Show error and remove handler
                MessageBox.Show("Could not log into your MyVMK account Are you sure your account information is correct?");
                //client.AddressChanged -= client_AddressChanged;
                form.loggedInUser = "";
                form.setStatus("MyVMK Pal");
            }
            //Logged in, possibly from clicking in user form
            else if (e.Url == new Uri(game + piratesUrl))
            {
                form.setStatus("Logged in (Pirates)");
                gameBrowser.Visible = true;
                //client.AddressChanged -= client_AddressChanged;
            }
            //Needs reauthentication
            else if (e.Url == new Uri(game + "index.php?module=authenticate"))
            {
                gameBrowser.Visible = false;
                string value = "", user = form.loggedInUser, pass = "";
                if (Prompt.ShowDialog("Password Entry", "Reauthentication needed for " + user, ref value) == DialogResult.OK)
                {
                    //Encode password
                    pass = Base64.encode(value);
                }
                //Initate interceptor
                WebCore.ResourceInterceptor = new MyVMKResourceInterceptor(user, pass);

                //Send client to login check, and listen for URL change
                gameBrowser.Source = new Uri(game + "security_check.php");
                gameBrowser.AddressChanged += client_AddressChanged;
                Console.WriteLine("Trying to log in... user " + user + "; pass: " + Base64.decode(pass));
                form.loggedInUser = user;
                form.doCredits(user, pass);
            }
        }
    }
}