using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_Currency : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = "XBRUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "XTIUSD")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Sub { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 2)]
        public double Magnify { get; set; }

        public int BarsAgo;
        public string _ratio;
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
            Result[index] = currency.Result[index];
            Average[index] = currency.Average[index];
            string sig = signal(index);
            if (sig == "above")
                sig_Result_A[index] = currency.Result[index];
            if (sig == "below")
                sig_Result_B[index] = currency.Result[index];
            #region Chart
            BarsAgo = barsago(index);
            _ratio = currency._ratio;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Average[i];
            }
            var midaverage = sum / Period;
            ChartObjects.DrawText("barsago", "Cross-" + BarsAgo.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio", "\nratio-" + _ratio, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio2", "\n\nRatio-" + Ratio.ToString() + "_Magnify-" + Magnify.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawHorizontalLine("midline", midaverage, NoCorel);
            #endregion
        }

        private string signal(int index)
        {
            //double u_result = currency.Result[index];
            //double u_average = currency.Average[index];
            double s_result = currency_sub.Result[index];
            //double s_average = currency_sub.Average[index];
            if (s_result < -50 * Magnify)
                return "below";
            if (s_result > 50 * Magnify)
                return "above";
            return null;
        }

        private int barsago(int index)
        {
            double u_result = currency.Result[index];
            double u_average = currency.Average[index];
            if (u_result > u_average)
                for (int i = index - 1; i > 0; i--)
                {
                    if (currency.Result[i] <= currency.Average[i])
                        return index - i;
                }
            if (u_result < u_average)
                for (int i = index - 1; i > 0; i--)
                {
                    if (currency.Result[i] >= currency.Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
