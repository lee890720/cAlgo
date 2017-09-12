using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ClosePosition : Robot
    {
        [Parameter("label")]
        public string label { get; set; }

        [Parameter("comment")]
        public string comment { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            foreach (var pos in Positions)
            {
                if (pos.Label == label)
                    ClosePosition(pos);
                if (pos.Comment == comment)
                    ClosePosition(pos);
            }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
