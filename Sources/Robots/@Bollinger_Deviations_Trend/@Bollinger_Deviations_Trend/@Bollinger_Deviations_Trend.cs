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
    public class Bollinger_Deviations_Trend : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

        [Parameter("Band Height (pips)", DefaultValue = 1, MinValue = 0)]
        public double BandHeightPips { get; set; }

        [Parameter("Consolidation Periods", DefaultValue = 5)]
        public int ConsolidationPeriods { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Bollinger Bands Periods", DefaultValue = 40)]
        public int Periods { get; set; }

        [Parameter("Bollinger Bands Deviations", DefaultValue = 2)]
        public double Deviations { get; set; }

        [Parameter("Bands Deviations Amendment", DefaultValue = 0.5)]
        public double Deviations_Amendment { get; set; }

        [Parameter("Bollinger Bands MA Type")]
        public MovingAverageType MAType { get; set; }

        #endregion
        #region
        BollingerBands bollingerBands;
        BollingerBands bollingerBands_increase;
        BollingerBands bollingerBands_decrease;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        int Open_Consolidation, Close_Consolidation;
        #endregion
        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollingerBands_increase = Indicators.BollingerBands(Source, Periods, Deviations + Deviations_Amendment, MAType);
            bollingerBands_decrease = Indicators.BollingerBands(Source, Periods, Deviations - Deviations_Amendment, MAType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;

        }
        protected override void OnBar()
        {
            #region parameter
            var top = bollingerBands.Top.Last(1);
            var bottom = bollingerBands.Bottom.Last(1);
            var top_increase = bollingerBands_increase.Top.Last(1);
            var bottom_increase = bollingerBands_increase.Bottom.Last(1);
            var top_decrease = bollingerBands_decrease.Top.Last(1);
            var bottom_decrease = bollingerBands_decrease.Bottom.Last(1);
            var balancelot = this.BalanceLots(5);
            var b_buypositions = this.GetPositions(b_buylabel);
            var b_sellpositions = this.GetPositions(b_selllabel);
            #endregion
            #region define b_consolidation and c_consolidation
            if (top - bottom >= BandHeightPips * Symbol.PipSize)
            {
                Open_Consolidation = Open_Consolidation + 1;
            }
            else
            {
                Open_Consolidation = 0;
            }
            if (top - bottom >= BandHeightPips * Symbol.PipSize)
            {
                Close_Consolidation = Close_Consolidation + 1;
            }
            else
            {
                Close_Consolidation = 0;
            }
            #endregion
            #region Close Position(b_buylabel and b_selllabel)
            if (Close_Consolidation >= ConsolidationPeriods)
            {
                //Close BuyPositions
                if (Symbol.Mid() > top_decrease)
                {
                    if (b_buypositions.Length != 0)
                    {
                        this.closeAllBuyPositions(b_buylabel);
                    }
                }
                //Close SellPositions
                if (Symbol.Mid() < bottom_decrease)
                {
                    if (b_sellpositions.Length != 0)
                    {
                        this.closeAllSellPositions(b_selllabel);
                    }
                }
            }
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                //Open b_selllabel
                if (Symbol.Mid() > top_decrease && !bollingerBands.Bottom.IsFalling())
                {
                    if (b_sellpositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && this.AveragePrice(b_selllabel) >= bottom_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && this.AveragePrice(b_selllabel) < bottom_decrease)
                    {
                        long volume = this.MartingaleLot(b_selllabel, bottom_decrease);
                        if (volume > balancelot - this.TotalLots(b_selllabel))
                            volume = (long)(balancelot - this.TotalLots(b_selllabel));
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
                //Open b_buylabel
                if (Symbol.Mid() < bottom_decrease && !bollingerBands.Top.IsRising())
                {
                    if (b_buypositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && this.AveragePrice(b_buylabel) <= top_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && this.AveragePrice(b_buylabel) > top_decrease)
                    {
                        long volume = this.MartingaleLot(b_buylabel, top_decrease);
                        if (volume > balancelot - this.TotalLots(b_buylabel))
                            volume = (long)(balancelot - this.TotalLots(b_buylabel));
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
            }
            #endregion
        }
    }
}
