using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_MaSub : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        private Oil_MaCross _macross;

        public int BarsAgo;

        private Colors _nocorel;

        protected override void Initialize()
        {
            _macross = Indicators.GetIndicator<Oil_MaCross>(ResultPeriods, AveragePeriods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _macross.Result[index] - _macross.Average[index];
            double sum = 0.0;
            for (int i = index - AveragePeriods + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / AveragePeriods;

            #region Chart
            BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private int GetBarsAgo(int index)
        {
            double sr = Result[index];
            double sa = Average[index];
            if (sr > sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (Result[i] <= Average[i])
                        return index - i;
                }
            if (sr < sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (Result[i] >= Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
