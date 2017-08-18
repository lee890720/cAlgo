using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using cAlgo.Indicators;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MCAverage : Indicator
    {
        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol2 { get; set; }

        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        private MCResult mcr;

        protected override void Initialize()
        {
            mcr = Indicators.GetIndicator<MCResult>(Symbol2);
        }

        public override void Calculate(int index)
        {
            Result[index] = mcr.Result[index];
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += mcr.Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
