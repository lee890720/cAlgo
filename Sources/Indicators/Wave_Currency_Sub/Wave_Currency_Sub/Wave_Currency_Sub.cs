using cAlgo.API;
using System;
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

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        private Wave_Currency currency;

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Wave_Currency>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            var c_index = currency.MarketSeries.OpenTime.GetIndexByExactTime(SymbolTime);
            Result[index] = currency.Result[c_index] - currency.Average[c_index];
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
