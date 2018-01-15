using cAlgo.API;
using cAlgo.API.Internals;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Wave_Currency_Sub_Highlight : Indicator
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

        private Wave_Currency currency;
        private Wave_Currency_Sub currency_sub;
        public string SIG;
        public int BarsAgo_Sub;
        public string Mark;
        private Colors PCorel, NCorel, NoCorel;

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Wave_Currency>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
            currency_sub = Indicators.GetIndicator<Wave_Currency_Sub>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
            SIG = null;
            PCorel = Colors.Lime;
            NCorel = Colors.OrangeRed;
            NoCorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            BarsAgo_Sub = barsago(index);
            Mark = string.Format("{0:d}", mark(index)) + "-" + string.Format("{0:00}", mark(index).Hour);
            Mark = mark(index).ToString("yyyy-MM-dd") + "-" + mark(index).ToString("HH");
            Result[index] = currency_sub.Result[index];
            Average[index] = currency_sub.Average[index];
            string sig = signal(index);
            SIG = sig;
            if (sig == "below")
                sig_Result_B[index] = currency_sub.Result[index];
            else
                sig_Result_B[index] = 0;
            if (sig == "above")
                sig_Result_A[index] = currency_sub.Result[index];
            else
                sig_Result_A[index] = 0;
            if (SIG == null)
                ChartObjects.DrawText("sig", "No-Signal", StaticPosition.TopLeft, NoCorel);
            else
                ChartObjects.DrawText("sig", "Signal-" + SIG, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("barsago", "\nCross-" + BarsAgo_Sub.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("mark", "\n\nMark-" + Mark, StaticPosition.TopLeft, NoCorel);
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

        private DateTime mark(int index)
        {
            int idx = index - BarsAgo_Sub;
            DateTime dt = MarketSeries.OpenTime[idx];
            return dt;
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
    }
}
