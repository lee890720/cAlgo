using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _USD_CADAUD : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("ResultAbove", Color = Colors.Red, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultAbove { get; set; }

        [Output("ResultBelow", Color = Colors.Blue, PlotType = PlotType.DiscontinuousLine, Thickness = 2)]
        public IndicatorDataSeries ResultBelow { get; set; }

        private USD_CADAUD usd_cadaud;
        private USD_JPYAUD usd_jpyaud;
        private USD_JPYCAD usd_jpycad;
        private int Period = 120;
        private string BigSymbol = "USDCAD";
        private string SmallSymbol = "AUDUSD";
        private MarketSeries _symbolbigSeries, _symbolsmallSeries;
        protected override void Initialize()
        {
            _symbolbigSeries = MarketData.GetSeries(BigSymbol, TimeFrame);
            _symbolsmallSeries = MarketData.GetSeries(SmallSymbol, TimeFrame);
            usd_cadaud = Indicators.GetIndicator<USD_CADAUD>();
            usd_jpyaud = Indicators.GetIndicator<USD_JPYAUD>();
            usd_jpycad = Indicators.GetIndicator<USD_JPYCAD>();
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int BigIndex = _symbolbigSeries.GetIndexByDate(SymbolTime);
            int SmallIndex = _symbolsmallSeries.GetIndexByDate(SymbolTime);
            Result[index] = (1 / _symbolbigSeries.Close[BigIndex] - _symbolsmallSeries.Close[SmallIndex]) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
            if (opensignal() == "Above_CADAUD")
                ResultAbove[index] = Result[index];
            if (opensignal() == "Below_CADAUD")
                ResultBelow[index] = Result[index];
        }
        private string opensignal()
        {
            #region Parameter
            string signal = null;
            string cadaudsignal = null;
            string jpyaudsignal = null;
            string jpycadsignal = null;
            int usdcad = 0;
            int usdjpy = 0;
            int audusd = 0;
            double cadaud = 0;
            double jpyaud = 0;
            double jpycad = 0;
            var RS_cadaud = usd_cadaud.Result.LastValue;
            var AV_cadaud = usd_cadaud.Average.LastValue;
            var RS_jpyaud = usd_jpyaud.Result.LastValue;
            var AV_jpyaud = usd_jpyaud.Average.LastValue;
            var RS_jpycad = usd_jpycad.Result.LastValue;
            var AV_jpycad = usd_jpycad.Average.LastValue;
            var distance = 30;
            double AV_above = distance;
            double AV_below = -distance;
            #endregion
            int Open_A = 0;
            int Open_B = 0;
            if (RS_cadaud >= AV_cadaud)
                Open_A++;
            if (RS_cadaud < AV_cadaud)
                Open_B++;
            if (RS_jpyaud >= AV_jpyaud)
                Open_A++;
            if (RS_jpyaud < AV_jpyaud)
                Open_B++;
            if (RS_jpycad >= AV_jpycad)
                Open_A++;
            if (RS_jpycad < AV_jpycad)
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
            if (RS_cadaud > AV_cadaud + AV_above)
                cadaudsignal = "Above_CADAUD";
            if (RS_cadaud < AV_cadaud + AV_below)
                cadaudsignal = "Below_CADAUD";
            if (RS_jpyaud > AV_jpyaud + AV_above)
                jpyaudsignal = "Above_JPYAUD";
            if (RS_jpyaud < AV_jpyaud + AV_below)
                jpyaudsignal = "Below_JPYAUD";
            if (RS_jpycad > AV_jpycad + AV_above)
                jpycadsignal = "Above_JPYCAD";
            if (RS_jpycad < AV_jpycad + AV_below)
                jpycadsignal = "Below_JPYCAD";
            if (cadaudsignal == "Above_CADAUD")
            {
                usdcad++;
                audusd++;
            }
            if (cadaudsignal == "Below_CADAUD")
            {
                usdcad--;
                audusd--;
            }
            if (jpyaudsignal == "Above_JPYAUD")
            {
                usdjpy++;
                audusd++;
            }
            if (jpyaudsignal == "Below_JPYAUD")
            {
                usdjpy--;
                audusd--;
            }
            if (jpycadsignal == "Above_JPYCAD")
            {
                usdjpy++;
                usdcad--;
            }
            if (jpycadsignal == "Below_JPYCAD")
            {
                usdjpy--;
                usdcad++;
            }
            if (usdcad == 0)
            {
                cadaudsignal = null;
                jpycadsignal = null;
            }
            if (usdjpy == 0)
            {
                jpyaudsignal = null;
                jpycadsignal = null;
            }
            if (audusd == 0)
            {
                cadaudsignal = null;
                jpyaudsignal = null;
            }
            if (cadaudsignal != null)
            {
                cadaud = Math.Abs(RS_cadaud - AV_cadaud);
            }
            if (jpyaudsignal != null)
            {
                jpyaud = Math.Abs(RS_jpyaud - AV_jpyaud);
            }
            if (jpycadsignal != null)
            {
                jpycad = Math.Abs(RS_jpycad - AV_jpycad);
            }
            List<double> abc = new List<double> 
            {
                cadaud,
                jpyaud,
                jpycad
            };
            var abcmax = abc.Max();
            if (abcmax == cadaud)
                signal = cadaudsignal;
            if (abcmax == jpyaud)
                signal = jpyaudsignal;
            if (abcmax == jpycad)
                signal = jpycadsignal;
            return signal;
            #endregion
        }
    }
}
