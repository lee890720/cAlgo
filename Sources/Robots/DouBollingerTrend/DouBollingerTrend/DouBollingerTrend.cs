using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Bollinger : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

        [Parameter("Lot(s)/10000USD", DefaultValue = 5, MinValue = 0.01)]
        public double per { get; set; }

        [Parameter("Mid Difference", DefaultValue = 4)]
        public int Mid_Dif { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Bollinger Periods", DefaultValue = 48)]
        public int Periods { get; set; }

        [Parameter("Bollinger Deviations", DefaultValue = 2)]
        public double Deviations { get; set; }

        [Parameter("Bollinger MA Type")]
        public MovingAverageType MAType { get; set; }

        #endregion
        #region
        BollingerBands boll;
        BollingerBands bollhalf;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        #endregion
        protected override void OnStart()
        {
            boll = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollhalf = Indicators.BollingerBands(Source, Periods / 2, Deviations, MAType);
        }
        protected override void OnBar()
        {
            #region parameter
            var midmin = boll.Main.Last(1) < bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var midmax = boll.Main.Last(1) > bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var uppermin = boll.Top.Last(1) < bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var uppermax = boll.Top.Last(1) > bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var lowermin = boll.Bottom.Last(1) < bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var lowermax = boll.Bottom.Last(1) > bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var balancelot = this.BalanceLots(per);
            var b_buyposs = this.GetPositions(b_buylabel);
            var b_sellposs = this.GetPositions(b_selllabel);
            var bars = MarketSeries.Bars();
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if ((midmax - midmin) / Symbol.PipSize > Mid_Dif)
            {
                //Open b_buylabel
                if (MarketSeries.Open[bars - 2] < midmax && MarketSeries.Open[bars - 1] >= midmax)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                }
                //Open b_selllabel
                if (MarketSeries.Open[bars - 2] > midmin && MarketSeries.Open[bars - 1] <= midmin)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                }
                #endregion
                #region Close Position(b_buylabel and selllabel)
                //Close b_buylabel
                if ((MarketSeries.Open[bars - 2] > midmin && MarketSeries.Open[bars - 1] <= midmin) || (MarketSeries.Open[bars - 1] > uppermax))
                    this.closeAllBuyPositions(b_buylabel);
                //Close b_selllabel
                if ((MarketSeries.Open[bars - 2] < midmax && MarketSeries.Open[bars - 1] >= midmax) || (MarketSeries.Open[bars - 1] < lowermin))
                    this.closeAllSellPositions(b_selllabel);
            }
            #endregion
        }
    }
}
