using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Wave_Currency_Sub : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsRatio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        private Wave_Currency currency;

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Wave_Currency>(FirstSymbol, SecondSymbol, Period, IsRatio, Ratio);
        }

        public override void Calculate(int index)
        {
            Result[index] = currency.Result[index] - currency.Average[index];
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
