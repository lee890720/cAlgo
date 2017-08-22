using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MC_EURJPY : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        private int Period = 120;
        private string BigSymbol = "EURUSD";
        private string SmallSymbol = "USDJPY";
        private MarketSeries _symbolbigSeries, _symbolsmallSeries;
        protected override void Initialize()
        {
            _symbolbigSeries = MarketData.GetSeries(BigSymbol, TimeFrame);
            _symbolsmallSeries = MarketData.GetSeries(SmallSymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            DateTime BigSymbolTime = _symbolbigSeries.OpenTime[index];
            DateTime SmallSymbolTime = _symbolsmallSeries.OpenTime[index];
            int BigIndex = _symbolbigSeries.GetIndexByDate(SmallSymbolTime);
            int SmallIndex = _symbolsmallSeries.GetIndexByDate(BigSymbolTime);
            if (Symbol.Code == BigSymbol)
            {
                Result[index] = (_symbolbigSeries.Close[index] - 100 / _symbolsmallSeries.Close[SmallIndex]) / 0.0001;
            }
            if (Symbol.Code == SmallSymbol)
            {
                Result[index] = (_symbolbigSeries.Close[BigIndex] - 100 / _symbolsmallSeries.Close[index]) / 0.0001;
            }
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
