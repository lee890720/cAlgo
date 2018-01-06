using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class test : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        protected override void OnStart()
        {
            Symbol sym1 = MarketData.GetSymbol("XAUUSD");
            Symbol sym2 = MarketData.GetSymbol("XAGUSD");
            Print(sym1 + "-" + sym2);
            Print(sym1.PipSize + "-" + sym2.PipSize);
        }

        protected override void OnTick()
        {
            ExecuteMarketOrder(TradeType.Buy, Symbol, 1000);
            Stop();
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
