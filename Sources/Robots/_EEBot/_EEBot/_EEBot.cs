using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;
using cAlgo.Strategies;
using System.Collections.Generic;
namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _EEBot : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 120)]
        public int period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string symbol2 { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        Symbol symbol;
        string MaAbove = "maabove";
        string MaBelow = "mabelow";
        OrderParams initBuyEur, initBuyGbp, initSellEur, initSellGbp;
        EEStrategy ees;
        protected override void OnStart()
        {
            symbol = MarketData.GetSymbol(symbol2);
            ees = new EEStrategy(this, period, symbol2, Distance);
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuyEur = new OrderParams(TradeType.Buy, Symbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuyGbp = new OrderParams(TradeType.Buy, Symbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellEur = new OrderParams(TradeType.Sell, symbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellGbp = new OrderParams(TradeType.Sell, symbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            if (ees.signal() == TradeType.Buy)
            {
                this.executeOrder(initBuyEur);
                this.executeOrder(initSellGbp);
            }
            if (ees.signal() == TradeType.Sell)
            {
                this.executeOrder(initSellEur);
                this.executeOrder(initBuyGbp);
            }
            if (Positions.Count != 0)
            {
                if (ees.singnalS() == "closebuy")
                {
                    this.closeAllPositions(MaAbove);
                }
                if (ees.singnalS() == "closesell")
                {
                    this.closeAllPositions(MaBelow);
                }
            }
        }
    }
}
