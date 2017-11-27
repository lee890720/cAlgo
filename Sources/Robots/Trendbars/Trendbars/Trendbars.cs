using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Trendbars : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here
        }

        protected override void OnBar()
        {
            int ii = MarketSeries.OpenTime.LastValue.DayOfWeek - DayOfWeek.Monday;
            if (ii == -1)
                ii = 6;
            // i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
            TimeSpan ts = new TimeSpan(ii, 0, 0, 0);
            DateTime sunday = MarketSeries.OpenTime.LastValue.Subtract(ts);
            string str = sunday.ToShortDateString() + " " + "21:00";
            DateTime dt = Convert.ToDateTime(str);
            Print(dt);
            int bars = MarketSeries.Bars();
            int index = bars - 1;
            for (int i = bars - 1; i >= 0; i--)
            {
                if (MarketSeries.OpenTime[i] == dt)
                    index = i;
            }
            int bull = 0;
            int bear = 0;
            for (int i = index; i < bars - 2; i++)
            {
                if (MarketSeries.isBullCandle(i) == true)
                    bull++;
                if (MarketSeries.isBearCandle(i) == true)
                    bear++;
            }
            Print(bull + "and" + bear);
            if (bull > bear && MarketSeries.isBullCandle(bars - 2) == true)
            {
                ExecuteMarketOrder(TradeType.Buy, Symbol, 1000, "buy");
            }
            if (bull < bear && MarketSeries.isBearCandle(bars - 2) == true)
            {
                ExecuteMarketOrder(TradeType.Sell, Symbol, 1000, "sell");
            }

            if (this.GetPositions("buy").Length > 0)
                if (this.TotalProfits("buy") > 0)
                {
                    Print(this.TotalProfits("buy"));
                    foreach (var pos in this.GetPositions("buy"))
                    {
                        ClosePosition(pos);
                    }
                }
            if (this.GetPositions("sell").Length > 0)
                if (this.TotalProfits("sell") > 0)
                {
                    Print(this.TotalProfits("sell"));
                    foreach (var pos in this.GetPositions("sell"))
                    {
                        ClosePosition(pos);
                    }
                }
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
        #region 得到一周的周一和周日的日期
        /// <summary> 
        /// 计算本周的周一日期 
        /// </summary> 
        /// <returns></returns> 
        public static DateTime GetMondayDate()
        {
            return GetMondayDate(DateTime.UtcNow);
        }
        /// <summary> 
        /// 计算本周周日的日期 
        /// </summary> 
        /// <returns></returns> 
        public static DateTime GetSundayDate()
        {
            return GetSundayDate(DateTime.UtcNow);
        }
        /// <summary> 
        /// 计算某日起始日期（礼拜一的日期） 
        /// </summary> 
        /// <param name="someDate">该周中任意一天</param> 
        /// <returns>返回礼拜一日期，后面的具体时、分、秒和传入值相等</returns> 
        public static DateTime GetMondayDate(DateTime someDate)
        {
            int i = someDate.DayOfWeek - DayOfWeek.Monday;
            if (i == -1)
                i = 6;
            // i值 > = 0 ，因为枚举原因，Sunday排在最前，此时Sunday-Monday=-1，必须+7=6。 
            TimeSpan ts = new TimeSpan(i, 0, 0, 0);
            return someDate.Subtract(ts);
        }
        /// <summary> 
        /// 计算某日结束日期（礼拜日的日期） 
        /// </summary> 
        /// <param name="someDate">该周中任意一天</param> 
        /// <returns>返回礼拜日日期，后面的具体时、分、秒和传入值相等</returns> 
        public static DateTime GetSundayDate(DateTime someDate)
        {
            int i = someDate.DayOfWeek - DayOfWeek.Sunday;
            //if (i != 0)
            //   i = 7 - i;
            // 因为枚举原因，Sunday排在最前，相减间隔要被7减。 
            //TimeSpan ts=new TimeSpan(i, 0, 0, 0);
            //return someDate.Add(ts);
            TimeSpan ts = new TimeSpan(i, 0, 0, 0);
            return someDate.Subtract(ts);
        }
        #endregion
    }
}
