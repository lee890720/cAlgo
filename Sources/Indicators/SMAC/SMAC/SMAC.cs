using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SMAC : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_B { get; set; }

        [Parameter("Symbol")]
        public string _Symbol { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Magnify", DefaultValue = 1)]
        public double Magnify { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double Sub { get; set; }

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        public string SignalOne;
        public int BarsAgo;
        private SMaCross _mac;
        private SMaSub _mas;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _mac = Indicators.GetIndicator<SMaCross>(_Symbol, ResultPeriods, AveragePeriods, Magnify);
            _mas = Indicators.GetIndicator<SMaSub>(_Symbol, ResultPeriods, AveragePeriods, Magnify);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index];
            Average[index] = _mac.Average[index];
            string sigone = GetSigOne(index);
            if (sigone == "above")
                SigOne_A[index] = _mac.Result[index];
            if (sigone == "below")
                SigOne_B[index] = _mac.Result[index];

            #region Chart
            SignalOne = sigone;
            BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private string GetSigOne(int index)
        {
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            if (-Sub > sr && sr > sa && cr < ca)
                return "below";
            if (Sub < sr && sr < sa && cr > ca)
                return "above";
            return null;
        }

        private int GetBarsAgo(int index)
        {
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            if (cr > ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] <= _mac.Average[i])
                        return index - i;
                }
            if (cr < ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] >= _mac.Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
