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
    public class Bollinger_Deviations_Trend_Count : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

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
        int B1, B2, B3, B4, B5, B6, B7, B8, B9, B10,
        B11, B12, B13, B14, B15, B16, B17;
        int S1, S2, S3, S4, S5, S6, S7, S8, S9, S10,
        S11, S12, S13, S14, S15, S16, S17;
        #endregion
        protected override void OnStart()
        {
            bollingerBands = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollingerBands_increase = Indicators.BollingerBands(Source, Periods, Deviations + Deviations_Amendment, MAType);
            bollingerBands_decrease = Indicators.BollingerBands(Source, Periods, Deviations - Deviations_Amendment, MAType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;
            Positions.Opened += PositionsOnOpened;
            B1 = 0;
            B2 = 0;
            B3 = 0;
            B4 = 0;
            B5 = 0;
            B6 = 0;
            B7 = 0;
            B8 = 0;
            B9 = 0;
            B10 = 0;
            B11 = 0;
            B12 = 0;
            B13 = 0;
            B14 = 0;
            B15 = 0;
            B16 = 0;
            B17 = 0;
            S1 = 0;
            S2 = 0;
            S3 = 0;
            S4 = 0;
            S5 = 0;
            S6 = 0;
            S7 = 0;
            S8 = 0;
            S9 = 0;
            S10 = 0;
            S11 = 0;
            S12 = 0;
            S13 = 0;
            S14 = 0;
            S15 = 0;
            S16 = 0;
            S17 = 0;
        }
        private void PositionsOnOpened(PositionOpenedEventArgs args)
        {
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
            if (args.Position.TradeType == TradeType.Buy)
            {
                if (b_buypositions.Length == 1)
                    B1++;
                if (b_buypositions.Length == 2)
                    B2++;
                if (b_buypositions.Length == 3)
                    B3++;
                if (b_buypositions.Length == 4)
                    B4++;
                if (b_buypositions.Length == 5)
                    B5++;
                if (b_buypositions.Length == 6)
                    B6++;
                if (b_buypositions.Length == 7)
                    B7++;
                if (b_buypositions.Length == 8)
                    B8++;
                if (b_buypositions.Length == 9)
                    B9++;
                if (b_buypositions.Length == 10)
                    B10++;
                if (b_buypositions.Length == 11)
                    B11++;
                if (b_buypositions.Length == 12)
                    B12++;
                if (b_buypositions.Length == 13)
                    B13++;
                if (b_buypositions.Length == 14)
                    B14++;
                if (b_buypositions.Length == 15)
                    B15++;
                if (b_buypositions.Length == 16)
                    B16++;
                if (b_buypositions.Length == 17)
                    B17++;
            }
            if (args.Position.TradeType == TradeType.Sell)
            {
                if (b_sellpositions.Length == 1)
                    S1++;
                if (b_sellpositions.Length == 2)
                    S2++;
                if (b_sellpositions.Length == 3)
                    S3++;
                if (b_sellpositions.Length == 4)
                    S4++;
                if (b_sellpositions.Length == 5)
                    S5++;
                if (b_sellpositions.Length == 6)
                    S6++;
                if (b_sellpositions.Length == 7)
                    S7++;
                if (b_sellpositions.Length == 8)
                    S8++;
                if (b_sellpositions.Length == 9)
                    S9++;
                if (b_sellpositions.Length == 10)
                    S10++;
                if (b_sellpositions.Length == 11)
                    S11++;
                if (b_sellpositions.Length == 12)
                    S12++;
                if (b_sellpositions.Length == 13)
                    S13++;
                if (b_sellpositions.Length == 14)
                    S14++;
                if (b_sellpositions.Length == 15)
                    S15++;
                if (b_sellpositions.Length == 16)
                    S16++;
                if (b_sellpositions.Length == 17)
                    S17++;
            }
            Print("B:" + " " + B1 + " " + B2 + " " + B3 + " " + B4 + " " + B5 + " " + B6 + " " + B7 + " " + B8 + " " + B9 + " " + B10 + " " + B11 + " " + B12 + " " + B13 + " " + B14 + " " + B15 + " " + B16 + " " + B17);
            Print("S:" + " " + S1 + " " + S2 + " " + S3 + " " + S4 + " " + S5 + " " + S6 + " " + S7 + " " + S8 + " " + S9 + " " + S10 + " " + S11 + " " + S12 + " " + S13 + " " + S14 + " " + S15 + " " + S16 + " " + S17);

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
                if (Symbol.Bid > top_decrease)
                {
                    if (b_buypositions.Length != 0)
                    {
                        foreach (var position in b_buypositions)
                            ClosePosition(position);
                    }
                }
                //Close SellPositions
                if (Symbol.Ask < bottom_decrease)
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
