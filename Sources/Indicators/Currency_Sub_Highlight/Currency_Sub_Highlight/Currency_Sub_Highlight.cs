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

            var a_100 = 0;
            var a_150 = 0;
            var a_200 = 0;
            var a_250 = 0;
            var a_300 = 0;
            var b_100 = 0;
            var b_150 = 0;
            var b_200 = 0;
            var b_250 = 0;
            var b_300 = 0;
            for (int i = 0; i < index; i++)
            {
                if (currency_sub.Result[i] > 100)
                    a_100++;
                if (currency_sub.Result[i] > 150)
                    a_150++;
                if (currency_sub.Result[i] > 200)
                    a_200++;
                if (currency_sub.Result[i] > 250)
                    a_250++;
                if (currency_sub.Result[i] > 300)
                    a_300++;
                if (currency_sub.Result[i] < -100)
                    b_100++;
                if (currency_sub.Result[i] < -150)
                    b_150++;
                if (currency_sub.Result[i] < -200)
                    b_200++;
                if (currency_sub.Result[i] < -250)
                    b_250++;
                if (currency_sub.Result[i] < -300)
                    b_300++;
            }
            Mark = mark(index).ToString("yyyy-MM-dd") + "-" + mark(index).ToString("HH");
            if (SIG == null)
                ChartObjects.DrawText("sig", "No-Signal", StaticPosition.TopLeft, NoCorel);
            else
                ChartObjects.DrawText("sig", "Signal-" + SIG, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("barsago", "\nCross-" + BarsAgo_Sub.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("mark", "\n\nMark-" + Mark, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawHorizontalLine("midline", 0, NoCorel);
            ChartObjects.DrawText("above", "\n\n\nT-" + index.ToString() + "_A-" + a_100 + "-" + a_150 + "-" + a_200 + "-" + a_250 + "-" + a_300 + "_B-" + b_100 + "-" + b_150 + "-" + b_200 + "-" + b_250 + "-" + b_300, StaticPosition.TopLeft, NoCorel);
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
    }
}
