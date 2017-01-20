using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Lib
{
    public static class LotExtensions
    {
        public static long MartingaleLot(this Robot robot,string label, double goalprice)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            string IsBorS = "Nope";
            int buy = 0;
            int sell = 0;
            if(poss.Length!=0)
            foreach (var pos in poss)
            { 
                if (pos.TradeType == TradeType.Buy)
                    buy++;
                if (pos.TradeType == TradeType.Sell)
                    sell++;
            }
            if (buy > sell)
                IsBorS = "buy";
            if (sell > buy)
                IsBorS = "sell";
            long marlot = 0;
            if (IsBorS == "buy" && robot.AveragePrice(label) < goalprice)
                return 0;
            if (IsBorS == "sell" && robot.AveragePrice(label) > goalprice)
                return 0;
            marlot = (long)robot.Symbol.NormalizeVolume(((robot.AveragePrice(label) * robot.TotalLots(label) - goalprice * robot.TotalLots(label)) / (goalprice - robot.Symbol.Mid())), RoundingMode.ToNearest);
            return marlot;
        }
        public static long TotalLots(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            long totallots = 0;
            foreach (var pos in poss)
                totallots += pos.Volume;
            return totallots;
        }
        public static long MaxLot(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            long maxlot = 0;
            foreach (var pos in poss)
            {
                if (maxlot == 0)
                    maxlot = pos.Volume;
                if (maxlot < pos.Volume)
                    maxlot = pos.Volume;
            }
            return maxlot;
        }
        public static long MinLot(this Robot robot, string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            long minlot = 0;
            foreach (var pos in poss)
            {
                if (minlot == 0)
                    minlot = pos.Volume;
                if (minlot < pos.Volume)
                    minlot = pos.Volume;
            }
            return minlot;
        }
        public static long BalanceLots(this Robot robot, double per)
        {
            var balance = robot.Account.Balance;
            var perP = 0.1 / per;
            var lot = robot.Symbol.NormalizeVolume(balance / perP, RoundingMode.ToNearest);
            return lot;
        }
    }
}
