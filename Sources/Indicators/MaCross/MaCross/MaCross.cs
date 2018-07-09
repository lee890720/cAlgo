using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MaCross : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        public int BarsAgo;
        private SimpleMovingAverage _result;
        private SimpleMovingAverage _average;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _result = Indicators.SimpleMovingAverage(MarketSeries.Close, ResultPeriods);
            _average = Indicators.SimpleMovingAverage(MarketSeries.Close, AveragePeriods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _result.Result[index] / Symbol.PipSize;
            Average[index] = _average.Result[index] / Symbol.PipSize;

            #region Chart
            BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private int GetBarsAgo(int index)
        {
            double cr = _result.Result[index];
            double ca = _average.Result[index];
            if (cr > ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_result.Result[i] <= _average.Result[i])
                        return index - i;
                }
            if (cr < ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_result.Result[i] >= _average.Result[i])
                        return index - i;
                }
            return -1;
        }
    }
}
