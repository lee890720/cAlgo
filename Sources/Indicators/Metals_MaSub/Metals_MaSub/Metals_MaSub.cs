using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Metals_MaSub : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        private Metals_MaCross _mac;

        protected override void Initialize()
        {
            _mac = Indicators.GetIndicator<Metals_MaCross>(ResultPeriods, AveragePeriods);
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index] - _mac.Average[index];
            double Sum = 0.0;
            for (int i = index - AveragePeriods + 1; i <= index; i++)
            {
                Sum += Result[i];
            }
            Average[index] = Sum / AveragePeriods;
        }
    }
}
