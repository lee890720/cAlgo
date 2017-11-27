using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Sub_AUDNZD : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        private USD_AUDNZD usd_audnzd;

        protected override void Initialize()
        {
            usd_audnzd = Indicators.GetIndicator<USD_AUDNZD>(Period);
        }

        public override void Calculate(int index)
        {
            Result[index] = usd_audnzd.Result[index] - usd_audnzd.Average[index];
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
