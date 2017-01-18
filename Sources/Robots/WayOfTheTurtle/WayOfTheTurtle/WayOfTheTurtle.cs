using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class WayOfTheTurtle : Robot
    {
        #region Parameters
        [Parameter("Volume", DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("ATR Periods", DefaultValue = 24)]
        public int Periods { get; set; }

        [Parameter("ATR MA")]
        public MovingAverageType MAType { get; set; }

        [Parameter("ATR TimeFrame")]
        public TimeFrame timeframe { get; set; }

        AverageTrueRange ATR;
        List<Position> BuyPositions = new List<Position>();
        List<Position> SellPositions = new List<Position>();
        public double LondonOpen = 8;
        public double LondonClose = 17;
        public double NYOpen = 8;
        public double NYClose = 17;
        public double LondonOpenPrice = 0;
        public double NYOpenPrice = 0;
        public double LondonClosePrice = 0;
        #endregion
        protected override void OnStart()
        {
            ATR = Indicators.AverageTrueRange(MarketData.GetSeries(timeframe), Periods, MAType);
        }
        protected override void OnTick()
        {
            #region Parameters
            BuyPositions.Clear();
            SellPositions.Clear();
            foreach (var position in Positions)
            {
                if (position.TradeType == TradeType.Buy)
                    BuyPositions.Add(position);
                if (position.TradeType == TradeType.Sell)
                    SellPositions.Add(position);
            }
            var atr = ATR.Result.LastValue / Symbol.PipSize;
            TimeZoneInfo LocalTimeZone = TimeZoneInfo.Local;
            TimeZoneInfo NYTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            TimeZoneInfo ServerTimeZone = TimeZoneInfo.Utc;
            DateTime LocalTime = MarketSeries.OpenTime.LastValue;
            DateTime LondonTime = TimeZoneInfo.ConvertTime(LocalTime, ServerTimeZone, LondonTimeZone);
            DateTime NYTime = TimeZoneInfo.ConvertTime(LocalTime, ServerTimeZone, NYTimeZone);
            string label = MarketSeries.OpenTime.LastValue.ToString("d");
            Position[] labelpositions = Positions.FindAll(label, Symbol);
            var labelhistory = History.FindAll(label, Symbol);
            #endregion
            if (LondonTime.Hour == LondonOpen + 1)
                LondonOpenPrice = MarketSeries.Open.LastValue;

            if (NYTime.Hour == NYOpen + 1)
            {
                NYOpenPrice = MarketSeries.Open.LastValue;
                #region First Position
                if (Math.Max(LondonOpenPrice, NYOpenPrice) == LondonOpenPrice)
                    if (labelpositions.Length == 0 && labelhistory.Length == 0)
                    {
                        if (SellPositions.Count == 0)
                            ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, label, 10 * atr, 2 * atr);
                        else
                        {
                            if (TotalProfits(SellPositions.ToArray()) > 0)
                            {
                                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, label, 10 * atr, 2 * atr);
                            }
                            else
                            {
                                var Mar = Math.Ceiling(Math.Abs(TotalProfits(SellPositions.ToArray())) / (2 * atr / Symbol.PipSize * Symbol.PipValue)) + 1;
                                Print(Mar + "Sell");
                                var volume = (long)Mar * Volume + TotalLots(SellPositions.ToArray());
                                foreach (var position in SellPositions)
                                {
                                    ClosePosition(position);
                                }
                                ExecuteMarketOrder(TradeType.Sell, Symbol, volume, label, 10 * atr, 2 * atr);
                            }
                        }
                    }
                if (Math.Max(LondonOpenPrice, NYOpenPrice) == NYOpenPrice)
                    if (labelpositions.Length == 0 && labelhistory.Length == 0)
                    {
                        if (BuyPositions.Count == 0)
                            ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, label, 10 * atr, 2 * atr);
                        else
                        {
                            if (TotalProfits(BuyPositions.ToArray()) > 0)
                            {
                                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, label, 10 * atr, 2 * atr);
                            }
                            else
                            {
                                var Mar = Math.Ceiling(Math.Abs(TotalProfits(BuyPositions.ToArray())) / (2 * atr / Symbol.PipSize * Symbol.PipValue)) + 1;
                                Print(Mar + "Buy");
                                var volume = (long)Mar * Volume + TotalLots(BuyPositions.ToArray());
                                foreach (var position in BuyPositions)
                                {
                                    ClosePosition(position);
                                }
                                ExecuteMarketOrder(TradeType.Buy, Symbol, volume, label, 10 * atr, 2 * atr);
                            }
                        }
                    }
                #endregion
            }
        }
        /*________________________________________________________________________
            if (LondonTime.Hour == LondonClose)
            {
                LondonClosePrice = MarketSeries.Open.LastValue;
                #region Second Position
                if (Math.Max(NYOpenPrice, LondonClosePrice) == NYOpenPrice)
                    if (labelpositions.Length != 0 && labelhistory.Length == 0)
                    {
                        if (TotalProfits(labelpositions) > 0)
                            return;
                        else
                        {
                            var Mar = Math.Ceiling(Math.Abs(TotalProfits(labelpositions)) / (2 * atr / Symbol.PipSize * Symbol.PipValue)) + 1;
                            Print(Mar + "SellS");
                            var volume = (long)Mar * Volume;
                            foreach (var position in labelpositions)
                            {
                                ClosePosition(position);
                            }
                            ExecuteMarketOrder(TradeType.Sell, Symbol, volume, label, 10 * atr, 2 * atr);
                        }
                    }
                if (Math.Max(NYOpenPrice, LondonClosePrice) == LondonClosePrice)
                    if (labelpositions.Length != 0 && labelhistory.Length == 0)
                    {
                        if (TotalProfits(labelpositions) > 0)
                            return;
                        else
                        {
                            var Mar = Math.Ceiling(Math.Abs(TotalProfits(labelpositions)) / (2 * atr / Symbol.PipSize * Symbol.PipValue)) + 1;
                            Print(Mar + "BuyS");
                            var volume = (long)Mar * Volume;
                            foreach (var position in labelpositions)
                            {
                                ClosePosition(position);
                            }
                            ExecuteMarketOrder(TradeType.Buy, Symbol, volume, label, 10 * atr, 2 * atr);
                        }
                    }
                #endregion
            }
             ________________________________________________________________________*/

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
        #endregion
        #region Profits
        private double TotalProfits(Position[] pos)
        {
            double totalprofits;
            totalprofits = 0;
            if (pos.Length != 0)
                foreach (var position in pos)
                {
                    totalprofits += position.NetProfit;
                }
            return totalprofits;
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
