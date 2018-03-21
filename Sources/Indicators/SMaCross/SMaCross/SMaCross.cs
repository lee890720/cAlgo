using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using System;
using cAlgo.Lib;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SMaCross : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("Symbol")]
        public string _Symbol { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Magnify", DefaultValue = 1)]
        public double Magnify { get; set; }

        private SimpleMovingAverage _result;
        private SimpleMovingAverage _average;
        private MarketSeries _marketseries;
        private Symbol _symbol;

        protected override void Initialize()
        {
            _marketseries = MarketData.GetSeries(_Symbol, TimeFrame);
            _symbol = MarketData.GetSymbol(_Symbol);
            _result = Indicators.SimpleMovingAverage(_marketseries.Close, ResultPeriods);
            _average = Indicators.SimpleMovingAverage(_marketseries.Close, AveragePeriods);
        }

        public override void Calculate(int index)
        {
            DateTime dt = MarketSeries.OpenTime[index];
            int idx = _marketseries.GetIndexByDate(dt);
            if (Magnify == 1)
            {
                Result[index] = _result.Result[idx] / _symbol.PipSize;
                Average[index] = _average.Result[idx] / _symbol.PipSize;
            }
            else
            {
                Result[index] = _result.Result[idx] / _symbol.PipSize / Magnify;
                Average[index] = _average.Result[idx] / _symbol.PipSize / Magnify;
            }
        }
    }
}
