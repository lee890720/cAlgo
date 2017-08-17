using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol2 { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        private Symbol OtherSymbol;
        private string MaAbove = "maabove";
        private string MaBelow = "mabelow";
        private OrderParams initBuyEur, initBuyGbp, initSellEur, initSellGbp;
        private MultiCurrency MC;
        protected override void OnStart()
        {
            MC = Indicators.GetIndicator<MultiCurrency>(Period, Symbol2);
            OtherSymbol = MarketData.GetSymbol(Symbol2);
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuyEur = new OrderParams(TradeType.Buy, Symbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuyGbp = new OrderParams(TradeType.Buy, OtherSymbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellEur = new OrderParams(TradeType.Sell, Symbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellGbp = new OrderParams(TradeType.Sell, OtherSymbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            if (opensignal() == "buyeur")
            {
                this.executeOrder(initBuyEur);
                this.executeOrder(initSellGbp);
            }
            if (opensignal() == "selleur")
            {
                this.executeOrder(initSellEur);
                this.executeOrder(initBuyGbp);
            }
            if (closesignal() == "closebuy")
            {
                this.closeAllLabel(MaAbove);
            }
            if (closesignal() == "closesell")
            {
                this.closeAllLabel(MaBelow);
            }
        }
        private string opensignal()
        {
            List<Position> positions = new List<Position>(Positions);
            positions.Reverse();
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            string sig = null;
            if (Positions.Count == 0)
            {
                if (result > average + Distance)
                    sig = "buyeur";
                if (result < average - Distance)
                    sig = "selleur";
            }
            else
            {
                var now = DateTime.UtcNow;
                if (DateTime.Compare(positions[0].EntryTime.AddHours(1), now) < 0)
                {
                    if (result > average + Distance)
                        sig = "buyeur";
                    if (result < average - Distance)
                        sig = "selleur";
                }
                else
                {
                    var eb = Math.Abs(positions[0].EntryPrice - positions[1].EntryPrice) / Symbol.PipSize;
                    if (result > average + eb + 10)
                        sig = "buyeur";
                    if (result < average - eb - 10)
                        sig = "selleur";
                }
            }
            return sig;
        }
        private string closesignal()
        {
            string sig = null;
            List<Position> positions = new List<Position>(Positions);
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            if (positions.Count != 0)
            {
                if (result >= average)
                    sig = "closesell";
                if (result <= average)
                    sig = "closebuy";
            }
            return sig;
        }
    }
}
