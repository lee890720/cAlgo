using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Bollinger_Deviations_Trend_ATR : Robot
    {
        #region Parameter
        [Parameter("Volume", DefaultValue = 1000, MinValue = 1)]
        public int Volume { get; set; }

        [Parameter("Band Height (pips)", DefaultValue = 10, MinValue = 0)]
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
        AverageTrueRange ATR;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        int Open_Consolidation, Close_Consolidation;
        double atr_in;
        #endregion
        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollingerBands_increase = Indicators.BollingerBands(Source, Periods, Deviations + Deviations_Amendment, MAType);
            bollingerBands_decrease = Indicators.BollingerBands(Source, Periods, Deviations - Deviations_Amendment, MAType);
            ATR = Indicators.AverageTrueRange(Periods / 4, MAType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;
            atr_in = 0.0012;
        }
        protected override void OnBar()
        {
            #region parameter
            var atr = ATR.Result.LastValue;
            var top = bollingerBands.Top.Last(1);
            var bottom = bollingerBands.Bottom.Last(1);
            var top_increase = bollingerBands_increase.Top.Last(1);
            var bottom_increase = bollingerBands_increase.Bottom.Last(1);
            var top_decrease = bollingerBands_decrease.Top.Last(1);
            var bottom_decrease = bollingerBands_decrease.Bottom.Last(1);
            var balancelot = GetBalance_Lots();
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
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
            #region Open Position(b_buylabel and b_selllabel)
            if (Close_Consolidation >= ConsolidationPeriods)
            {
                //Close BuyPositions
                if (Symbol.Bid > top_decrease || atr > atr_in)
                {
                    if (b_buypositions.Length != 0)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                    }
                }
                //Close SellPositions
                if (Symbol.Ask < bottom_decrease || atr > atr_in)
                {
                    if (b_sellpositions.Length != 0)
                    {
                        foreach (var position in b_sellpositions)
                            ClosePosition(position);
                    }
                }
            }
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                //Open b_selllabel
                if (Symbol.Bid > top_decrease && !bollingerBands.Bottom.IsFalling() && atr < atr_in)
                {
                    if (b_sellpositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) >= bottom_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) < bottom_decrease)
                    {
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_sellpositions) * TotalLots(b_sellpositions) - bottom_decrease * TotalLots(b_sellpositions)) / (bottom_decrease - Symbol.Bid)), RoundingMode.ToNearest);
                        if (volume > balancelot - TotalLots(b_sellpositions))
                            volume = (long)(balancelot - TotalLots(b_sellpositions));
                        if (volume > Volume * 50)
                            volume = Volume * 50;
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
                //Open b_buylabel
                if (Symbol.Ask < bottom_decrease && !bollingerBands.Top.IsRising() && atr < atr_in)
                {
                    if (b_buypositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) <= top_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) > top_decrease)
                    {
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_buypositions) * TotalLots(b_buypositions) - top_decrease * TotalLots(b_buypositions)) / (top_decrease - Symbol.Ask)), RoundingMode.ToNearest);
                        if (volume > balancelot - TotalLots(b_buypositions))
                            volume = (long)(balancelot - TotalLots(b_buypositions));
                        if (volume > Volume * 50)
                            volume = Volume * 50;
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
            }
            #endregion
        }
        #region Position[]
        private Position[] GetB_BuyPositions()
        {
            return Positions.FindAll(b_buylabel, Symbol);
        }
        private Position[] GetB_SellPositions()
        {
            return Positions.FindAll(b_selllabel, Symbol);
        }
        #endregion
        #region Lots
        private long TotalLots(Position[] pos)
        {
            long totallots;
            totallots = 0;
            if (pos.Length != 0)
                foreach (var position in pos)
                {
                    totallots += position.Volume;
                }
            return totallots;
        }
        private long GetBalance_Lots()
        {
            var balance = Account.Balance;
            var lot = Symbol.NormalizeVolume(balance / 0.02, RoundingMode.ToNearest);
            return lot;
        }
        #endregion
        #region Price
        private double AveragePrice(Position[] pos)
        {
            double totalVolume, totalProduct, averagePrice;
            totalVolume = 0;
            totalProduct = 0;
            averagePrice = 0;
            if (pos.Length != 0)
            {
                foreach (var position in pos)
                {
                    totalVolume += position.Volume;
                    totalProduct += position.Volume * position.EntryPrice;
                }
                averagePrice = totalProduct / totalVolume;
            }
            return averagePrice;
        }
        #endregion
    }
}
