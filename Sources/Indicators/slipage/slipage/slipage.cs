using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class slipage : Indicator
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        [Output("Main")]
        public IndicatorDataSeries Result { get; set; }
        [Output("Main1")]
        public IndicatorDataSeries Average { get; set; }


        protected override void Initialize()
        {
            // Initialize and create nested indicators
        }

        public override void Calculate(int index)
        {
            Result[index] = Symbol.Spread;
            double Sum = 0.0;
            for (int i = index - 120 + 1; i <= 120; i++)
            {
                Sum += Result[i];
            }
            Average[index] = Sum / 120;
        }
    }
}
