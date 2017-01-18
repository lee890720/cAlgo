using System;
using cAlgo.API;
using cAlgo.API.Indicators;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(AccessRights = AccessRights.None)]
    public class Scalper : Robot
    {
        [Parameter("BarCount", DefaultValue = 3)]
        public int BarCount { get; set; }

        [Parameter("MaxOrders", DefaultValue = 100)]
        public int MaxOrders { get; set; }

        [Parameter("TakeProfit", DefaultValue = 1000)]
        public int TakeProfit { get; set; }

        [Parameter("StopLoss", DefaultValue = 1000)]
        public int StopLoss { get; set; }

        [Parameter("Volume", DefaultValue = 1000)]
        public int Volume { get; set; }

        [Parameter("MaxDropDown", DefaultValue = 0)]
        public double MaxDropDown { get; set; }

        [Parameter("MaxProfit", DefaultValue = 0)]
        public double MaxProfit { get; set; }

        private int OpenIndex = 0;
        private double StartBalanse;
        string buylabel = "buy";
        string selllabel = "sell";
        private DateTime dt;

        protected override void OnStart()
        {
            StartBalanse = Account.Balance;
            dt = Server.Time;
        }

        protected override void OnBar()
        {
            int last = MarketSeries.Close.Count - 1;
            var buypositions = GetBuyPositions();
            var sellpositions = GetSellPositions();
            if (!(MarketSeries.Open[last] == MarketSeries.High[last] && MarketSeries.Open[last] == MarketSeries.Low[last]))
                return;
            if (dt.Date != Server.Time.Date)
            {
                StartBalanse = Account.Balance;
                dt = Server.Time;
            }

            double bp = (StartBalanse - Account.Balance) / (StartBalanse / 100);
            if (bp > 0 && bp >= MaxDropDown && MaxDropDown != 0)
                return;
            if (bp < 0 && Math.Abs(bp) >= MaxProfit && MaxProfit != 0)
                return;
            if (last - OpenIndex > BarCount)
            {
                if (IsBuy(last))
                {
                    long volume = Volume;
                    //if (sellpositions.Length != 0)
                    //   volume = TotalLots(sellpositions);
                    if (sellpositions.Length != 0 && buypositions.Length != 0 && TotalProfit(buypositions) + TotalProfit(sellpositions) > 0)
                    {
                        foreach (var position in sellpositions)
                            ClosePosition(position);
                        foreach (var position in buypositions)
                            ClosePosition(position);
                        volume = Volume;
                    }
                    if (sellpositions.Length != 0)
                    {
                        foreach (var position in sellpositions)
                            ClosePosition(position);
                        volume = Volume;
                    }
                    if (buypositions.Length == 0 || TotalProfit(buypositions) > 0)
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, buylabel, 100, 100);
                    OpenIndex = last;
                }
                if (IsSell(last))
                {
                    long volume = Volume;
                    //if (buypositions.Length != 0)
                    //   volume = TotalLots(buypositions);
                    if (sellpositions.Length != 0 && buypositions.Length != 0 && TotalProfit(buypositions) + TotalProfit(sellpositions) > 0)
                    {
                        foreach (var position in sellpositions)
                            ClosePosition(position);
                        foreach (var position in buypositions)
                            ClosePosition(position);
                        volume = Volume;
                    }
                    if (buypositions.Length != 0)
                    {
                        foreach (var position in buypositions)
                            ClosePosition(position);
                        volume = Volume;
                    }
                    if (sellpositions.Length == 0 || TotalProfit(sellpositions) > 0)
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, selllabel, 100, 100);
                    OpenIndex = last;
                }
            }
        }

        private Position[] GetBuyPositions()
        {
            return Positions.FindAll(buylabel, Symbol);
        }
        private Position[] GetSellPositions()
        {
            return Positions.FindAll(selllabel, Symbol);
        }

        private bool IsSell(int last)
        {

            for (int i = BarCount; i > 0; i--)
            {
                if (MarketSeries.Open[last - i] < MarketSeries.Close[last - i])
                    return false;
                if (i < 2)
                    continue;
                if (MarketSeries.High[last - i] > MarketSeries.High[last - i - 1])
                    return false;
            }
            return true;
        }

        private bool IsBuy(int last)
        {

            for (int i = BarCount; i > 0; i--)
            {
                if (MarketSeries.Open[last - i] > MarketSeries.Close[last - i])
                    return false;
                if (i < 2)
                    continue;
                if (MarketSeries.Low[last - i] < MarketSeries.Low[last - i - 1])
                    return false;
            }
            return true;
        }
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
    }
}
