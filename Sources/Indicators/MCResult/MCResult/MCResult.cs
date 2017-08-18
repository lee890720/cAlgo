using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MCResult : Indicator
    {
        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol2 { get; set; }

        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        private MarketSeries _symbol2Series;

        protected override void Initialize()
        {
            _symbol2Series = MarketData.GetSeries(Symbol2, TimeFrame);
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int index2 = _symbol2Series.GetIndexByDate(SymbolTime);
            Result[index] = (_symbol2Series.Close[index2] - MarketSeries.Close[index]) / Symbol.PipSize;
        }
    }
}
