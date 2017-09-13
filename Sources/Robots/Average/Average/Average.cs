using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Average : Robot
    {
        [Parameter("label")]
        public string label { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            var average = this.AveragePrice(label);
            ChartObjects.DrawText("text", Math.Round(average, 4).ToString(), MarketSeries.Bars() - 1, average, VerticalAlignment.Top);
            ChartObjects.DrawHorizontalLine("average", average, Colors.White, 2, LineStyle.Lines);
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
