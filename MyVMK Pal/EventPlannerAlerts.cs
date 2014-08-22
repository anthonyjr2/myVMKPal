using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyVMK_Pal
{
    class EventPlannerAlerts
    {
        Form1 form;

        public EventPlannerAlerts(Form1 form)
        {
            this.form = form;
        }

        List<string> alertedFive = new List<string>();
        List<string> alertedOne = new List<string>();

        public void checkForEvents()
        {
            /* string time = "";
            form.events.TryGetValue(label1.Text, out time);
            DateTime timx = DateTime.Parse(time);
            string ampm = (timx.Hour > 12 ? "PM" : "AM");
            int hour = (timx.Hour > 12 ? (timx.Hour - 12) : timx.Hour);
            time = timx.Month + "/" + timx.Day + "/" + timx.Year + " " + hour + ampm; */

            //Check against list of events
            DateTime now = DateTime.Now;
            foreach (KeyValuePair<string, string> entry in form.events)
            {
                DateTime timx = DateTime.Parse(entry.Value);
                var diff = timx.Subtract(now);
                if(diff.Minutes == 5 && diff.Hours == 0) {
                    createAlert(entry.Key, 5);
                }
                else if (diff.Minutes == 1 && diff.Hours == 0)
                {
                    createAlert(entry.Key, 1);
                }
            }
        }

        private void createAlert(string eventName, int minutesUntil)
        {
            //Check when alert should be created
            if (minutesUntil == 5)
            {
                if (!alertedFive.Contains(eventName))
                {
                    //Show popup
                    MessageBox.Show("Heads up!\n" + eventName + " starts in 5 minutes!");
                    alertedFive.Add(eventName);
                }
            }
            else if (minutesUntil == 1)
            {
                if (!alertedOne.Contains(eventName))
                {
                    MessageBox.Show("Heads up!\n" + eventName + " starts in 1 minute!");
                    alertedOne.Add(eventName);
                }
            }
        }
    }
}
