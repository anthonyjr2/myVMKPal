using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MyVMK_Pal
{
    class Screenshot
    {
        /*
         * MyVMK Pal Screenshot Handler
         */
        WebControl client;

        public Screenshot(WebControl client)
        {
            this.client = client;
        }

        //Capture method
        public Bitmap capture()
        {
            //Create new Bitmap
            Bitmap bmpScreenShot = new Bitmap(client.Width, client.Height);
            Graphics gfx = Graphics.FromImage((Image)bmpScreenShot);
            Point ctrlp = client.PointToScreen(Point.Empty);
            //Copy screen data, and return Bitmap from RAM
            gfx.CopyFromScreen(ctrlp.X, ctrlp.Y, 0, 0, new Size(client.Width, client.Height));
            return bmpScreenShot;
        }

        public void save(Bitmap screenshot)
        {
            //Check to see if screenshots directory exists
            dirExists("Screenshots");
            //Format save path
            string screenpath = MyVMK_Pal.Properties.Settings.Default.gal;
            string dateformatted = DateTime.Now.ToString("yyyyMMddHHmmss");
            string imagepath = String.Format("{0}\\vmk_{1}" + ".png", screenpath, dateformatted);
            //Save image as PNG
            screenshot.Save(imagepath, ImageFormat.Png);
        }

        //Checks if directory exists
        private Boolean dirExists(string location)
        {
            //Looks in ApData
            string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = appdatapath + "\\MyVMK_Pal\\" + location;

            if (Directory.Exists(path))
            {
                return true;
            }

            //Creates directory if does not exist
            DirectoryInfo di = Directory.CreateDirectory(path);
            return false;
        }
    }
}
