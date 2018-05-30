using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MAS : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_B { get; set; }

        [Output("SigTwo", Color = Colors.Yellow, PlotType = PlotType.Histogram, LineStyle = LineStyle.Dots, Thickness = 1)]
        public IndicatorDataSeries SigTwo { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double Sub { get; set; }

        [Parameter("Break", DefaultValue = 100)]
        public double Brk { get; set; }

        public string SignalOne;
        public string SignalTwo;
        public int BarsAgo;
        public string Mark;
        private MaCross _mac;
        private MaSub _mas;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _mac = Indicators.GetIndicator<MaCross>(ResultPeriods, AveragePeriods);
            _mas = Indicators.GetIndicator<MaSub>(ResultPeriods, AveragePeriods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mas.Result[index];
            Average[index] = _mas.Average[index];

            string sigtwo = "";
            if (Result[index] > 0)
            {
                if (Average[index] > 0)
                {
                    if (Result[index] > Average[index])
                    {
                        SigTwo[index] = Result[index] - Average[index];
                        if (SigTwo[index] > Brk)
                            sigtwo = "aboveBreak";
                    }
                    else
                        SigTwo[index] = 0;
                }
                else
                    SigTwo[index] = 0;
            }
            else
            {
                if (Average[index] < 0)
                {
                    if (Result[index] < Average[index])
                    {
                        SigTwo[index] = Math.Abs(Result[index] - Average[index]);
                        if (SigTwo[index] > Brk)
                            sigtwo = "belowBreak";
                    }
                    else
                        SigTwo[index] = 0;
                }
                else
                    SigTwo[index] = 0;
            }

            string sigone = GetSigOne(index);
            if (sigone == "below")
                SigOne_B[index] = _mas.Result[index];
            else
                SigOne_B[index] = 0;
            if (sigone == "above")
                SigOne_A[index] = _mas.Result[index];
            else
                SigOne_A[index] = 0;

            #region Chart
            SignalOne = sigone;
            SignalTwo = sigtwo;
            BarsAgo = GetBarsAgo(index);
            Mark = GetMark(index).ToString("yyyy-MM-dd") + "-" + GetMark(index).ToString("HH");
            if (string.IsNullOrEmpty(SignalOne))
                ChartObjects.DrawText("sigone", "NoSignal-1", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("sigone", "Signal1_(" + SignalOne + ")", StaticPosition.TopLeft, _nocorel);
            if (string.IsNullOrEmpty(SignalTwo))
                ChartObjects.DrawText("sigtwo", "\nNoSignal-2", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("sigtwo", "\nSignal2_(" + SignalTwo + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("barsago", "\n\nCross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("mark", "\n\n\nMark_(" + Mark + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("break", "\n\n\n\n" + GetBreak(index), StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawHorizontalLine("breakLine", Brk, Colors.DarkCyan);
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
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            if (sr > sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] <= _mas.Average[i])
                        return index - i;
                }
            if (sr < sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] >= _mas.Average[i])
                        return index - i;
                }
            return -1;
        }

        private DateTime GetMark(int index)
        {
            int idx = index - BarsAgo;
            DateTime dt = MarketSeries.OpenTime[idx];
            return dt;
        }

        private string GetBreak(int index)
        {
            string br = null;
            int sub = 0;
            int initmax = 0;
            int t1 = 0;
            int t2 = 0;
            int t3 = 0;
            int t4 = 0;
            int t5 = 0;
            int bars = MarketSeries.Bars();
            int per = AveragePeriods * 10;
            if (bars < AveragePeriods * 10)
                per = bars;
            double maxbar = _mas.Result.Maximum(per);
            double minbar = _mas.Result.Minimum(per);
            double getmax = Math.Round(maxbar > Math.Abs(minbar) ? maxbar : Math.Abs(minbar));
            if (getmax < 100)
            {
                sub = 10;
                initmax = 100;
            }
            else if (getmax < 150)
            {
                sub = 15;
                initmax = 150;
            }
            else if (getmax < 200)
            {
                sub = 20;
                initmax = 200;
            }
            else if (getmax < 250)
            {
                sub = 25;
                initmax = 250;
            }
            else if (getmax < 300)
            {
                sub = 30;
                initmax = 300;
            }
            else if (getmax < 350)
            {
                sub = 35;
                initmax = 350;
            }
            else if (getmax < 400)
            {
                sub = 40;
                initmax = 400;
            }
            else
            {
                sub = 50;
                initmax = 500;
            }

            for (int i = ((bars > AveragePeriods * 10) ? (bars - AveragePeriods * 10) : 0); i < bars; i++)
            {
                var sr = Math.Abs(_mas.Result[i]);
                if (sr > initmax - sub * 0)
                {
                    t5++;
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 1)
                {
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 2)
                {
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 3)
                {
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 4)
                {
                    t1++;
                    continue;
                }
            }
            br = "(" + Math.Round(150 / getmax, 3).ToString() + "-" + per.ToString() + "-" + getmax.ToString() + ")_(" + initmax.ToString() + "-" + sub.ToString() + ")_(" + (initmax - sub * 4).ToString() + "-" + t1.ToString() + ")_(" + (initmax - sub * 3).ToString() + "-" + t2.ToString() + ")_(" + (initmax - sub * 2).ToString() + "-" + t3.ToString() + ")_(" + (initmax - sub * 1).ToString() + "-" + t4.ToString() + ")_(" + (initmax - sub * 0).ToString() + "-" + t5.ToString() + ")";
            return br;
        }
    }
}
