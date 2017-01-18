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
    public class Bollinger : Robot
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
        public int Deviations { get; set; }

        [Parameter("Bollinger Bands MA Type")]
        public MovingAverageType MAType { get; set; }
        #endregion
        #region
        BollingerBands bollingerBands;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        int b_consolidation;
        #endregion
        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            b_consolidation = ConsolidationPeriods;
        }
        protected override void OnBar()
        {
            #region parameter
            var top = bollingerBands.Top.Last(1);
            var bottom = bollingerBands.Bottom.Last(1);
            var balancelot = GetBalance_Lots();
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
            #endregion
            #region define b_consolidation and h_consolidation
            if (top - bottom >= BandHeightPips * Symbol.PipSize)
            {
                b_consolidation = b_consolidation + 1;
            }
            else
            {
                b_consolidation = 0;
            }
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if (b_consolidation >= ConsolidationPeriods)
            {
                //Close BuyPositions
                if (Symbol.Bid > top)
                {
                    if (b_buypositions.Length != 0)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                    }
                }
                //Close SellPositions
                if (Symbol.Ask < bottom)
                {
                    if (b_sellpositions.Length != 0)
                    {
                        foreach (var position in b_sellpositions)
                            ClosePosition(position);
                    }
                }
                //Open b_selllabel
                if (Symbol.Bid > top)
                {
                    if (b_sellpositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, b_selllabel);
                        b_consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) >= bottom)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, b_selllabel);
                        b_consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) < bottom)
                    {
                        foreach (var position in b_sellpositions)
                            ClosePosition(position);
                        b_consolidation = 0;
                        return;
                    }
                }
                //Open b_buylabel
                if (Symbol.Ask < bottom)
                {
                    if (b_buypositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, b_buylabel);
                        b_consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) <= top)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, b_buylabel);
                        b_consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) > top)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                        b_consolidation = 0;
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
        #region Get Side Of Positions
        private string GetSideOfPositions()
        {
            string side = "BuyOrSell";
            int buy = 0, sell = 0;
            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy && position.Label == b_buylabel)
                    buy++;
                if (position.TradeType == TradeType.Sell && position.Label == b_selllabel)
                    sell++;
            }
            if (buy > sell)
                side = "Buy";
            if (sell > buy)
                side = "Sell";
            return side;
        }
        #endregion
    }
}
