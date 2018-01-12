using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Windows.Forms;
using System.Threading;

// To download the software please visit: https://clickalgo.com/ctrader-moving-average-instant-alert-messages"

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ClickAlgoSoftware : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        private static Mutex dialogMutex = new Mutex();
        private static bool dialogIsShownOnce = false;

        protected override void OnStart()
        {
            ShowDialogBox();
        }

        protected override void OnTick()
        {
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }

        public static void ShowDialogBox()
        {
            dialogMutex.WaitOne();

            if (dialogIsShownOnce)
                return;

            var ret = MessageBox.Show("It is not possible to download the software from the cTDN website.\nWould you like to visit us at ClickAlgo.com where you can download it?", "Downloading...", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (ret == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("https://clickalgo.com/ctrader-moving-average-instant-alert-messages");
            }

            dialogIsShownOnce = true;

            dialogMutex.ReleaseMutex();
        }
    }
}
