using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class USD_EURCHF_L : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("ResultAbove", Color = Colors.Red, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultAbove { get; set; }

        [Output("ResultBelow", Color = Colors.Blue, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultBelow { get; set; }

        private USD_EURCHF usd_eurchf;
        private USD_GBPCHF usd_gbpchf;
        private USD_GBPEUR usd_gbpeur;
        private int Period = 120;
        private string BigSymbol = "EURUSD";
        private string SmallSymbol = "USDCHF";
        private MarketSeries _symbolbigSeries, _symbolsmallSeries;
        protected override void Initialize()
        {
            _symbolbigSeries = MarketData.GetSeries(BigSymbol, TimeFrame);
            _symbolsmallSeries = MarketData.GetSeries(SmallSymbol, TimeFrame);
            usd_eurchf = Indicators.GetIndicator<USD_EURCHF>();
            usd_gbpchf = Indicators.GetIndicator<USD_GBPCHF>();
            usd_gbpeur = Indicators.GetIndicator<USD_GBPEUR>();
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int BigIndex = _symbolbigSeries.GetIndexByDate(SymbolTime);
            int SmallIndex = _symbolsmallSeries.GetIndexByDate(SymbolTime);
            Result[index] = (_symbolbigSeries.Close[BigIndex] - 1 / _symbolsmallSeries.Close[SmallIndex]) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
            if (opensignal() == "Above_EURCHF")
                ResultAbove[index] = Result[index];
            if (opensignal() == "Below_EURCHF")
                ResultBelow[index] = Result[index];
        }
        private string opensignal()
        {
            #region Parameter
            string signal = null;
            string eurchfsignal = null;
            string gbpchfsignal = null;
            string gbpeursignal = null;
            int eurusd = 0;
            int gbpusd = 0;
            int usdchf = 0;
            double eurchf = 0;
            double gbpchf = 0;
            double gbpeur = 0;
            var RS_eurchf = usd_eurchf.Result.LastValue;
            var AV_eurchf = usd_eurchf.Average.LastValue;
            var RS_gbpchf = usd_gbpchf.Result.LastValue;
            var AV_gbpchf = usd_gbpchf.Average.LastValue;
            var RS_gbpeur = usd_gbpeur.Result.LastValue;
            var AV_gbpeur = usd_gbpeur.Average.LastValue;
            var distance = 30;
            double AV_above = distance;
            double AV_below = -distance;
            #endregion
            int Open_A = 0;
            int Open_B = 0;
            if (RS_eurchf >= AV_eurchf)
                Open_A++;
            if (RS_eurchf < AV_eurchf)
                Open_B++;
            if (RS_gbpchf >= AV_gbpchf)
                Open_A++;
            if (RS_gbpchf < AV_gbpchf)
                Open_B++;
            if (RS_gbpeur >= AV_gbpeur)
                Open_A++;
            if (RS_gbpeur < AV_gbpeur)
                Open_B++;
            if (Open_A == 3)
                AV_below = -10000;
            if (Open_B == 3)
                AV_above = 10000;
            if (Open_A == 2 && Open_B == 1)
                AV_above = 10000;
            if (Open_A == 1 && Open_B == 2)
                AV_below = -10000;
            #region Signal
            if (RS_eurchf > AV_eurchf + AV_above)
                eurchfsignal = "Above_EURCHF";
            if (RS_eurchf < AV_eurchf + AV_below)
                eurchfsignal = "Below_EURCHF";
            if (RS_gbpchf > AV_gbpchf + AV_above)
                gbpchfsignal = "Above_GBPCHF";
            if (RS_gbpchf < AV_gbpchf + AV_below)
                gbpchfsignal = "Below_GBPCHF";
            if (RS_gbpeur > AV_gbpeur + AV_above)
                gbpeursignal = "Above_GBPEUR";
            if (RS_gbpeur < AV_gbpeur + AV_below)
                gbpeursignal = "Below_GBPEUR";
            if (eurchfsignal == "Above_EURCHF")
            {
                eurusd--;
                usdchf--;
            }
            if (eurchfsignal == "Below_EURCHF")
            {
                eurusd++;
                usdchf++;
            }
            if (gbpchfsignal == "Above_GBPCHF")
            {
                gbpusd--;
                usdchf--;
            }
            if (gbpchfsignal == "Below_GBPCHF")
            {
                gbpusd++;
                usdchf++;
            }
            if (gbpeursignal == "Above_GBPEUR")
            {
                gbpusd--;
                eurusd++;
            }
            if (gbpeursignal == "Below_GBPEUR")
            {
                gbpusd++;
                eurusd--;
            }
            if (eurusd == 0)
            {
                eurchfsignal = null;
                gbpeursignal = null;
            }
            if (gbpusd == 0)
            {
                gbpchfsignal = null;
                gbpeursignal = null;
            }
            if (usdchf == 0)
            {
                eurchfsignal = null;
                gbpchfsignal = null;
            }
            if (eurchfsignal != null)
            {
                eurchf = Math.Abs(RS_eurchf - AV_eurchf);
            }
            if (gbpchfsignal != null)
            {
                gbpchf = Math.Abs(RS_gbpchf - AV_gbpchf);
            }
            if (gbpeursignal != null)
            {
                gbpeur = Math.Abs(RS_gbpeur - AV_gbpeur);
            }
            List<double> abc = new List<double> 
            {
                eurchf,
                gbpchf,
                gbpeur
            };
            var abcmax = abc.Max();
            if (abcmax == eurchf)
                signal = eurchfsignal;
            if (abcmax == gbpchf)
                signal = gbpchfsignal;
            if (abcmax == gbpeur)
                signal = gbpeursignal;
            return signal;
            #endregion
        }
    }
}
