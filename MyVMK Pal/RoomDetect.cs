using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace MyVMK_Pal
{
    class RoomDetect
    {
        /*
         * Room detection MyVMK Pal
         */
        public string appdatapath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public string temp = String.Format("{0}\\MyVMK_Pal\\RoomData\\temp.vmk.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        public string jc = String.Format("{0}\\MyVMK_Pal\\RoomData\\temp.jc.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public RoomDetect()
        {
            string path = appdatapath + "\\MyVMK_Pal\\RoomData";
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
            }

            if (!File.Exists(jc))
            {
                WebClient wc = new WebClient();
                wc.DownloadFile("http://i.enx3s.com/0lfp91fY.jpg", jc);
            }
        }

        //Room detection algorithm
        public int roomDetect(Awesomium.Windows.Forms.WebControl wb, int bw)
        {
            int room = 0;
            //Take screenshot of region, store to temporary bitmap
            Bitmap bmpScreenShot = new Bitmap(17, 36);
            Graphics gfx = Graphics.FromImage((Image)bmpScreenShot);
            Point ctrlp = wb.PointToScreen(Point.Empty);
            try { gfx.CopyFromScreen(ctrlp.X, ctrlp.Y, 0, 0, new Size(17, 36)); }
            catch { return -1; }

            bmpScreenShot.Save(temp);

            //load jungle cruise bitmap
            Bitmap fw = MyVMK_Pal.Properties.Resources.jungle_cruise_rc;
            //Check to see if is equal
            if (IsEqual())
            {
                room = 2;
            }
            //Delete temporary file
            if (File.Exists(temp))
            {
                File.Delete(temp);
            }
            //Return room id
            return room;
        }

        //Check to see if files are equal
        public static bool IsEqual()
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            string file1 = String.Format("{0}\\MyVMK_Pal\\RoomData\\temp.jc.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            string file2 = String.Format("{0}\\MyVMK_Pal\\RoomData\\temp.vmk.jpg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read);
            fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }
    }
}
