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
    public class Double_Bollingers_5M : Robot
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

        [Parameter("Period", DefaultValue = 40)]
        public int Period { get; set; }

        [Parameter("SD Weight Coef", DefaultValue = 2)]
        public double K { get; set; }

        [Parameter("MA Type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MaType { get; set; }

        #endregion
        #region
        BollingerBands boll;
        BollingerBands bollhalf;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        int Open_Consolidation, Close_Consolidation;
        bool IsMargingle;
        #endregion
        protected override void OnStart()
        {
            boll = Indicators.BollingerBands(Source, Period, K, MaType);
            bollhalf = Indicators.BollingerBands(Source, Period / 2, K, MaType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;
            IsMargingle = false;
        }
        protected override void OnBar()
        {
            #region parameter
            var mainmin = boll.Main.Last(1) < bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var mainmax = boll.Main.Last(1) > bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var topmin = boll.Top.Last(1) < bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var topmax = boll.Top.Last(1) > bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var bottommin = boll.Bottom.Last(1) < bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var bottommax = boll.Bottom.Last(1) > bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var balancelot = GetBalance_Lots();
            var b_buypositions = GetB_BuyPositions();
            var b_sellpositions = GetB_SellPositions();
            #endregion
            #region define b_consolidation and c_consolidation
            if (topmax - bottommin >= BandHeightPips * Symbol.PipSize)
            {
                Open_Consolidation = Open_Consolidation + 1;
            }
            else
            {
                Open_Consolidation = 0;
            }
            if (topmax - bottommin >= BandHeightPips * Symbol.PipSize)
            {
                Close_Consolidation = Close_Consolidation + 1;
            }
            else
            {
                Close_Consolidation = 0;
            }
            #endregion
            #region Close Position(b_buylabel and b_selllabel)
            //Close BuyPositions
            if (Symbol.Bid > topmin)
            {
                if (b_buypositions.Length != 0)
                {
                    foreach (var position in b_buypositions)
                        ClosePosition(position);
                    IsMargingle = false;
                }
            }
            //Close SellPositions
            if (Symbol.Ask < bottommax)
            {
                if (b_sellpositions.Length != 0)
                {
                    foreach (var position in b_sellpositions)
                        ClosePosition(position);
                    IsMargingle = false;
                }
            }
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                //Open b_selllabel
                if (Symbol.Bid > topmax)
                {
                    if (b_sellpositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) >= bottommax)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellpositions.Length != 0 && AveragePrice(b_sellpositions) < bottommax)
                    {
                        if (!IsMargingle)
                        {
                            IsMargingle = true;
                            Open_Consolidation = 0;
                            return;
                        }
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_sellpositions) * TotalLots(b_sellpositions) - bottommax * TotalLots(b_sellpositions)) / (bottommax - Symbol.Bid)), RoundingMode.ToNearest);
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
                if (Symbol.Ask < bottommin)
                {
                    if (b_buypositions.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) <= topmin)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buypositions.Length != 0 && AveragePrice(b_buypositions) > topmin)
                    {
                        if (!IsMargingle)
                        {
                            IsMargingle = true;
                            Open_Consolidation = 0;
                            return;
                        }
                        long volume = 0;
                        volume = (long)Symbol.NormalizeVolume(((AveragePrice(b_buypositions) * TotalLots(b_buypositions) - topmin * TotalLots(b_buypositions)) / (topmin - Symbol.Ask)), RoundingMode.ToNearest);
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
        private double MinPrice(Position[] pos)
        {
            double minprice;
            minprice = 0;
            if (pos.Length != 0)
            {
                foreach (var position in pos)
                {
                    if (minprice == 0)
                        minprice = position.EntryPrice;
                    if (minprice > position.EntryPrice)
                        minprice = position.EntryPrice;
                }
            }
            return minprice;
        }
        private double MaxPrice(Position[] pos)
        {
            double maxprice;
            maxprice = 0;
            if (pos.Length != 0)
            {
                foreach (var position in pos)
                {
                    if (maxprice == 0)
                        maxprice = position.EntryPrice;
                    if (maxprice < position.EntryPrice)
                        maxprice = position.EntryPrice;
                }
            }
            return maxprice;
        }
        #endregion
    }
}
