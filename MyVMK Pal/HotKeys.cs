using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace MyVMK_Pal
{
    class HotKeys
    {
        /*
         * Handles hot keys for phrases and screenshots
         */ 
        Form1 form;
        Awesomium.Windows.Forms.WebControl client;

        public HotKeys(Form1 form, Awesomium.Windows.Forms.WebControl client) {
            this.form = form;
            this.client = client;
        }
       
        public void handle(int keys)
        {
            switch (keys)
            {
                //START PHRASES
                case 3211265:
                    //Phrase 1
                    form.sendPhrase(0);
                    break;
                case 3276801:
                    //Phrase 2
                    form.sendPhrase(1);
                    break;
                case 3342337:
                    //Phrase 3
                    form.sendPhrase(2);
                    break;
                case 3407873:
                    //Phrase 4
                    form.sendPhrase(3);
                    break;
                case 3473409:
                    //Phrase 5
                    form.sendPhrase(4);
                    break;
                case 3538945:
                    //Phrase 6
                    form.sendPhrase(5);
                    break;
                case 3604481:
                    //Phrase 7
                    form.sendPhrase(6);
                    break;
                case 3670017:
                    //Phrase 8
                    form.sendPhrase(7);
                    break;
                case 3735553:
                    //Phrase 9
                    form.sendPhrase(8);
                    break;
                case 3145729:
                    //Phrase 10
                    form.sendPhrase(9);
                    break;
                case 589826:
                    Console.WriteLine("Took screenshot");
                    //Create new screenshot
                    Screenshot screen = new Screenshot(client);
                    //Capture screen
                    Bitmap screenshot = screen.capture();
                    //Save to AppData
                    screen.save(screenshot);
                    break;
                default:
                    
                    break;
                //END PHRASES
            }
        }
    }
}
