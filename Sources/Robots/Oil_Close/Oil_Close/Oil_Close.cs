using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_Close : Robot
    {

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnTick()
        {
            List<Position> xlist = new List<Position>();
            foreach (var p in Positions)
            {
                if (p.SymbolCode == "XBRUSD" || p.SymbolCode == "XTIUSD")
                {
                    xlist.Add(p);
                }
            }
            var profit = xlist.Select(x => x.NetProfit).Sum();
            Print(profit.ToString());
            //if (profit > 0)
            //{
            //    foreach (var xp in xlist)
            //        ClosePosition(xp);
            //}
            if (Symbol.MarketHours.IsOpened())
                Print("OK");
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
