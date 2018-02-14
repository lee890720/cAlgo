using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_MAS : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("Sig1_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries Sig1_A { get; set; }

        [Output("Sig1_B", Color = Colors.OrangeRed, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries Sig1_B { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double Sub { get; set; }

        public string _Signal1;
        public int _BarsAgo;
        public string _Mark;
        private Oil_MaCross _mac;
        private Oil_MaSub _mas;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _mac = Indicators.GetIndicator<Oil_MaCross>(ResultPeriods, AveragePeriods);
            _mas = Indicators.GetIndicator<Oil_MaSub>(ResultPeriods, AveragePeriods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mas.Result[index];
            Average[index] = _mas.Average[index];
            string Sig1 = GetSig1(index);
            if (Sig1 == "below")
                Sig1_B[index] = _mas.Result[index];
            else
                Sig1_B[index] = 0;
            if (Sig1 == "above")
                Sig1_A[index] = _mas.Result[index];
            else
                Sig1_A[index] = 0;

            #region Chart
            _Signal1 = Sig1;
            _BarsAgo = GetBarsAgo(index);
            _Mark = GetMark(index).ToString("yyyy-MM-dd") + "-" + GetMark(index).ToString("HH");
            if (_Signal1 == null)
                ChartObjects.DrawText("sig", "NoSignal", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("sig", "Signal_(" + _Signal1 + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("barsago", "\nCross_(" + _BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("mark", "\n\nMark_(" + _Mark + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("break", "\n\n\n" + GetBreak(index), StaticPosition.TopLeft, _nocorel);
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
            double SR = _mas.Result[index];
            double SA = _mas.Average[index];
            if (SR > SA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] <= _mas.Average[i])
                        return index - i;
                }
            if (SR < SA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] >= _mas.Average[i])
                        return index - i;
                }
            return -1;
        }

        private DateTime GetMark(int index)
        {
            int IDX = index - _BarsAgo;
            DateTime DT = MarketSeries.OpenTime[IDX];
            return DT;
        }

        private string GetBreak(int index)
        {
            string BR = null;
            int SUB = 0;
            int Initmax = 0;
            int T1 = 0;
            int T2 = 0;
            int T3 = 0;
            int T4 = 0;
            int T5 = 0;
            int TotalBars = MarketSeries.Bars();
            int Per = AveragePeriods * 10;
            if (TotalBars < AveragePeriods * 10)
                Per = TotalBars;
            double MaxBar = _mas.Result.Maximum(Per);
            double MinBar = _mas.Result.Minimum(Per);
            double GetMax = Math.Round(MaxBar > Math.Abs(MinBar) ? MaxBar : Math.Abs(MinBar));
            if (GetMax < 100)
            {
                SUB = 10;
                Initmax = 100;
            }
            else if (GetMax < 150)
            {
                SUB = 15;
                Initmax = 150;
            }
            else if (GetMax < 200)
            {
                SUB = 20;
                Initmax = 200;
            }
            else if (GetMax < 250)
            {
                SUB = 25;
                Initmax = 250;
            }
            else if (GetMax < 300)
            {
                SUB = 30;
                Initmax = 300;
            }
            else if (GetMax < 350)
            {
                SUB = 35;
                Initmax = 350;
            }
            else if (GetMax < 400)
            {
                SUB = 40;
                Initmax = 400;
            }
            else
            {
                SUB = 50;
                Initmax = 500;
            }

            for (int i = ((TotalBars > AveragePeriods * 10) ? (TotalBars - AveragePeriods * 10) : 0); i < TotalBars; i++)
            {
                var SR = Math.Abs(_mas.Result[i]);
                if (SR > Initmax - SUB * 0)
                {
                    T5++;
                    T4++;
                    T3++;
                    T2++;
                    T1++;
                    continue;
                }
                if (SR > Initmax - SUB * 1)
                {
                    T4++;
                    T3++;
                    T2++;
                    T1++;
                    continue;
                }
                if (SR > Initmax - SUB * 2)
                {
                    T3++;
                    T2++;
                    T1++;
                    continue;
                }
                if (SR > Initmax - SUB * 3)
                {
                    T2++;
                    T1++;
                    continue;
                }
                if (SR > Initmax - SUB * 4)
                {
                    T1++;
                    continue;
                }
            }
            BR = "(" + Math.Round(150 / GetMax, 3).ToString() + "-" + Per.ToString() + "-" + GetMax.ToString() + ")_(" + Initmax.ToString() + "-" + SUB.ToString() + ")_(" + (Initmax - SUB * 4).ToString() + "-" + T1.ToString() + ")_(" + (Initmax - SUB * 3).ToString() + "-" + T2.ToString() + ")_(" + (Initmax - SUB * 2).ToString() + "-" + T3.ToString() + ")_(" + (Initmax - SUB * 1).ToString() + "-" + T4.ToString() + ")_(" + (Initmax - SUB * 0).ToString() + "-" + T5.ToString() + ")";
            return BR;
        }
    }
}
