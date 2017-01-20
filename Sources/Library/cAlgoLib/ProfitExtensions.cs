using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Lib
{
   public static  class ProfitExtensions
    {
        public static double TotalProfits(this Robot robot,string label)
        {
            var poss = robot.GetPositions(label);
            if (poss.Length == 0)
                return 0;
            double totalprofits = 0;
            foreach (var pos in poss)
                totalprofits += pos.NetProfit;
            return totalprofits;
        }
    }
}
