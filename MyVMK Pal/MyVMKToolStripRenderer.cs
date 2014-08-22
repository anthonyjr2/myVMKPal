using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyVMK_Pal
{
    public class MyVMKToolStripRenderer : ToolStripProfessionalRenderer
    {
        public MyVMKToolStripRenderer()
        {
            //Fix weird formatting
            this.RoundedEdges = false;
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
}
