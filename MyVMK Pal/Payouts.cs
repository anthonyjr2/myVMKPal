using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MyVMK_Pal
{
    class Payouts
    {
        public Payouts() {}

        public int jungle(int score)
        {
            //Calculation for users score (round up to nearest whole of 2% of the score)
            return (int)Math.Ceiling((score * 0.02));
        }

        public int fireworks(int score)
        {
            return score;
        }

        public int readScore(Awesomium.Windows.Forms.WebControl clientBrowser, int gameID)
        {
            int score = 0;
            int[] size = new int[2];
            int fontColor = 255;

            switch (gameID)
            {
                case 1:
                    //Fireworks
                    break;
                case 2:
                    //Jungle Cruise
                    size[0] = 94;
                    size[1] = 36;
                    fontColor = 239;
                    break;
                default:
                    //Something went wrong
                    return 0;
            }

            //Take screenshot of region, store to temporary bitmap
            Bitmap scoreShot = new Bitmap(size[0], size[1]);
            Graphics gfx = Graphics.FromImage((Image)scoreShot);
            Point ctrlp = clientBrowser.PointToScreen(Point.Empty);
            try { gfx.CopyFromScreen(ctrlp.X, ctrlp.Y, 0, 0, new Size(size[0], size[1])); }
            catch { return -1; }

            //Replace everything except characters with black
            for (int x = 0; x < scoreShot.Width; x++)
            {
                for (int y = 0; y < scoreShot.Height; y++)
                {
                    Color gotColor = scoreShot.GetPixel(x, y);
                    if (gotColor != Color.FromArgb(fontColor, fontColor, fontColor))
                    {
                        scoreShot.SetPixel(x, y, Color.Black);
                    }
                    
                }
            }

            //Use tessnet2 to extract score via OCR

            Ocr ocr = new Ocr();
            List<tessnet2.Word> res = new List<tessnet2.Word>();
            using (Bitmap bmp = scoreShot)
            {
                tessnet2.Tesseract tessocr = new tessnet2.Tesseract();
                tessocr.Init(null, "eng", false);
                tessocr.GetThresholdedImage(bmp, Rectangle.Empty).Save("c:\\x\\" + Guid.NewGuid().ToString() + ".bmp");
                // Tessdata directory must be in the same directory as this exe
                res = ocr.DoOCRNormal(bmp, "eng");
            }

            foreach (tessnet2.Word word in res)
            {
                bool isNumeric = int.TryParse(word.Text, out score);
                if (!isNumeric)
                {
                    score = 0;
                }
            }

            //Return the score value
            return score;
        }
    }

    public class Ocr
    {
        public void DumpResult(List<tessnet2.Word> result)
        {
            foreach (tessnet2.Word word in result)
                Console.WriteLine("{0} : {1}", word.Confidence, word.Text);
        }

        public List<tessnet2.Word> DoOCRNormal(Bitmap image, string lang)
        {
            tessnet2.Tesseract ocr = new tessnet2.Tesseract();
            ocr.Init(null, lang, false);
            List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
            return result;
        }

        public void Finished(List<tessnet2.Word> result)
        {
            DumpResult(result);
        }

        void ocr_ProgressEvent(int percent)
        {
            Console.WriteLine("{0}% progression", percent);
        }
    }
}
