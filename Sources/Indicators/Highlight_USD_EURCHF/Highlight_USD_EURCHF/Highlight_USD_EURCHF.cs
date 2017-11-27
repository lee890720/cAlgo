using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Highlight_USD_EURCHF : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.Red, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.Blue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Sub { get; set; }

        private USD_EURCHF usd_eurchf;
        private Sub_EURCHF sub_eurchf;

        protected override void Initialize()
        {
            usd_eurchf = Indicators.GetIndicator<USD_EURCHF>(Period);
            sub_eurchf = Indicators.GetIndicator<Sub_EURCHF>(Period);
        }

        public override void Calculate(int index)
        {
            Result[index] = usd_eurchf.Result[index];
            Average[index] = usd_eurchf.Average[index];
            string sig = signal(index);
            if (sig == "below")
                sig_Result_B[index] = usd_eurchf.Result[index];
            if (sig == "above")
                sig_Result_A[index] = usd_eurchf.Result[index];
        }

        private string signal(int index)
        {
            double u_result = usd_eurchf.Result[index];
            double u_average = usd_eurchf.Average[index];
            double s_result = sub_eurchf.Result[index];
            double s_average = sub_eurchf.Average[index];
            if (-Sub > s_result && s_result > s_average && u_result < u_average)
                return "below";
            if (Sub < s_result && s_result < s_average && u_result > u_average)
                return "above";
            return null;
        }
    }
}
