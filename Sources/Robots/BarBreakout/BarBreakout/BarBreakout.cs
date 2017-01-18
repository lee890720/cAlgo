using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BarBreakout : Robot
    {
        [Parameter("Volume", DefaultValue = 1000, MinValue = 1)]
        public int Volume { get; set; }
        private string buylabel = "Buy";
        private string selllabel = "Sell";
        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {
            var count = MarketSeries.Open.Count;
            var buypositions = BuyPositions();
            var sellpositions = SellPositions();
            if (Math.Max(MarketSeries.Open[count - 2], MarketSeries.Close[count - 2]) == MarketSeries.Open[count - 2])
            {
                if (buypositions.Length != 0)
                    foreach (var position in buypositions)
                    {
                        ClosePosition(position);
                    }
                ExecuteMarketOrder(TradeType.Sell, Symbol, Volume, selllabel);
            }
            if (Math.Max(MarketSeries.Open[count - 2], MarketSeries.Close[count - 2]) == MarketSeries.Close[count - 2])
            {
                if (sellpositions.Length != 0)
                    foreach (var position in sellpositions)
                    {
                        ClosePosition(position);
                    }
                ExecuteMarketOrder(TradeType.Buy, Symbol, Volume, buylabel);
            }
        }
        private Position[] BuyPositions()
        {
            return Positions.FindAll(buylabel, Symbol);
        }
        private Position[] SellPositions()
        {
            return Positions.FindAll(selllabel, Symbol);
        }
        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
