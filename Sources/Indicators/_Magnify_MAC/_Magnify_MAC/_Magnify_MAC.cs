using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _Magnify_MAC : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("Sig1_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries Sig1_A { get; set; }

        [Output("Sig1_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries Sig1_B { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Magnify", DefaultValue = 1)]
        public double Magnify { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double Sub { get; set; }

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        public string _Signal1;
        public int _BarsAgo;
        private _Magnify_MaCross _mac;
        private _Magnify_MaSub _mas;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _mac = Indicators.GetIndicator<_Magnify_MaCross>(ResultPeriods, AveragePeriods, Magnify);
            _mas = Indicators.GetIndicator<_Magnify_MaSub>(ResultPeriods, AveragePeriods, Magnify);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index];
            Average[index] = _mac.Average[index];
            string Sig1 = GetSig1(index);
            if (Sig1 == "above")
                Sig1_A[index] = _mac.Result[index];
            if (Sig1 == "below")
                Sig1_B[index] = _mac.Result[index];

            #region Chart
            _Signal1 = Sig1;
            _BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + _BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private string GetSig1(int index)
        {
            double CR = _mac.Result[index];
            double CA = _mac.Average[index];
            double SR = _mas.Result[index];
            double SA = _mas.Average[index];
            if (-Sub > SR && SR > SA && CR < CA)
                return "below";
            if (Sub < SR && SR < SA && CR > CA)
                return "above";
            return null;
        }

        private int GetBarsAgo(int index)
        {
            double CR = _mac.Result[index];
            double CA = _mac.Average[index];
            if (CR > CA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] <= _mac.Average[i])
                        return index - i;
                }
            if (CR < CA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] >= _mac.Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
