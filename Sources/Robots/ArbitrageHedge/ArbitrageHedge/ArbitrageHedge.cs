﻿using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        [Parameter(DefaultValue = 1)]
        public int timer { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsTrade { get; set; }

        private Symbol symbol;
        private string AboveLabel;
        private string BelowLabel;
        private OrderParams initBuy, initSell;
        private Currency_Highlight currency;
        private Currency_Sub_Highlight currency_sub;
        private bool AboveCross;
        private bool BelowCross;

        protected override void OnStart()
        {
            AboveCross = false;
            BelowCross = false;
            // Currency_Highlight has a public parameter that it's BarsAgo.
            currency = Indicators.GetIndicator<Currency_Highlight>(FirstSymbol, SecondSymbol, Period, Distance);
            // Currency_Sub_Highlight has a public parameter that it's SIG.
            currency_sub = Indicators.GetIndicator<Currency_Sub_Highlight>(FirstSymbol, SecondSymbol, Period, Distance);
            string currencysymbol = (FirstSymbol.Substring(0, 3) == "USD" ? FirstSymbol.Substring(3) : FirstSymbol.Substring(0, 3)) + (SecondSymbol.Substring(0, 3) == "USD" ? SecondSymbol.Substring(3) : SecondSymbol.Substring(0, 3));
            Print("The currency of the current transaction is : " + currencysymbol + ".");
            symbol = MarketData.GetSymbol(currencysymbol);
            AboveLabel = "Above" + "-" + symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            BelowLabel = "Below" + "-" + symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuy = new OrderParams(TradeType.Buy, symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSell = new OrderParams(TradeType.Sell, symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            // Symbol.Pipsize
            Symbol sym1 = MarketData.GetSymbol("EURUSD");
            Symbol sym2 = MarketData.GetSymbol("GBPUSD");
            Symbol sym3 = MarketData.GetSymbol("USDCHF");
            Symbol sym4 = MarketData.GetSymbol("USDJPY");
            Symbol sym5 = MarketData.GetSymbol("AUDUSD");
            Symbol sym6 = MarketData.GetSymbol("NZDUSD");
            Symbol sym7 = MarketData.GetSymbol("USDCAD");
            Symbol sym8 = MarketData.GetSymbol("XAUUSD");
            Print(sym1.PipSize + "-" + sym2.PipSize + "-" + sym3.PipSize + "-" + sym4.PipSize + "-" + sym5.PipSize + "-" + sym6.PipSize + "-" + sym7.PipSize + "-" + sym8.PipSize);
        }

        protected override void OnTick()
        {
            chartdraw();
            if (IsTrade)
            {
                #region Parameter
                var UR = currency.Result.LastValue;
                var UA = currency.Average.LastValue;
                var SR = currency_sub.Result.LastValue;
                var SA = currency_sub.Average.LastValue;

                List<Position> Pos_above = new List<Position>(this.GetPositions(AboveLabel));
                List<Position> Pos_below = new List<Position>(this.GetPositions(BelowLabel));
                Pos_above.Reverse();
                Pos_below.Reverse();
                #endregion

                #region Open
                if (opensignal() == "above")
                {
                    initSell.Volume = Init_Volume * Math.Pow(2, Pos_above.Count);
                    initSell.Label = AboveLabel;
                    initSell.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_above)) + "-" + string.Format("{0:000}", Pos_above.Count + 1);
                    this.executeOrder(initSell);
                    AboveCross = false;
                }
                if (opensignal() == "below")
                {
                    initBuy.Volume = Init_Volume * Math.Pow(2, Pos_below.Count);
                    initBuy.Label = BelowLabel;
                    initBuy.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_below)) + "-" + string.Format("{0:000}", Pos_below.Count + 1);
                    this.executeOrder(initBuy);
                    BelowCross = false;
                }
                #endregion

                #region Close
                if (Pos_above.Count != 0)
                    if (UR <= UA)
                        this.closeAllLabel(AboveLabel);
                if (Pos_below.Count != 0)
                    if (UR >= UA)
                        this.closeAllLabel(BelowLabel);
                #endregion

                #region Cross
                if (Pos_above.Count == 0)
                    AboveCross = true;
                else
                {
                    if (SR > SA)
                        AboveCross = true;
                }
                if (Pos_below.Count == 0)
                    BelowCross = true;
                else
                {
                    if (SR < SA)
                        BelowCross = true;
                }
                #endregion
            }
        }

        private int CrossAgo(List<Position> pos)
        {
            int cross = (int)Math.Round((double)currency.BarsAgo / 24) * 5;
            int crossago = Distance / 2;
            if (pos.Count == 0)
                if (cross > crossago)
                    crossago = cross;
            if (pos.Count != 0)
                crossago = Convert.ToInt16(pos[0].Comment.Substring(7, 3)) + 5;
            return crossago;
        }

        private string opensignal()
        {
            #region Parameter
            string signal = null;
            var UR = currency.Result.LastValue;
            var UA = currency.Average.LastValue;
            var SR = currency_sub.Result.LastValue;
            var SA = currency_sub.Average.LastValue;
            var sig = currency_sub.SIG;
            if (sig == null)
            {
                return signal;
            }

            List<Position> Pos_above = new List<Position>(this.GetPositions(AboveLabel));
            List<Position> Pos_below = new List<Position>(this.GetPositions(BelowLabel));
            Pos_above.Reverse();
            Pos_below.Reverse();

            var now = DateTime.UtcNow;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_above.Count != 0)
                lastPosTime.Add(Pos_above[0].EntryTime.AddHours(timer));
            if (Pos_below.Count != 0)
                lastPosTime.Add(Pos_below[0].EntryTime.AddHours(timer));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-timer) : lastPosTime.Max();
            #endregion
            if (DateTime.Compare(now, Pos_LastTime) > 0)
            {
                if (sig == "above" && AboveCross)
                {
                    signal = "above";
                    if (Pos_above.Count != 0)
                    {
                        if (UR - CrossAgo(Pos_above) < Convert.ToDouble(Pos_above[0].Comment.Substring(0, 6)))
                            signal = null;
                    }
                }
                if (sig == "below" && BelowCross)
                {
                    signal = "below";
                    if (Pos_below.Count != 0)
                    {
                        if (UR + CrossAgo(Pos_below) > Convert.ToDouble(Pos_below[0].Comment.Substring(0, 6)))
                            signal = null;
                    }
                }
            }
            return signal;
        }

        private void chartdraw()
        {
            #region Parameter
            var UR = currency.Result.LastValue;
            var UA = currency.Average.LastValue;
            var SR = currency_sub.Result.LastValue;
            var SA = currency_sub.Average.LastValue;

            List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(AboveLabel));
            List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(BelowLabel));
            Pos_eurchfabove.Reverse();
            Pos_eurchfbelow.Reverse();

            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_eurchfabove.Count != 0)
                lastPosTime.Add(Pos_eurchfabove[0].EntryTime.AddHours(timer));
            if (Pos_eurchfbelow.Count != 0)
                lastPosTime.Add(Pos_eurchfbelow[0].EntryTime.AddHours(timer));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-timer) : lastPosTime.Max();
            #endregion

            double marginlevel = 0;
            if (this.Account.MarginLevel.HasValue)
                marginlevel = Math.Round((double)this.Account.MarginLevel);
            List<string> _currency = new List<string>();
            if (Positions.Count != 0)
                foreach (var pos in Positions)
                    if (!_currency.Contains(pos.SymbolCode + "-" + Symbol.VolumeToQuantity(this.TotalLots(pos.Label))))
                        _currency.Add(pos.SymbolCode + "-" + Symbol.VolumeToQuantity(this.TotalLots(pos.Label)));
            ChartObjects.DrawText("info1", this.Account.Number + " - " + Symbol.VolumeToQuantity(this.TotalLots()) + " - " + Pos_LastTime, StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("info2", "\nSR-" + Math.Round(SR) + "\t\tSA-" + Math.Round(SA), StaticPosition.TopLeft, Colors.White);
            int i = 0;
            string si = null;
            string t = null;
            string tt = "\t\t";
            foreach (string c in _currency)
            {
                i++;
                si = "_C" + i.ToString();
                ChartObjects.DrawText(si, "\n\n" + t + c, StaticPosition.TopLeft, Colors.White);
                t += tt;
            }
            //ChartObjects.DrawText("info2", "\nEq-" + Math.Round(this.Account.Equity) + "\tPr-" + Math.Round(this.Account.UnrealizedNetProfit) + "\tMa-" + Math.Round(this.Account.Margin) + "\tLe-" + marginlevel + "%", StaticPosition.TopLeft, Colors.White);
        }
    }
}
