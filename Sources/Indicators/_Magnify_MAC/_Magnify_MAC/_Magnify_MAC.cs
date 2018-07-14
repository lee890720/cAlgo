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

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_B { get; set; }

        [Output("SigTwo", Color = Colors.Yellow, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigTwo { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Magnify", DefaultValue = 1)]
        public double Magnify { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double Sub { get; set; }

        private _Magnify_MaCross _mac;
        private _Magnify_MaSub _mas;

        public string SignalOne;
        public string SignalTwo;
        public int BarsAgo;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
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
            SignalOne = GetSigOne(index);
            if (SignalOne == "above")
                SigOne_A[index] = _mac.Result[index];
            if (SignalOne == "below")
                SigOne_B[index] = _mac.Result[index];
            SignalTwo = GetSigTwo(index);
            if (SignalTwo != null)
                SigTwo[index] = _mac.Result[index];
            #region Chart
            BarsAgo = _mac.BarsAgo;
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

        private string GetSigTwo(int index)
        {
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            double sr1 = _mas.Result[index - 1];
            double cBarsAgo = _mac.BarsAgo;
            if (sa > 0)
            {
                if (sr <= -Sub && sr1 > -Sub)
                {
                    for (int i = index - (int)cBarsAgo - 1; i < index; i++)
                    {
                        if (sr > _mas.Result[i])
                            return null;
                    }
                    return "belowTrend";
                }
            }
            if (sa < 0)
            {
                if (sr >= Sub && sr1 < Sub)
                {
                    for (int i = index - (int)cBarsAgo - 1; i < index; i++)
                    {
                        if (sr < _mas.Result[i])
                            return null;
                    }
                    return "aboveTrend";
                }
            }
            return null;
        }
    }
}
