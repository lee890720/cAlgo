using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class _EEBot : Robot
    {
        [Parameter(DefaultValue = 120)]
        public int period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string symbol2 { get; set; }

        private _EE ee;

        Symbol symbol;

        protected override void OnStart()
        {
            ee = Indicators.GetIndicator<_EE>(period, symbol2);
            symbol = MarketData.GetSymbol(symbol2);
        }

        protected override void OnTick()
        {
            //if Result>Average Sell GBP and Buy EUR
            //if Result<Average Buy GBP and Sell EUR
            double result = ee.Result.LastValue;
            double average = ee.Average.LastValue;
            if (Positions.Count == 0)
            {
                if (result > average + 30)
                {
                    ExecuteMarketOrder(TradeType.Sell, symbol, 1000);
                    ExecuteMarketOrder(TradeType.Buy, Symbol, 1000);
                }
                if (result < average - 30)
                {
                    ExecuteMarketOrder(TradeType.Buy, symbol, 1000);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, 1000);
                }
            }
            if (Positions.Count != 0 && DateTime.Compare(Positions[Positions.Count - 1].EntryTime, DateTime.Now.AddHours(-1)) > 0)
            {
                if (result > average + 30)
                {
                    ExecuteMarketOrder(TradeType.Sell, symbol, 1000);
                    ExecuteMarketOrder(TradeType.Buy, Symbol, 1000);
                }
                if (result < average - 30)
                {
                    ExecuteMarketOrder(TradeType.Buy, symbol, 1000);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, 1000);
                }
            }
            //if (Positions.Count != 0 && Positions[Positions.Count - 1].EntryTime >= DateTime.Now.AddHours(-1))
            //{
            //    if (result > Math.Abs((Positions[Positions.Count - 1].EntryPrice - Positions[Positions.Count - 2].EntryPrice) / Symbol.PipSize) + 10)
            //    {
            //        ExecuteMarketOrder(TradeType.Sell, symbol, 1000);
            //        ExecuteMarketOrder(TradeType.Buy, Symbol, 1000);
            //    }
            //    if (result < Math.Abs((Positions[Positions.Count - 1].EntryPrice - Positions[Positions.Count - 2].EntryPrice) / Symbol.PipSize) - 10)
            //    {
            //        ExecuteMarketOrder(TradeType.Buy, symbol, 1000);
            //        ExecuteMarketOrder(TradeType.Sell, Symbol, 1000);
            //    }
            //}
            if (Positions.Count != 0)
            {
                if (result >= average)
                {
                    foreach (var p in Positions)
                    {
                        if ((p.SymbolCode == symbol2 && p.TradeType == TradeType.Buy) || (p.SymbolCode == Symbol.Code && p.TradeType == TradeType.Sell))
                            ClosePosition(p);
                    }
                }
                if (result <= average)
                {
                    foreach (var p in Positions)
                    {
                        if ((p.SymbolCode == symbol2 && p.TradeType == TradeType.Sell) || (p.SymbolCode == Symbol.Code && p.TradeType == TradeType.Buy))
                            ClosePosition(p);
                    }
                }
            }
        }
    }
}
