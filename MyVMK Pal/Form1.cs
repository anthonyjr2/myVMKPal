using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Security.Principal;
using Awesomium.Core;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MyVMK_Pal
{
    public partial class Form1 : Form
    {
        /*
        * MyVMKPal Main Form
        */

        //DONT TOUCH ANY OF THIS
        private static int bR = MyVMK_Pal.Properties.Settings.Default.bR;
        private static int bG = MyVMK_Pal.Properties.Settings.Default.bG;
        private static int bB = MyVMK_Pal.Properties.Settings.Default.bB;

        public Color borderColor = Color.FromArgb(bR, bG, bB);
        private int borderWidth = 10;
        private int borderTopWidth = 20;
        private Rectangle formRegion;
        private Color strcolor1;

        [DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        private Form blackbox = new BlackBox(); //Full screen blackbox

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, int dwExtraInfo);

        //HotKey stuff
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd,
          int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        //Testing something here...
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

        [DllImport("Shell32")]
        public static extern int ExtractIconEx(
            string sFile,
            int iIndex,
            out IntPtr piLargeVersion,
            out IntPtr piSmallVersion,
            int amountIcons);

        private const int MOUSEEVENTF_ABSOLUTE = 0x8000;
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;
        private const int MOUSEEVENTF_XDOWN = 0x0080;
        private const int MOUSEEVENTF_XUP = 0x0100;

        private BackgroundWorker bw;

        private bool cursorVis = true;
        private string homeurl = "http://myvmkpal.com";

        private Awesomium.Windows.Forms.WebControl anna = new Awesomium.Windows.Forms.WebControl(); //Analytic web browser

        private PictureBox pallatte = new PictureBox();

        //SAFE
        //Okay you can touch this stuff:)

        //Initialise variables
        public string currentversion = "2.1d.1";
        public string updaterversion = ""; //Don't bother setting this, it'll get overwritten
        MyVMK mv; //Login handler
        Credits ch; //Credit count handler
        Updates u; //Update handler
        Accounts ac; //AccMan
        HotKeys hk; //Hotkey manager
        Payouts pay;
        EventPlannerAlerts epa; //Event planner alert manager
        Timer room_timer = new Timer(); //Room detection timer
        Timer credit_timer = new Timer(); //Credit grab timer
        public Timer event_timer = new Timer(); //Event planner timer

        //Pirate game client swap
        private bool isSwapped = false;
        private bool firstSwap = true;
        private string piratesUrl = "http://play.myvmk.com/pirates.php";
        Awesomium.Windows.Forms.WebControl piratesClient;

        //List of phrases
        List<string> phrases = new List<string>();

        //Initialise up room detection
        RoomDetect rd = new RoomDetect();

        //Keep track of logged in user
        public string loggedInUser = "";
        public bool isUserLoggedIn = false;

        //Item ID list for testers
        public Dictionary<int, Dictionary<string, string>> itemIDSList = null;

        //List of events
        public Dictionary<string, string> events = new Dictionary<string, string>();

        public Form1()
        {
            //Debugger stuff
            InitializeComponent();
            DoubleBuffered = true;

            //Disable plugins for now, while bugs are worked out
            //PresentPlugins();
        }

        //Setup plugin manager
        private void PresentPlugins()
        {
            PluginManager pMan = new PluginManager(toolStrip1);
            Plugin[] plugins = pMan.Plugins;

            foreach (Plugin plugin in plugins)
            {
                if (plugin.Exception == null)
                {
                    Console.WriteLine("Loaded " + plugin.PluginName);
                }
                else
                {
                    string msg = @"Could not load plugin """ + plugin.PluginName + "\".\n";
                    MessageBox.Show(msg + plugin.Exception.Message, Application.ProductName,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        //Runs on form load
        private void Form1_Load(object sender, EventArgs e)
        {
            //Get updater version
            System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.HttpWebRequest.Create("http://myvmkpal.com/update/updater.version.txt");
            System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());
            updaterversion = sr.ReadToEnd();

            //Set error mode
            SetErrorMode(ErrorModes.SEM_FAILCRITICALERRORS);

            //Add pirates browser
            piratesClient = new Awesomium.Windows.Forms.WebControl();
            this.Controls.Add(piratesClient);
            piratesClient.Visible = false;

            //Make sure BlackBox is not visible in taskbar
            blackbox.ShowInTaskbar = false;

            //Check for needed Dlls
            checkForDlls();

            //Check if is after update
            fresh();

            //Check for updates
            u = new Updates(this, currentversion, updaterversion);
            u.updateUpdater();
            if (MyVMK_Pal.Properties.Settings.Default.startupupdate)
            {
                u.checkUpdates();
            }

            //Check runtime activation policy
            u.checkRuntimeActivationPolicy();

            //Iniate helpers
            mv = new MyVMK(this, clientBrowser);
            ac = new Accounts(clientBrowser, mv);
            epa = new EventPlannerAlerts(this);

            //HotKey helper
            hk = new HotKeys(this, clientBrowser);

            //Set up room detection
            room_timer.Interval = 1000;
            room_timer.Tick += new EventHandler(CheckRoom);
            //room_timer.Start();

            MyVMK_Pal.Properties.Settings.Default.creditPreview = false;
            MyVMK_Pal.Properties.Settings.Default.Save();
            MyVMK_Pal.Properties.Settings.Default.Reload();

            //Set defaults for labels
            setStatus("MyVMK Pal");

            //Set clientBrowser location to top left, and set size
            clientBrowser.Location = new Point(0, toolStrip1.Height);
            clientBrowser.Size = new Size(800, 600);

            //Remove any buttons from the toolstrip that the user doesnt want
            setToolStrip();

            //Style form
            this.Paint += paint;
            setBorderSettings();
            
            //Go to start page
            clientBrowser.Source = new Uri(homeurl);

            checkNewSecurity();

            //Attempt to automatically log in
            ac.tryAutoLogin();

            //Set up payout reader
            pay = new Payouts();

            //Set up credit timer
            credit_timer.Interval = 2000;
            credit_timer.Tick += new EventHandler(TimerEventProcessor);
            toolStripStatusLabel2.Text = "";

            //Make sure credits are pushed to right side
            toolStripStatusLabel1.Spring = true;
            toolStripStatusLabel1.TextAlign = ContentAlignment.TopLeft;

            
            //Set score display to blank
            toolStripStatusLabel3.Text = "";
            
            /* 
             * Something about Analytics is SEVERELY broken. Looking for a fix.
            //Check if hasn't told about analytics
            analytics();
            
            //Start (or not) analytics
            allowAnalytics(); */

            //Load in phrases
            loadPhrases();

            //Load in events
            loadEvents();

            //Register hotKeys
            registerCommands();

            //Check for screenshot directory
            checkScreenshots();

            //Set up event planner timer
            event_timer.Interval = 60000;
            event_timer.Tick += event_timer_Tick;

            if (MyVMK_Pal.Properties.Settings.Default.eventAlertsIP)
            {
                Console.WriteLine("Running event timer");
                event_timer.Start();
            }

            Console.WriteLine("Everything Loaded");
        }

        void event_timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("Checking for events");
            epa.checkForEvents();
        }

        //Sets all form color settings
        private void setBorderSettings()
        {

            this.Width = 817 + (borderWidth * 2);
            this.Height = 721 + (borderTopWidth);

            //Vertical
            bool doVert = MyVMK_Pal.Properties.Settings.Default.vertical;
            int wdth = 0;
            if (doVert)
            {
                wdth = toolStrip1.Height;
                toolStrip1.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
                toolStrip1.Width = wdth;
                this.Width += wdth;
                this.Height -= borderTopWidth + (wdth / 2);
            }

            this.FormBorderStyle = FormBorderStyle.None;
            formRegion = new Rectangle(0, 0, this.Width, this.Height);

            //Set form background color (Not used currently, because gradients)
            //this.BackColor = borderColor;

            Color tc = textColor(borderColor);

            //Move controls back to location
            foreach (Control ctl in this.Controls)
            {
                if (!(ctl is StatusStrip)) {
                    //Vertical form
                    if(doVert) {
                        if (ctl is Awesomium.Windows.Forms.WebControl)
                        {
                            ctl.Left += wdth + borderWidth;
                            ctl.Top -= borderTopWidth + borderWidth;
                            continue;
                        }
                    }
                    ctl.Anchor = AnchorStyles.None;
                    ctl.Left += borderWidth;
                    ctl.Top += borderTopWidth;
                }
            }

            //FIX STUFF
            //Fix Toolstrip
            toolStrip1.BackColor = Color.Transparent;
            toolStrip1.Renderer = new MyVMKToolStripRenderer();

            //Fix statusStrip
            statusStrip1.BackColor = Color.Transparent;
            statusStrip1.SizingGrip = false;

            //Setup full screen
            blackbox.BackColor = Color.Black;
            blackbox.FormBorderStyle = FormBorderStyle.None;

            //Render close button
            /*Label closeButton = new Label();
            closeButton.AutoSize = true;
            closeButton.Text = "X";
            float currentSize = 12.0F;
            closeButton.BackColor = Color.Transparent;
            closeButton.Font = new Font(closeButton.Font, closeButton.Font.Style | FontStyle.Bold);
            closeButton.Font = new Font(closeButton.Font.Name, currentSize, closeButton.Font.Style, closeButton.Font.Unit);
            closeButton.ForeColor = tc; */
            PictureBox closeButton = new PictureBox();
            closeButton.Image = MyVMK_Pal.Properties.Resources.exitbutton;
            closeButton.BackColor = Color.Transparent;
            closeButton.Size = new Size(24, 24);
            closeButton.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(closeButton);
            closeButton.Location = new Point((this.Width - (5 + closeButton.Width)), (clientBrowser.Location.Y - closeButton.Height) / 2);
            closeButton.Click += closeButton_Click;

            //Render maximize button
            /* Label maxButton = new Label();
            maxButton.AutoSize = true;
            maxButton.Text = "[]";
            maxButton.BackColor = Color.Transparent;
            maxButton.Font = new Font(maxButton.Font, maxButton.Font.Style | FontStyle.Bold);
            maxButton.Font = new Font(maxButton.Font.Name, currentSize, maxButton.Font.Style, maxButton.Font.Unit);
            maxButton.ForeColor = tc; */
            PictureBox maxButton = new PictureBox();
            maxButton.Image = MyVMK_Pal.Properties.Resources.maximize;
            maxButton.BackColor = Color.Transparent;
            maxButton.Size = new Size(24, 24);
            maxButton.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(maxButton);
            maxButton.Location = new Point((this.Width - (5 + maxButton.Width + closeButton.Width + 5)), (clientBrowser.Location.Y - maxButton.Height) / 2);
            maxButton.Click += maxButton_Click;

            //Render minimize button
            /* Label minButton = new Label();
            minButton.AutoSize = true;
            minButton.Text = "--";
            minButton.BackColor = Color.Transparent;
            minButton.Font = new Font(minButton.Font, minButton.Font.Style | FontStyle.Bold);
            minButton.Font = new Font(minButton.Font.Name, currentSize, minButton.Font.Style, minButton.Font.Unit);
            minButton.ForeColor = tc; */
            PictureBox minButton = new PictureBox();
            minButton.Image = MyVMK_Pal.Properties.Resources.minimize;
            minButton.BackColor = Color.Transparent;
            minButton.Size = new Size(24, 24);
            minButton.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(minButton);
            minButton.Location = new Point((this.Width - (5 + minButton.Width + maxButton.Width + 5 + closeButton.Width + 5)), (clientBrowser.Location.Y - minButton.Height) / 2);
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
            float currentSize = 10.0F;
            applicationTitle.BackColor = Color.Transparent;
            applicationTitle.ForeColor = tc;
            applicationTitle.Font = new Font(applicationTitle.Font.Name, currentSize, applicationTitle.Font.Style, applicationTitle.Font.Unit);
            this.Controls.Add(applicationTitle);
            applicationTitle.Location = new Point(5 + favicon.Width + 5, 2);

            //Render pallate button
            pallatte.Image = MyVMK_Pal.Properties.Resources.pallette;
            pallatte.BackColor = Color.Transparent;
            pallatte.Size = new Size(20, 20);
            pallatte.SizeMode = PictureBoxSizeMode.Zoom;
            pallatte.Location = new Point((minButton.Location.X - 30), (clientBrowser.Location.Y - pallatte.Height) / 2);
            this.Controls.Add(pallatte);
            pallatte.Click += pallatteButton_Click;

            //Fix mouse settings
            this.MouseDown += Form1_MouseDown;

            //Re-render tool-strip buttons for contrast if needed
            controlColorsDynamic(borderColor);

            //Fix screenshot button tooltip
            //toolStripButton4.AutoToolTip = false;
            this.Refresh();


        }

        //Picks best text color
        private Color textColor(Color bg)
        {
            switch(MyVMK_Pal.Properties.Settings.Default.contrast) {
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

        //Fixes mouse settings
        void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (blackbox.Visible != true)
                {
                    ReleaseCapture();
                    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                }
            }
        }

        //FORM CLOSE, MINIMIZE, AND PALLATTE BUTTONS
        private void closeButton_Click(object sender, EventArgs e) { this.Close(); }
        private void minButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            if (blackbox.Visible == true)
            {
                blackbox.Hide();
            }
        }
        private void maxButton_Click(object sender, EventArgs e)
        {
            //Detect if user is already in full screen
            if (blackbox.Visible != true)
            {
                //Create BlackBox, if not already created
                if (MyVMK_Pal.Properties.Settings.Default.fsDefault)
                {
                    blackbox.BackgroundImage = MyVMK_Pal.Properties.Resources.blacknoise;
                    blackbox.BackgroundImageLayout = ImageLayout.Tile;
                }
                else if (MyVMK_Pal.Properties.Settings.Default.fsIsImage)
                {
                    blackbox.BackgroundImage = Image.FromFile(MyVMK_Pal.Properties.Settings.Default.fsImage);
                    blackbox.BackgroundImageLayout = ImageLayout.Tile;
                }
                else
                {
                    blackbox.BackColor = MyVMK_Pal.Properties.Settings.Default.fsColor;
                }
                blackbox.Show();
                blackbox.WindowState = FormWindowState.Maximized;
                this.BringToFront();
                //Center Pal on screen
                this.CenterToScreen();
            }
            else
            {
                blackbox.Hide();
            }
        }
        private void pallatteButton_Click(object sender, EventArgs e)
        {
            //Show color selector
            ColorDialog cd = new ColorDialog();
            cd.Color = borderColor;
            cd.AllowFullOpen = true;
            if (cd.ShowDialog() == DialogResult.OK)
            {
                controlColorsDynamic(cd.Color);

                MyVMK_Pal.Properties.Settings.Default.bR = cd.Color.R;
                MyVMK_Pal.Properties.Settings.Default.bG = cd.Color.G;
                MyVMK_Pal.Properties.Settings.Default.bB = cd.Color.B;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
            }
        }

        //Set control colors
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

            int icoVer = MyVMK_Pal.Properties.Settings.Default.icoVer;

            //Icon array [new/old, b/w, id]
            Image[, ,] icon = new Image[2, 2, 14];

            //Old Icon Array
            {
                //Black
                icon[0, 0, 0] = MyVMK_Pal.Properties.Resources.MYVMK52B;
                icon[0, 0, 1] = MyVMK_Pal.Properties.Resources.Forum52B;
                icon[0, 0, 2] = MyVMK_Pal.Properties.Resources.camerawiki_04;
                icon[0, 0, 3] = MyVMK_Pal.Properties.Resources.Icon_10_52;
                icon[0, 0, 4] = MyVMK_Pal.Properties.Resources.Cal52B;
                icon[0, 0, 5] = MyVMK_Pal.Properties.Resources.Settings52B;
                icon[0, 0, 6] = MyVMK_Pal.Properties.Resources.Flaticon_14449small;
                icon[0, 0, 7] = MyVMK_Pal.Properties.Resources.phrases_521;
                icon[0, 0, 8] = MyVMK_Pal.Properties.Resources.twitter_52;
                icon[0, 0, 9] = MyVMK_Pal.Properties.Resources.itemID;
                icon[0, 0, 10] = MyVMK_Pal.Properties.Resources.planner;
                icon[0, 0, 11] = MyVMK_Pal.Properties.Resources.newsletter;
                icon[0, 0, 12] = MyVMK_Pal.Properties.Resources.piratesB;
                icon[0, 0, 13] = MyVMK_Pal.Properties.Resources.pallette;

                //White

                icon[0, 1, 0] = MyVMK_Pal.Properties.Resources.MYVMK52B_W;
                icon[0, 1, 1] = MyVMK_Pal.Properties.Resources.Forum52B_W;
                icon[0, 1, 2] = MyVMK_Pal.Properties.Resources.camerawiki_04_W;
                icon[0, 1, 3] = MyVMK_Pal.Properties.Resources.Icon_10_52_W;
                icon[0, 1, 4] = MyVMK_Pal.Properties.Resources.Cal52B_W;
                icon[0, 1, 5] = MyVMK_Pal.Properties.Resources.Settings52B_W;
                icon[0, 1, 6] = MyVMK_Pal.Properties.Resources.Flaticon_14449small_W;
                icon[0, 1, 7] = MyVMK_Pal.Properties.Resources.phrases_521_W;
                icon[0, 1, 8] = MyVMK_Pal.Properties.Resources.twitter_52_w;
                icon[0, 1, 9] = MyVMK_Pal.Properties.Resources.itemID_W;
                icon[0, 1, 10] = MyVMK_Pal.Properties.Resources.planner_w;
                icon[0, 1, 11] = MyVMK_Pal.Properties.Resources.newsletter_w;
                icon[0, 1, 12] = MyVMK_Pal.Properties.Resources.piratesW;
                icon[0, 1, 13] = MyVMK_Pal.Properties.Resources.pallette_W;
            }

            //New Icon Array
            {
                //Black
                icon[1, 0, 0] = MyVMK_Pal.Properties.Resources.home_2_b;
                icon[1, 0, 1] = MyVMK_Pal.Properties.Resources.accounts_2_b;
                icon[1, 0, 2] = MyVMK_Pal.Properties.Resources.screen_2_b;
                icon[1, 0, 3] = MyVMK_Pal.Properties.Resources.gallery_2_b;
                icon[1, 0, 4] = MyVMK_Pal.Properties.Resources.calendar_2_b;
                icon[1, 0, 5] = MyVMK_Pal.Properties.Resources.settings_2_b;
                icon[1, 0, 6] = MyVMK_Pal.Properties.Resources.stats_2_b;
                icon[1, 0, 7] = MyVMK_Pal.Properties.Resources.phrases_2_b;
                icon[1, 0, 8] = MyVMK_Pal.Properties.Resources.twitter_2_b;
                icon[1, 0, 9] = MyVMK_Pal.Properties.Resources.itemID;
                icon[1, 0, 10] = MyVMK_Pal.Properties.Resources.planner_2_b;
                icon[1, 0, 11] = MyVMK_Pal.Properties.Resources.news_2_b;
                icon[1, 0, 12] = MyVMK_Pal.Properties.Resources.pirates_2_b;
                icon[1, 0, 13] = MyVMK_Pal.Properties.Resources.pallette;

                //White
                icon[1, 1, 0] = MyVMK_Pal.Properties.Resources.home_2_w;
                icon[1, 1, 1] = MyVMK_Pal.Properties.Resources.accounts_2_w;
                icon[1, 1, 2] = MyVMK_Pal.Properties.Resources.screen_2_w;
                icon[1, 1, 3] = MyVMK_Pal.Properties.Resources.gallery_2_w;
                icon[1, 1, 4] = MyVMK_Pal.Properties.Resources.calendar_2_w;
                icon[1, 1, 5] = MyVMK_Pal.Properties.Resources.settings_2_w;
                icon[1, 1, 6] = MyVMK_Pal.Properties.Resources.stats_2_w;
                icon[1, 1, 7] = MyVMK_Pal.Properties.Resources.phrases_2_w;
                icon[1, 1, 8] = MyVMK_Pal.Properties.Resources.twitter_2_w;
                icon[1, 1, 9] = MyVMK_Pal.Properties.Resources.itemID_W;
                icon[1, 1, 10] = MyVMK_Pal.Properties.Resources.planner_2_w;
                icon[1, 1, 11] = MyVMK_Pal.Properties.Resources.news_2_w;
                icon[1, 1, 12] = MyVMK_Pal.Properties.Resources.pirates_2_w;
                icon[1, 1, 13] = MyVMK_Pal.Properties.Resources.pallette_W;
            }

            //Re-render tool-strip buttons for contrast if needed
            if (textColor(borderColor) == Color.White)
            {
                //If need contrast against dark
                toolStripButton1.Image = icon[icoVer, 1, 0];
                toolStripButton2.Image = icon[icoVer, 1, 1];
                toolStripButton4.Image = icon[icoVer, 1, 2];
                toolStripButton5.Image = icon[icoVer, 1, 3];
                toolStripButton6.Image = icon[icoVer, 1, 4];
                toolStripButton7.Image = icon[icoVer, 1, 5];
                toolStripButton8.Image = icon[icoVer, 1, 6];
                toolStripButton9.Image = icon[icoVer, 1, 7];
                toolStripButton10.Image = icon[icoVer, 1, 8];
                toolStripButton12.Image = icon[icoVer, 1, 10];
                toolStripButton13.Image = icon[icoVer, 1, 11];
                toolStripButton3.Image = icon[icoVer, 1, 12];
                pallatte.Image = icon[icoVer, 1, 13];
                toolStripStatusLabel1.ForeColor = Color.White;
                toolStripStatusLabel2.ForeColor = Color.White;
                toolStripStatusLabel3.ForeColor = Color.White;
                toolStrip1.Refresh();
            }
            else
            {
                //If need contrast against light
                toolStripButton1.Image = icon[icoVer, 0, 0];
                toolStripButton2.Image = icon[icoVer, 0, 1];
                toolStripButton4.Image = icon[icoVer, 0, 2];
                toolStripButton5.Image = icon[icoVer, 0, 3];
                toolStripButton6.Image = icon[icoVer, 0, 4];
                toolStripButton7.Image = icon[icoVer, 0, 5];
                toolStripButton8.Image = icon[icoVer, 0, 6];
                toolStripButton9.Image = icon[icoVer, 0, 7];
                toolStripButton10.Image = icon[icoVer, 0, 8];
                toolStripButton12.Image = icon[icoVer, 0, 10];
                toolStripButton13.Image = icon[icoVer, 0, 11];
                toolStripButton3.Image = icon[icoVer, 0, 12];
                pallatte.Image = icon[icoVer, 0, 13];
                toolStripStatusLabel1.ForeColor = Color.Black;
                toolStripStatusLabel2.ForeColor = Color.Black;
                toolStripStatusLabel3.ForeColor = Color.Black;
                toolStrip1.Refresh();
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

        //Set toolStrip status
        public void setStatus(string msg) { toolStripStatusLabel1.Text = msg; }

        //Handles log in from outside classes (Account manager)
        public void doLogIn(string user, string pass)
        {
            mv.logIn(user, pass);
            try
            {
                doCredits(user, pass);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }

        public void doLogInPirates(string user, string pass)
        {
            //Login user to Pirates
            mv.logIn(user, pass, true);
        }

        //Get credits going
        public void doCredits(string user, string pass)
        {
            ch = new Credits(user, pass);
            ch.initiateCredits();
            credit_timer.Start();
        }

        //Check for room ID, timer handler
        private void CheckRoom(Object myObject, EventArgs myEventArgs)
        {
            //Get the room id from RoomDetect
            int id = rd.roomDetect(clientBrowser, borderWidth);
            //Check to see if user wants CreditPreview
            if (MyVMK_Pal.Properties.Settings.Default.creditPreview)
            {
                //Check game user is in
                if (id == 2)
                {
                    //Jungle cruise (ID 2)
                    int score = pay.readScore(clientBrowser, 2);
                    int payout = pay.jungle(score);
                    toolStripStatusLabel3.Text = "Estimated Payout: " + payout + " credits |";
                }
                else //Otherwsie if not in a game
                {
                    toolStripStatusLabel3.Text = "";
                }
            }
        }

        //Download callback for events calendar
        private void DownloadFileCallback(Object sender, AsyncCompletedEventArgs e)
        {
            // If the request was not canceled and did not throw 
            // an exception, display the resource. 
            if (!e.Cancelled && e.Error == null)
            {
                toolStripButton6.Enabled = true;
            }
        }

        //Home button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            setStatus("MyVMK Pal");
            //Send user to home page
            clientBrowser.Source = new Uri(homeurl);
            isUserLoggedIn = false;
        }

        //Account manager button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //Create new account manager, and open
            Users u = new Users(this);
            u.Show();
        }

        //Screenshots button
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //Create new screenshot
            Screenshot screen = new Screenshot(clientBrowser);
            //Capture screen
            Bitmap screenshot = screen.capture();
            //Save to AppData
            screen.save(screenshot);
        }

        //Gallery button
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //Create new gallery and show
            Photos p = new Photos(this);
            p.Show();
        }

        //Events button
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Events ev = new Events(this);
            ev.Show();
        }

        //Settings button
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            //Create and open settings window
            Settings s = new Settings(this);
            s.Show();
        }

        //Stats button
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            //Create and open stats window
            Stats st = new Stats();
            st.Show();
        }

        //Credits counter
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            toolStripStatusLabel2.Text = "Credits: " + ch.getCredits();
            toolStripStatusLabel2.ForeColor = toolStripStatusLabel1.ForeColor;
        }

        private void checkForDlls()
        {
            //Checks to see if user has all nedded DLLs for myVMKPal to function
            string[] files = new string[7] { "Awesomium.Core.dll", "Awesomium.Windows.Forms.dll", "Newtonsoft.Json.dll", "awesomium.dll", "icudt.dll", "3xSX.dll", "tessnet2_32.dll" };
            //string[] files = new string[1] { "Newtonsoft.Json.dll" };
            WebClient client = new WebClient();
            Boolean hasMsg = false;
            foreach (string file in files)
            {
                if (!File.Exists(Directory.GetCurrentDirectory() + @"/" + file))
                {
                    //Restart program as admin if not
                    if (IsAdministrator() == false)
                    {
                        var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                        ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                        startInfo.Verb = "runas";
                        System.Diagnostics.Process.Start(startInfo);
                        this.Close();
                        return;
                    }
                    if (!hasMsg)
                    {
                        hasMsg = true;
                        MessageBox.Show("Setting up some new stuff! We should only have to do this once. Please close this, and wait for the next prompt.");
                    }
                    string uri = "http://myvmkpal.com/update/dll/" + file;
                    Console.WriteLine(uri);
                    client.DownloadFile(new Uri(uri), Directory.GetCurrentDirectory() + @"/" + file);
                }
            }
            //Checks to see if user has downloaded all language data (used in estimated payout)
            if (!Directory.Exists(Directory.GetCurrentDirectory() + @"/tessdata"))
            {
                if (IsAdministrator() == false)
                {
                    var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    this.Close();
                    return;
                }
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"/tessdata");
                string[] langFiles = new string[8] { "eng.DangAmbigs", "eng.freq-dawg", "eng.inttemp", "eng.normproto", "eng.pffmtable", "eng.unicharset", "eng.user-words", "eng.word-dawg" };

                foreach (string file in langFiles)
                {
                    if (!File.Exists(Directory.GetCurrentDirectory() + @"/tessdata/" + file))
                    {
                        if (!hasMsg)
                        {
                            hasMsg = true;
                            MessageBox.Show("Setting up some new stuff! We should only have to do this once. Please close this, and wait for the next prompt.");
                        }
                        string uri = "http://myvmkpal.com/update/lang/" + file;
                        Console.WriteLine(uri);
                        client.DownloadFile(new Uri(uri), Directory.GetCurrentDirectory() + @"/" + file);
                    }
                }
            }
            if (hasMsg)
            {
                MessageBox.Show("Done! Please restart MyVMK Pal to apply changes.");
                this.Close();
            }
        }

        //Is administrator?
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        //Check if is fresh version
        private void fresh()
        {
            string latest;
            string ll = String.Format("{0}\\MyVMK_Pal\\latest.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            if (!File.Exists(ll))
            {
                File.WriteAllText(ll, "NOPENOPENOPENOPENOPE");
            }
            using (StreamReader srv = new StreamReader(ll))
            {
                String line = srv.ReadToEnd();
                latest = line;
            }
            if (latest != currentversion)
            {
                File.Delete(ll);
                File.WriteAllText(ll, currentversion);
                if (MessageBox.Show("Welcome to version " + currentversion + "! See what's new at the changelog?", "New Version", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                {
                    System.Diagnostics.Process.Start("http://www.myvmkpal.com/changelog");
                }
                if (MyVMK_Pal.Properties.Settings.Default.isTester && !File.Exists(String.Format("{0}\\MyVMK_Pal\\UserSettings.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))))
                {
                    MyVMK_Pal.Properties.Settings.Default.isTester = false;
                    MyVMK_Pal.Properties.Settings.Default.Save();
                    MyVMK_Pal.Properties.Settings.Default.Reload();
                    MessageBox.Show("Tester login disabled. Re-enable in settings.");
                }
            }
        }

        //Check to see if agreement needed (analytics)
        public void analytics()
        {
            string has;
            string ll = String.Format("{0}\\MyVMK_Pal\\an.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            if (!File.Exists(ll))
            {
                File.WriteAllText(ll, "false");
            }
            using (StreamReader srv = new StreamReader(ll))
            {
                String line = srv.ReadToEnd();
                has = line;
            }
            if (has == "false")
            {
                File.Delete(ll);
                File.WriteAllText(ll, "true");
                MessageBox.Show("MyVMKPal collects usage data (time spent using, number of people using, user location). \n\n We understand if you do not wish to share this with us. In that case, you may simply opt out in the settings menu.");
            }
        }

        //Starts (or doesn't) anayltics
        public void allowAnalytics()
        {

            if (MyVMK_Pal.Properties.Settings.Default.analytics)
            {
                this.Controls.Add(anna);
                anna.Visible = false;
                anna.Source = new Uri("http://myvmk.analytic.sky3x.com/?inClient");
            }
            else
            {
                anna.Dispose();
            }
        }

        //PHRASES!
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            Phrases p = new Phrases(this);
            p.Show();
        }

        //Send phrase to chat
        public void sendPhrase(int phrase)
        {
            Console.WriteLine("Trying to write phrase... " + phrases[phrase]);
            Point loc = clientBrowser.PointToScreen(new Point());

            //Click on chat bar
            int x = loc.X + (clientBrowser.Width / 2);
            int y = loc.Y + (clientBrowser.Height - 10);

            uint xpos = (uint)x;
            uint ypos = (uint)y;

            Console.WriteLine("Location: " + loc.X + ", " + loc.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(50);

            //Fix special cases
            string ph = Regex.Replace(phrases[phrase], "[+^%~()]", "{$0}");
            //string ph = phrases[phrase];
            SendKeys.Send(ph);
            System.Threading.Thread.Sleep(200);
            SendKeys.Send("{ENTER}");
        }

        //Load phrases into list
        public void loadPhrases()
        {
            phrases.Clear();
            string _path = String.Format("{0}\\MyVMK_Pal\\Phrases.txt", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            if (File.Exists(_path))
            {
                string[] ph = File.ReadAllLines(_path);
                foreach (string phrase in ph)
                {
                    phrases.Add(phrase);
                }
            }
        }

        //Override Keys for hotkeys
        protected override void WndProc(ref Message m)
        {
            if (Form1.ActiveForm == this)
            {

                int WM_KEYDOWN = 0x0100;
                int WM_SYSKEYDOWN = 0x0104;
                int WM_CMDKEYS = 0x0312;
                
                if (m.Msg == WM_SYSKEYDOWN || m.Msg == WM_KEYDOWN || m.Msg == WM_CMDKEYS)
                {
                    hk.handle((int)m.LParam);
                }
                else
                {
                    base.WndProc(ref m);
                    
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        //Registers hotkeys
        private void registerCommands()
        {
            // Alt = 1, Ctrl = 2, Shift = 4, Win = 8
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'1');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'2');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'3');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'4');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'5');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'6');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'7');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'8');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'9');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 1, (int)'0');
            Form1.RegisterHotKey(this.Handle, this.GetType().GetHashCode(), 2, (int)Keys.Tab);
        }

        //Checks for screenshot directory set
        private void checkScreenshots()
        {
            if (MyVMK_Pal.Properties.Settings.Default.gal == "NULL")
            {
                string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string gal_path = String.Format("{0}\\MyVMK_Pal\\Screenshots", appdatapath);
                MyVMK_Pal.Properties.Settings.Default.gal = gal_path;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
            }
        }

        //Twitter Button
        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Form tweetForm = new Form();
            Awesomium.Windows.Forms.WebControl tweets = new Awesomium.Windows.Forms.WebControl();

            tweetForm.Controls.Add(tweets);

            tweetForm.Height = 660;
            tweetForm.Width = 520;

            tweets.Size = new Size(tweetForm.Width, tweetForm.Height);
            String code = @"<a class='twitter-timeline' data-dnt='true' href='https://twitter.com/MyVMK' data-widget-id='461290926791352321' data-chrome='nofooter noheader' style='font-family:Arial;color:#000000;text-decoration:none;'>Loading...</a><script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document,'script','twitter-wjs');</script>";

            tweetForm.Text = "@myVMK";
            tweetForm.Icon = GetExecutableIcon();

            tweetForm.Show();

            tweets.LoadHTML(code);
        }

        private Icon GetExecutableIcon()
        {
            IntPtr large;
            IntPtr small;
            ExtractIconEx(Application.ExecutablePath, 0, out large, out small, 1);
            return Icon.FromHandle(small);
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            Planner p = new Planner(this);
            p.ShowDialog();
        }

        private void checkNewSecurity()
        {
            if (MyVMK_Pal.Properties.Settings.Default.firstNS)
            {
                MyVMK_Pal.Properties.Settings.Default.firstNS = false;
                MyVMK_Pal.Properties.Settings.Default.autoLoginDetail = "";
                MyVMK_Pal.Properties.Settings.Default.doAutoLogin = false;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                MessageBox.Show("We have made security changes to myVMKPal. Your user data will be significantly more securely stored! \nHowever, this means that you must manually re-enter your saved accounts. \n\n We highly recommend you also remove the old user data, using the button provided in the account manager.");
                
            }
        }

        //Load events into list from planner.xml
        private void loadEvents()
        {
            string _path = String.Format("{0}\\MyVMK_Pal\\Planner.xml", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            if(!File.Exists(_path)) {
                XmlDocument doc = new XmlDocument();
                XmlNode events = doc.CreateElement("events");
                doc.AppendChild(events);
                doc.Save(_path);
            }
            XmlTextReader reader = new XmlTextReader(_path);
            int i = 0;
            string name = "";
            string time = "";
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Text:
                        i++;
                        if (i == 1)
                        {
                            name = reader.Value;
                        }
                        else if (i == 2)
                        {
                            time = reader.Value;
                            events.Add(name, time);
                            name = "";
                            time = "";
                            i = 0;
                        }
                        break;
                }
            }
            reader.Close();
        }

        //Removes buttons form toolStrip that user doesn't want
        public void setToolStrip()
        {
            string[] values = MyVMK_Pal.Properties.Settings.Default.toolItems.Select(x => x.ToString()).ToArray();
            Console.WriteLine(MyVMK_Pal.Properties.Settings.Default.toolItems);
            if (values.Length != 6) {
                Console.WriteLine("Something is wrong here.. resetting toolItems.");
                MyVMK_Pal.Properties.Settings.Default.toolItems = "tttttt";
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                values = MyVMK_Pal.Properties.Settings.Default.toolItems.Select(x => x.ToString()).ToArray();
            }

            int i = 0;

            foreach (ToolStripButton tb in toolStrip1.Items)
            {
                if (tb.Text != "Item ID Search")
                {
                    switch (tb.Text)
                    {
                        //These are all defaults, that can't be removed
                        case "Home":
                        case "Account Manager":
                        case "Take Screenshot":
                        case "Gallery":
                        case "Settings":
                        case "Pirates":
                            break;
                        default:
                            Console.WriteLine(i);
                            if (values[i] == "f")
                            {
                                tb.Visible = false;
                            }
                            else
                            {
                                tb.Visible = true;
                            }
                            i++;
                            break;
                    }

                }
            }
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            Newsletter n = new Newsletter();
            n.ShowDialog();
        }

        //Opens Pirates
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            //if (!MyVMK_Pal.Properties.Settings.Default.piratesActive)
            {
                if (!isItTimeYet())
                {
                    return;
                }
            }
            switch (MyVMK_Pal.Properties.Settings.Default.piratesType)
            {
                case 0:
                    //Popup
                    Pirates p = new Pirates(this);
                    p.Show();
                    break;
                case 1:
                    //Replace
                    if (!isUserLoggedIn)
                    {
                        PiratesUsers pu = new PiratesUsers(this);
                        pu.ShowDialog();
                        return;
                    }
                    if (!isSwapped)
                    {
                        clientBrowser.Source = new Uri(piratesUrl);
                        isSwapped = true;
                    }
                    else
                    {
                        clientBrowser.Source = new Uri("http://play.myvmk.com/client.php");
                        isSwapped = false;
                    }
                    
                    break;
                case 2:
                    //Swap
                    if (!isUserLoggedIn) { MessageBox.Show("You must already be logged into MyVMK to use swap mode."); return; }
                    if (!isSwapped)
                    {
                        piratesClient.Size = clientBrowser.Size;
                        piratesClient.Location = clientBrowser.Location;
                        if(firstSwap) {
                            piratesClient.Source = new Uri(piratesUrl);
                            firstSwap = false;
                        }
                        piratesClient.Visible = true;
                        clientBrowser.Visible = false;
                        isSwapped = true;
                    }
                    else
                    {
                        piratesClient.Visible = false;
                        clientBrowser.Visible = true;
                        isSwapped = false;
                    }
                    break;
                default:
                    MessageBox.Show("Error: piratesType out of bounds.\n\nThis should never occur. If you're seeing this, please contact us on the forums or by email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("Err; piratesType out of bounds.");
                    break;
            }
        }

        //Checks to see if Pirates is online yet (will be removed in 2.2, as not needed)
        private bool isItTimeYet()
        {
            WebClient wc = new WebClient();
            string isOpen = wc.DownloadString("http://myvmkpal.com/update/pirates.txt");
            if (isOpen != "false")
            {
                MyVMK_Pal.Properties.Settings.Default.piratesActive = true;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                return true;
            }
            else
            {
                MyVMK_Pal.Properties.Settings.Default.piratesActive = false;
                MyVMK_Pal.Properties.Settings.Default.Save();
                MyVMK_Pal.Properties.Settings.Default.Reload();
                MessageBox.Show("Avast ye!\n\nNow be not th' hour to set sail.");
                return false;
            }
        }
    }
}
