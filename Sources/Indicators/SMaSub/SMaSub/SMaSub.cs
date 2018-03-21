using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SMaSub : Indicator
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

        private SMaCross _macross;

        protected override void Initialize()
        {
            _macross = Indicators.GetIndicator<SMaCross>(_Symbol, ResultPeriods, AveragePeriods, Magnify);
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
        }
    }
}
