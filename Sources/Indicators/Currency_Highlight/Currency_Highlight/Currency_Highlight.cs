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
        public int Sub { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
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
            var _currentDaySeries = MarketData.GetSeries(Symbol, TimeFrame.Daily);
            var _currentHourSeries = MarketData.GetSeries(Symbol, TimeFrame.Hour);
            double t_day = 0;
            double t_hour = 0;
            double a_day = 0;
            double a_hour = 0;
            for (int i = index - Period; i < index; i++)
            {
                var td = Math.Abs(_currentDaySeries.High[i] - _currentDaySeries.Low[i]);
                var th = Math.Abs(_currentHourSeries.High[i] - _currentHourSeries.Low[i]);
                t_day += (double.IsNaN(td) ? 0 : td) / Symbol.PipSize;
                t_hour += (double.IsNaN(th) ? 0 : th) / Symbol.PipSize;
            }
            a_day = Math.Round(t_day / Period);
            a_hour = Math.Round(t_hour / Period);
            var e_currentDaySeries = MarketData.GetSeries("EURCHF", TimeFrame.Daily);
            var e_currentHourSeries = MarketData.GetSeries("EURCHF", TimeFrame.Hour);
            double et_day = 0;
            double et_hour = 0;
            double ea_day = 0;
            double ea_hour = 0;
            Symbol esymbol = MarketData.GetSymbol("EURCHF");
            for (int i = index - Period; i < index; i++)
            {
                var etd = Math.Abs(e_currentDaySeries.High[i] - e_currentDaySeries.Low[i]);
                var eth = Math.Abs(e_currentHourSeries.High[i] - e_currentHourSeries.Low[i]);
                et_day += (double.IsNaN(etd) ? 0 : etd) / esymbol.PipSize;
                et_hour += (double.IsNaN(eth) ? 0 : eth) / esymbol.PipSize;
            }
            ea_day = Math.Round(et_day / Period);
            ea_hour = Math.Round(et_hour / Period);
            var _magnify = Math.Round(Ratio * Ratio / (a_hour / ea_hour), 2);
            ChartObjects.DrawText("barsago", "Cross-" + BarsAgo.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio", "\nratio-" + _ratio + "_manify-" + _magnify.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("Ratio2", "\n\nRatio-" + Ratio.ToString() + "_Magnify-" + Magnify.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("symbol", "\n\n\n" + Symbol.Code + "_D-" + a_day.ToString() + "_H-" + a_hour.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("esymbol", "\n\n\n\n" + esymbol.Code + "_D-" + ea_day.ToString() + "_H-" + ea_hour.ToString(), StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawHorizontalLine("midline", midaverage, NoCorel);
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
