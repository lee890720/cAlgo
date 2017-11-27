using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Sub_EURCHF : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        private USD_EURCHF usd_eurchf;

        protected override void Initialize()
        {
            usd_eurchf = Indicators.GetIndicator<USD_EURCHF>(Period);
        }

        public override void Calculate(int index)
        {
            Result[index] = usd_eurchf.Result[index] - usd_eurchf.Average[index];
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
