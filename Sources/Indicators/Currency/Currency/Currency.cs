using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency : Indicator
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

        private MarketSeries _symbolFirstSeries, _symbolSecondSeries;
        protected override void Initialize()
        {
            _symbolFirstSeries = MarketData.GetSeries(FirstSymbol, TimeFrame);
            _symbolSecondSeries = MarketData.GetSeries(SecondSymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int FirstIndex = _symbolFirstSeries.GetIndexByDate(SymbolTime);
            int SecondIndex = _symbolSecondSeries.GetIndexByDate(SymbolTime);

            //FirstClose
            double FirstClose = 0;
            if (FirstSymbol.Substring(3, 3) == "USD")
                FirstClose = _symbolFirstSeries.Close[FirstIndex];
            if (FirstSymbol.Substring(0, 3) == "USD")
                FirstClose = 1 / _symbolFirstSeries.Close[FirstIndex];
            if (FirstSymbol.Substring(3, 3) == "JPY")
                FirstClose = 100 / _symbolFirstSeries.Close[FirstIndex];

            //SecondClose
            double SecondClose = 0;
            if (SecondSymbol.Substring(3, 3) == "USD")
                SecondClose = _symbolSecondSeries.Close[SecondIndex];
            if (SecondSymbol.Substring(0, 3) == "USD")
                SecondClose = 1 / _symbolSecondSeries.Close[SecondIndex];
            if (SecondSymbol.Substring(3, 3) == "JPY")
                SecondClose = 100 / _symbolSecondSeries.Close[SecondIndex];

            Result[index] = (FirstClose - SecondClose) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
