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
    public partial class BlackBox : Form
    {
        //Mouse constants, to make suer tha user cannot interact with form
        private const int WM_MOUSEACTIVATE = 0x0021, MA_NOACTIVATE = 0x0003;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        //Detect mouse clicks on form
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle = WS_EX_NOACTIVATE;
                return createParams;
            }
        }

        public BlackBox()
        {
            InitializeComponent();
        }

        private void BlackBox_Load(object sender, EventArgs e)
        {
            /*
             * This form only used for full screen. No controls or init needed.
             */ 
        }
    }
}
