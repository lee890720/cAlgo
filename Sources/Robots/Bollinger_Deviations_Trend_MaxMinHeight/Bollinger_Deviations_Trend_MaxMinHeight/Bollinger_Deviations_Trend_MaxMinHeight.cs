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
    public class Bollinger_Deviations_Trend_MaxMinHeight : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

        [Parameter("Min Band Height (pips)", DefaultValue = 10, MinValue = 0)]
        public double MinBandHeightPips { get; set; }

        [Parameter("Max Band Height (pips)", DefaultValue = 100, MinValue = 0)]
        public double MaxBandHeightPips { get; set; }

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
            var balancelot = GetBalance_Lots();
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
            #endregion
            #region define b_consolidation and c_consolidation
            if (top - bottom >= MinBandHeightPips * Symbol.PipSize && top - bottom < MaxBandHeightPips * Symbol.PipSize)
            {
                Open_Consolidation = Open_Consolidation + 1;
            }
            else
            {
                Open_Consolidation = 0;
            }
            if (top - bottom >= MinBandHeightPips * Symbol.PipSize)
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
                if (Symbol.Bid > top_decrease || top - bottom > MaxBandHeightPips * Symbol.PipSize)
                {
                    if (b_buypositions.Length != 0)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                    }
                }
                //Close SellPositions
                if (Symbol.Ask < bottom_decrease || top - bottom > MaxBandHeightPips * Symbol.PipSize)
                {
                    if (b_sellpositions.Length != 0)
                    {
                        foreach (var position in b_sellpositions)
                            ClosePosition(position);
                    }
                }
            }
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                //Open b_selllabel
                if (Symbol.Bid > top_decrease && !bollingerBands.Bottom.IsFalling())
                {
                    if (b_sellpositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) >= bottom_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) < bottom_decrease)
                    {
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_sellpositions) * TotalLots(b_sellpositions) - bottom_decrease * TotalLots(b_sellpositions)) / (bottom_decrease - Symbol.Bid)), RoundingMode.ToNearest);
                        if (volume > balancelot - TotalLots(b_sellpositions))
                            volume = (long)(balancelot - TotalLots(b_sellpositions));
                        if (volume > Init_Volume * 50)
                            volume = Init_Volume * 50;
                        //if (volume > MaxLot(b_sellpositions) * 2)
                        //    volume = MaxLot(b_sellpositions) * 2;
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
                //Open b_buylabel
                if (Symbol.Ask < bottom_decrease && !bollingerBands.Top.IsRising())
                {
                    if (b_buypositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) <= top_decrease)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) > top_decrease)
                    {
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_buypositions) * TotalLots(b_buypositions) - top_decrease * TotalLots(b_buypositions)) / (top_decrease - Symbol.Ask)), RoundingMode.ToNearest);
                        if (volume > balancelot - TotalLots(b_buypositions))
                            volume = (long)(balancelot - TotalLots(b_buypositions));
                        if (volume > Init_Volume * 50)
                            volume = Init_Volume * 50;
                        //if (volume > MaxLot(b_buypositions) * 2)
                        //    volume = MaxLot(b_buypositions) * 2;
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
        private long MaxLot(Position[] pos)
        {
            long maxlot;
            maxlot = 0;
            if (pos.Length != 0)
                foreach (var position in pos)
                {
                    if (maxlot == 0)
                        maxlot = position.Volume;
                    if (maxlot < position.Volume)
                        maxlot = position.Volume;
                }
            return maxlot;
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
