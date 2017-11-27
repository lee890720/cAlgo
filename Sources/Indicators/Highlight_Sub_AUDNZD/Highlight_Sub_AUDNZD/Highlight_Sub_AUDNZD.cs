using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Highlight_Sub_AUDNZD : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("sig_Result_A", Color = Colors.Red, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries sig_Result_A { get; set; }

        [Output("sig_Result_B", Color = Colors.Blue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries sig_Result_B { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Sub { get; set; }

        public string SIG;

        private USD_AUDNZD usd_audnzd;
        private Sub_AUDNZD sub_audnzd;

        protected override void Initialize()
        {
            usd_audnzd = Indicators.GetIndicator<USD_AUDNZD>(Period);
            sub_audnzd = Indicators.GetIndicator<Sub_AUDNZD>(Period);
            SIG = null;
        }

        public override void Calculate(int index)
        {
            Result[index] = sub_audnzd.Result[index];
            Average[index] = sub_audnzd.Average[index];
            string sig = signal(index);
            SIG = sig;
            if (sig == "below")
                sig_Result_B[index] = sub_audnzd.Result[index];
            else
                sig_Result_B[index] = 0;
            if (sig == "above")
                sig_Result_A[index] = sub_audnzd.Result[index];
            else
                sig_Result_A[index] = 0;
        }

        private string signal(int index)
        {
            double u_result = usd_audnzd.Result[index];
            double u_average = usd_audnzd.Average[index];
            double s_result = sub_audnzd.Result[index];
            double s_average = sub_audnzd.Average[index];
            if (-Sub > s_result && s_result > s_average && u_result < u_average)
                return "below";
            if (Sub < s_result && s_result < s_average && u_result > u_average)
                return "above";
            return null;
        }
    }
}
