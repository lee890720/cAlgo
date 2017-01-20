using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Lib
{
    public static class PriceExtensions
    {
        public static double AveragePrice(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            long totallots = 0;
            double lotsprice = 0;
            double averageprice = 0;
            foreach (var pos in poss)
            {
                totallots += pos.Volume;
                lotsprice += pos.Volume * pos.EntryPrice;
            }
            averageprice = lotsprice / totallots;
            return averageprice;
        }
        public static double MaxPrice(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            double maxprice = 0;
            foreach (var pos in poss)
            {
                if (maxprice == 0)
                    maxprice = pos.EntryPrice;
                if (maxprice < pos.EntryPrice)
                    maxprice = pos.EntryPrice;
            }
            return maxprice;
        }
        public static double MinPrice(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            double minprice = 0;
            foreach (var pos in poss)
            {
                if (minprice == 0)
                    minprice = pos.EntryPrice;
                if (minprice < pos.EntryPrice)
                    minprice = pos.EntryPrice;
            }
            return minprice;
        }
    }
}
