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
    public class Bollinger_Deviations_Range : Robot
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
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        int Open_Consolidation, Close_Consolidation;
        bool b_increse, b_main, b_decrease, b_open;
        bool s_increse, s_main, s_decrease, s_open;
        #endregion
        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollingerBands_increase = Indicators.BollingerBands(Source, Periods, Deviations + Deviations_Amendment, MAType);
            bollingerBands_decrease = Indicators.BollingerBands(Source, Periods, Deviations - Deviations_Amendment, MAType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;
            b_increse = false;
            b_main = false;
            b_decrease = false;
            b_open = false;
            s_increse = false;
            s_main = false;
            s_decrease = false;
            s_open = false;
        }
        protected override void OnBar()
        {
            #region parameter
            var top = bollingerBands.Top.Last(1);
            var bottom = bollingerBands.Bottom.Last(1);
            var main = bollingerBands.Main.Last(1);
            var top_increase = bollingerBands_increase.Top.Last(1);
            var bottom_increase = bollingerBands_increase.Bottom.Last(1);
            var top_decrease = bollingerBands_decrease.Top.Last(1);
            var bottom_decrease = bollingerBands_decrease.Bottom.Last(1);
            var balancelot = GetBalance_Lots();
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
            #endregion
            #region Open Position Signal
            if (Symbol.Ask < bottom_increase)
                b_increse = true;
            if (Symbol.Ask < bottom && b_increse)
                b_main = true;
            if (Symbol.Ask < bottom_decrease && b_main)
                b_decrease = true;
            if (Symbol.Ask > bottom_decrease && b_decrease)
            {
                b_open = true;
                b_increse = false;
                b_main = false;
                b_decrease = false;
            }
            if (Symbol.Bid > top_increase)
                s_increse = true;
            if (Symbol.Bid > top && s_increse)
                s_main = true;
            if (Symbol.Bid > top_decrease && s_main)
                s_decrease = true;
            if (Symbol.Bid < top_decrease && s_decrease)
            {
                s_open = true;
                s_increse = false;
                s_main = false;
                s_decrease = false;
            }
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
                if (Symbol.Bid > top_decrease)
                {
                    if (b_buypositions.Length != 0 && b_sellpositions.Length == 0 && TotalProfit(b_buypositions) > 0)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                    }
                }
                //Close SellPositions
                if (Symbol.Ask < bottom_decrease)
                {
                    if (b_sellpositions.Length != 0 && b_buypositions.Length == 0 && TotalProfit(b_sellpositions) > 0)
                    {
                        foreach (var position in b_sellpositions)
                            ClosePosition(position);
                    }
                }
                if (b_buypositions.Length != 0 && b_sellpositions.Length != 0 && TotalProfit(b_buypositions) + TotalProfit(b_sellpositions) > 0)
                {
                    foreach (var position in b_buypositions)
                        ClosePosition(position);
                    foreach (var position in b_sellpositions)
                        ClosePosition(position);
                }
            }
            //Open b_selllabel
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                if (s_open)
                {
                    if (b_sellpositions.Length == 0)
                    {
                        var volume = Volume;
                        if (b_buypositions.Length != 0)
                            volume = Volume * 2;
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        s_open = false;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) >= bottom_decrease)
                    {
                        var volume = Volume;
                        if (b_buypositions.Length != 0)
                            volume = Volume * 2;
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        s_open = false;
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
                        s_open = false;
                        return;
                    }
                }
                //Open b_buylabel
                if (b_open)
                {
                    if (b_buypositions.Length == 0)
                    {
                        var volume = Volume;
                        if (b_buypositions.Length != 0)
                            volume = Volume * 2;
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, b_buylabel);
                        Open_Consolidation = 0;
                        b_open = false;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) <= top_decrease)
                    {
                        var volume = Volume;
                        if (b_buypositions.Length != 0)
                            volume = Volume * 2;
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, b_buylabel);
                        Open_Consolidation = 0;
                        b_open = false;
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
                        b_open = false;
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
        #region
        private double TotalProfit(Position[] pos)
        {
            double totalprofit;
            totalprofit = 0;
            if (pos.Length != 0)
                foreach (var position in pos)
                {
                    totalprofit += position.NetProfit;
                }
            return totalprofit;
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
