using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MaCross : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Parameter("SourceSeries")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        private MovingAverage _result;
        private MovingAverage _average;


        protected override void Initialize()
        {
            _result = Indicators.MovingAverage(SourceSeries, ResultPeriods, MAType);
            _average = Indicators.MovingAverage(SourceSeries, AveragePeriods, MAType);
        }

        public override void Calculate(int index)
        {
            Result[index] = _result.Result[index] / Symbol.PipSize;
            Average[index] = _average.Result[index] / Symbol.PipSize;
        }
    }
}
