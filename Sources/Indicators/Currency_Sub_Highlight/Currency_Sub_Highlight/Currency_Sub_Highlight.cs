using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency_Sub_Highlight : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.OrangeRed, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Sub { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        public string SIG;
        public int BarsAgo_Sub;
        public string Mark;
        private Currency currency;
        private Currency_Sub currency_sub;
        //private Colors PCorel;
        //private Colors NCorel;
        private Colors NoCorel;

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Currency>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
            currency_sub = Indicators.GetIndicator<Currency_Sub>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
            //PCorel = Colors.Lime;
            //NCorel = Colors.OrangeRed;
            NoCorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = currency_sub.Result[index];
            Average[index] = currency_sub.Average[index];
            string sig = signal(index);
            if (sig == "below")
                sig_Result_B[index] = currency_sub.Result[index];
            else
                sig_Result_B[index] = 0;
            if (sig == "above")
                sig_Result_A[index] = currency_sub.Result[index];
            else
                sig_Result_A[index] = 0;

            SIG = sig;
            BarsAgo_Sub = barsago(index);
            Mark = mark(index).ToString("yyyy-MM-dd") + "-" + mark(index).ToString("HH");
            if (SIG == null)
                ChartObjects.DrawText("sig", "No-Signal", StaticPosition.TopLeft, NoCorel);
            else
                ChartObjects.DrawText("sig", "Signal-" + SIG, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("barsago", "\nCross-" + BarsAgo_Sub.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("mark", "\n\nMark-" + Mark, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("above", "\n\n\n" + getbreak(index), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawHorizontalLine("midline", 0, NoCorel);
        }

        private string signal(int index)
        {
            double u_result = currency.Result[index];
            double u_average = currency.Average[index];
            double s_result = currency_sub.Result[index];
            double s_average = currency_sub.Average[index];
            if (-Sub > s_result && s_result > s_average && u_result < u_average)
                return "below";
            if (Sub < s_result && s_result < s_average && u_result > u_average)
                return "above";
            return null;
        }

        private int barsago(int index)
        {
            double s_result = currency_sub.Result[index];
            double s_average = currency_sub.Average[index];
            if (s_result > s_average)
                for (int i = index - 1; i > 0; i--)
                {
                    if (currency_sub.Result[i] <= currency_sub.Average[i])
                        return index - i;
                }
            if (s_result < s_average)
                for (int i = index - 1; i > 0; i--)
                {
                    if (currency_sub.Result[i] >= currency_sub.Average[i])
                        return index - i;
                }
            return -1;
        }

        private DateTime mark(int index)
        {
            int idx = index - BarsAgo_Sub;
            DateTime dt = MarketSeries.OpenTime[idx];
            return dt;
        }

        private string getbreak(int index)
        {
            string _break = null;
            int sub = 0;
            int initmax = 0;
            int t1 = 0;
            int t2 = 0;
            int t3 = 0;
            int t4 = 0;
            int t5 = 0;
            int bars = MarketSeries.Bars();
            int period = Period * 10;
            if (bars < Period * 10)
                period = bars;
            double maxbar = currency_sub.Result.Maximum(period);
            double minbar = currency_sub.Result.Minimum(period);
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
                sub = 25;
                initmax = 200;
            }
            else if (getmax < 300)
            {
                sub = 50;
                initmax = 300;
            }
            else if (getmax < 600)
            {
                sub = 100;
                initmax = 600;
            }
            else
            {
                sub = 200;
                initmax = 1200;
            }

            for (int i = ((bars > Period * 10) ? (bars - Period * 10) : 0); i < bars; i++)
            {
                var SR = Math.Abs(currency_sub.Result[i]);
                if (SR > initmax - sub * 0)
                {
                    t5++;
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (SR > initmax - sub * 1)
                {
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (SR > initmax - sub * 2)
                {
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (SR > initmax - sub * 3)
                {
                    t2++;
                    t1++;
                    continue;
                }
                if (SR > initmax - sub * 4)
                {
                    t1++;
                    continue;
                }
            }
            _break = initmax.ToString() + "_" + sub.ToString() + "_" + getmax.ToString() + "_" + Math.Round(150 / getmax, 3).ToString() + "_" + (initmax - sub * 4).ToString() + "-" + t1.ToString() + "_" + (initmax - sub * 3).ToString() + "-" + t2.ToString() + "_" + (initmax - sub * 2).ToString() + "-" + t3.ToString() + "_" + (initmax - sub * 1).ToString() + "-" + t4.ToString() + "_" + (initmax - sub * 0).ToString() + "-" + t5.ToString();
            return _break;
        }
    }
}
