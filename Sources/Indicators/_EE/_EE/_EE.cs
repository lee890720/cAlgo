using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _EE : Indicator
    {
        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol2 { get; set; }

        [Output("Main")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        private MarketSeries _symbol2Series;

        protected override void Initialize()
        {
            _symbol2Series = MarketData.GetSeries(Symbol2, TimeFrame);
        }

        public override void Calculate(int index)
        {
            var totalbar = MarketSeries.Close.Count;
            var sin = totalbar - index - 1;
            Result[index] = (_symbol2Series.Close.Last(sin) - MarketSeries.Close[index]) / Symbol.PipSize;
            double totalR = 0;
            for (var i = index; i > index - Period; i--)
                totalR += Result[i];
            Average[index] = totalR / Period;
        }
    }
}
