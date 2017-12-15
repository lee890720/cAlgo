using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency_Highlight : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.Red, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.Blue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Sub { get; set; }

        private Currency currency;
        private Currency_Sub currency_sub;
        public int BarsAgo;

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Currency>(FirstSymbol, SecondSymbol, Period);
            currency_sub = Indicators.GetIndicator<Currency_Sub>(FirstSymbol, SecondSymbol, Period);
        }

        public override void Calculate(int index)
        {
            Result[index] = currency.Result[index];
            Average[index] = currency.Average[index];
            string sig = signal(index);
            if (sig == "below")
                sig_Result_B[index] = currency.Result[index];
            if (sig == "above")
                sig_Result_A[index] = currency.Result[index];
            BarsAgo = barsago(index);
            ChartObjects.DrawText("barsago", "Cross: " + BarsAgo.ToString(), StaticPosition.TopRight, Colors.Red);
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
