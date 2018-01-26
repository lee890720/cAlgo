using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TEST : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        private double ratio;
        double ratio2;

        protected override void OnStart()
        {
            ratio = 0.1234;
            Print(ratio.ToString("0.0000"));
            Print(Math.Round(ratio, 2).ToString("0.0000"));
            Print(Math.Round(ratio).ToString("0.0000"));
            ratio2 = 12.23;
            Print(ratio2.ToString("0.0000"));
            Print(ratio2.ToString("0.0000").Substring(0, 6));
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            this.Stop();
            // Put your core logic here
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
