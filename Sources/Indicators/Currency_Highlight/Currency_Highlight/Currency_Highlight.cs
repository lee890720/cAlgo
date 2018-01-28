using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency_Highlight : Indicator
    {
        #region Parameter
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public double Sub { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        public int BarsAgo;
        public string _ratio;
        private Currency currency;
        private Currency_Sub currency_sub;
        private Colors NoCorel;
        #endregion

        protected override void Initialize()
        {
            currency = Indicators.GetIndicator<Currency>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
            currency_sub = Indicators.GetIndicator<Currency_Sub>(FirstSymbol, SecondSymbol, Period, Ratio, Magnify);
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
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio", "\nratio_" + _ratio, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Magnify", "\n\nmagnify_" + currency._magnify, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Param_R_M", "\n\n\nRatio_(" + Ratio.ToString() + ")" + "_Magnify_(" + Magnify.ToString() + ")", StaticPosition.TopLeft, NoCorel);
            //ChartObjects.DrawHorizontalLine("midline", midaverage, NoCorel);
            #endregion
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
