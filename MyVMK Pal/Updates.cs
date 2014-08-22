using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Security.Principal;
using System.Xml;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace MyVMK_Pal
{
    class Updates
    {
        /*
         * Handles updating the updater.
         * Run in background. You wouldn't often know this takes place.
         */ 

        Form1 form;
        string currentversion;
        string updaterversion;

        public Updates(Form1 form, string currentversion, string updaterversion)
        {
            this.currentversion = currentversion;
            this.updaterversion = updaterversion;
            this.form = form;
        }

        public void updateUpdater()
        {
            string path = Directory.GetCurrentDirectory() + @"/updater.txt";
            bool needs = false;
            if (!File.Exists(path))
            {
                needs = true;
            }
            else
            {
                string ver = File.ReadAllText(path);
                if (ver != updaterversion)
                {
                    needs = true;
                }
            }
            if (needs)
            {
                string updateUrl = "http://myvmkpal.com/update/Updater.exe";

                //Start download
                using (WebClient w = new WebClient())
                {
                    try
                    {
                        if (IsAdministrator() == false)
                        {
                            var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                            ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                            startInfo.Verb = "runas";
                            System.Diagnostics.Process.Start(startInfo);
                            Environment.Exit(0);
                            return;
                        }
                        //Download  async
                        w.DownloadFileAsync(new Uri(updateUrl), "Updater.exe.update");
                        w.DownloadFileCompleted += w_DownloadFileCompleted;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
        }

        public Boolean checkUpdates()
        {
            System.Net.HttpWebRequest request = (HttpWebRequest)System.Net.HttpWebRequest.Create("http://myvmkpal.com/update/version.txt");
            System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream());
            string newestversion = sr.ReadToEnd();
            if (newestversion == currentversion)
            {
                return false;
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("A new version of MyVMK Pal (" + newestversion + ") is available. Would you like to download it now?", "Update Available", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {

                    Process process = null;
                    ProcessStartInfo processStartInfo = new ProcessStartInfo();

                    processStartInfo.FileName = Directory.GetCurrentDirectory() + "\\Updater.exe";

                    processStartInfo.Verb = "runas";
                    processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                    processStartInfo.UseShellExecute = true;

                    process = Process.Start(processStartInfo);
                    //System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "\\Updater.exe");
                    //process.Dispose();
                    //form.Close();
                    //Environment.Exit(0);
                }
            }
            return true;
        }
        public void checkIsNew()
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
            }
        }

        //Upon completion
        void w_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            string dir = Directory.GetCurrentDirectory();
            string path = Directory.GetCurrentDirectory() + @"/updater.txt";

            //Delete MyVMKPal.exe and replace
            try
            {
                File.Delete(dir + @"\Updater.exe");
                File.Move(dir + @"\Updater.exe.update", dir + @"\Updater.exe");
                File.Delete(dir + @"\Updater.exe.update");
                File.WriteAllText(path, updaterversion);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error has occured while updating the update utility." + "\n\n" + ex.Message);
            }
        }

        //Check if user is administrator
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void checkRuntimeActivationPolicy()
        {
            string file = Directory.GetCurrentDirectory() + @"\MyVMK Pal.exe.config";
            XmlDocument doc = new XmlDocument();
            bool needSave = false;
            doc.Load(file);
            try
            {
                //Check to see if useLegacyV2RuntimeActivationPolicy is true
                string attrVal = doc.SelectSingleNode("/configuration/startup/@useLegacyV2RuntimeActivationPolicy").Value;
                if (attrVal != "true")
                {
                    Console.WriteLine("Well, it's there. But it's set to false");
                    throw new Exception();
                }
                else
                {
                    Console.WriteLine("Found it! It's currently " + attrVal);
                }
            }
            catch
            {
                //Create or fixing
                if (!IsAdministrator())
                {
                    //Make sure to restart if it's not administrator
                    var exeName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    ProcessStartInfo startInfo = new ProcessStartInfo(exeName);
                    startInfo.Verb = "runas";
                    System.Diagnostics.Process.Start(startInfo);
                    Environment.Exit(0);
                    return;
                }
                Console.WriteLine("Creating/Fixing.");
                needSave = true;
                //Create attribute and set value
                XmlAttribute attr = doc.CreateAttribute("useLegacyV2RuntimeActivationPolicy");
                attr.Value = "true";
                //Add/set the attribute
                XmlNodeList n = doc.DocumentElement.GetElementsByTagName("startup");
                XmlElement s = (XmlElement)n[0];
                s.SetAttributeNode(attr);
            }
            finally
            {
                if (needSave)
                {
                    //Save the file if needed
                    doc.Save(file);
                }
            } 
        }
    }
}
