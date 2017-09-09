using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _nUSD_EURXAG : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("ResultAbove", Color = Colors.Red, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultAbove { get; set; }

        [Output("ResultBelow", Color = Colors.Blue, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultBelow { get; set; }

        private nUSD_EURXAG usd_eurxag;
        private nUSD_XAUXAG usd_xauxag;
        private nUSD_XAUEUR usd_xaueur;
        private int Period = 120;
        private string BigSymbol = "EURUSD";
        private string SmallSymbol = "XAGUSD";
        private MarketSeries _symbolbigSeries, _symbolsmallSeries;
        protected override void Initialize()
        {
            _symbolbigSeries = MarketData.GetSeries(BigSymbol, TimeFrame);
            _symbolsmallSeries = MarketData.GetSeries(SmallSymbol, TimeFrame);
            usd_eurxag = Indicators.GetIndicator<nUSD_EURXAG>();
            usd_xauxag = Indicators.GetIndicator<nUSD_XAUXAG>();
            usd_xaueur = Indicators.GetIndicator<nUSD_XAUEUR>();
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int BigIndex = _symbolbigSeries.GetIndexByDate(SymbolTime);
            int SmallIndex = _symbolsmallSeries.GetIndexByDate(SymbolTime);
            Result[index] = (_symbolbigSeries.Close[BigIndex] - _symbolsmallSeries.Close[SmallIndex] / 100 * 2) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
            Average[index] = sum / Period;
            if (opensignal() == "Above_EURXAG")
                ResultAbove[index] = Result[index];
            if (opensignal() == "Below_EURXAG")
                ResultBelow[index] = Result[index];
        }
        private string opensignal()
        {
            #region Parameter
            string signal = null;
            string eurxagsignal = null;
            string xauxagsignal = null;
            string xaueursignal = null;
            int eurusd = 0;
            int xauusd = 0;
            int xagusd = 0;
            double eurxag = 0;
            double xauxag = 0;
            double xaueur = 0;
            var RS_eurxag = usd_eurxag.Result.LastValue;
            var AV_eurxag = usd_eurxag.Average.LastValue;
            var RS_xauxag = usd_xauxag.Result.LastValue;
            var AV_xauxag = usd_xauxag.Average.LastValue;
            var RS_xaueur = usd_xaueur.Result.LastValue;
            var AV_xaueur = usd_xaueur.Average.LastValue;
            var distance = 100;
            double AV_above = distance;
            double AV_below = -distance;
            #endregion
            int Open_A = 0;
            int Open_B = 0;
            if (RS_eurxag >= AV_eurxag)
                Open_A++;
            if (RS_eurxag < AV_eurxag)
                Open_B++;
            if (RS_xauxag >= AV_xauxag)
                Open_A++;
            if (RS_xauxag < AV_xauxag)
                Open_B++;
            if (RS_xaueur >= AV_xaueur)
                Open_A++;
            if (RS_xaueur < AV_xaueur)
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
            if (RS_eurxag > AV_eurxag + AV_above)
                eurxagsignal = "Above_EURXAG";
            if (RS_eurxag < AV_eurxag + AV_below)
                eurxagsignal = "Below_EURXAG";
            if (RS_xauxag > AV_xauxag + AV_above)
                xauxagsignal = "Above_XAUXAG";
            if (RS_xauxag < AV_xauxag + AV_below)
                xauxagsignal = "Below_XAUXAG";
            if (RS_xaueur > AV_xaueur + AV_above)
                xaueursignal = "Above_XAUEUR";
            if (RS_xaueur < AV_xaueur + AV_below)
                xaueursignal = "Below_XAUEUR";
            if (eurxagsignal == "Above_EURXAG")
            {
                eurusd--;
                xagusd++;
            }
            if (eurxagsignal == "Below_EURXAG")
            {
                eurusd++;
                xagusd--;
            }
            if (xauxagsignal == "Above_XAUXAG")
            {
                xauusd--;
                xagusd++;
            }
            if (xauxagsignal == "Below_XAUXAG")
            {
                xauusd++;
                xagusd--;
            }
            if (xaueursignal == "Above_XAUEUR")
            {
                xauusd--;
                eurusd++;
            }
            if (xaueursignal == "Below_XAUEUR")
            {
                xauusd++;
                eurusd--;
            }
            if (eurusd == 0)
            {
                eurxagsignal = null;
                xaueursignal = null;
            }
            if (xauusd == 0)
            {
                xauxagsignal = null;
                xaueursignal = null;
            }
            if (xagusd == 0)
            {
                eurxagsignal = null;
                xauxagsignal = null;
            }
            if (eurxagsignal != null)
            {
                eurxag = Math.Abs(RS_eurxag - AV_eurxag);
            }
            if (xauxagsignal != null)
            {
                xauxag = Math.Abs(RS_xauxag - AV_xauxag);
            }
            if (xaueursignal != null)
            {
                xaueur = Math.Abs(RS_xaueur - AV_xaueur);
            }
            List<double> abc = new List<double> 
            {
                eurxag,
                xauxag,
                xaueur
            };
            var abcmax = abc.Max();
            if (abcmax == eurxag)
                signal = eurxagsignal;
            if (abcmax == xauxag)
                signal = xauxagsignal;
            if (abcmax == xaueur)
                signal = xaueursignal;
            return signal;
            #endregion
        }
    }
}
