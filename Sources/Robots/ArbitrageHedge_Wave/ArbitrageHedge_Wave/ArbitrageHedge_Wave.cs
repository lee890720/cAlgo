using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge_Wave : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public double Init_Volume { get; set; }

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

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsTrade { get; set; }

        private Symbol _symbol, _firstsymbol, _secondsymbol;
        private bool SymbolExist;
        private string AboveLabel;
        private string BelowLabel;
        private OrderParams initBuy, initSell;
        private OrderParams initBuyF, initBuyS, initSellF, initSellS;
        private Wave_Currency_Highlight currency;
        private Wave_Currency_Sub_Highlight currency_sub;
        private bool AboveCross;
        private bool BelowCross;
        private List<string> list_mark = new List<string>();
        private List<string> _metalssymbol = new List<string>();
        private List<string> _oilsymbol = new List<string>();
        private Colors PCorel, NCorel, NoCorel;

        protected override void OnStart()
        {
            PCorel = Colors.Lime;
            NCorel = Colors.OrangeRed;
            NoCorel = Colors.Gray;
            _metalssymbol.Add("XAUUSD");
            _metalssymbol.Add("XAGUSD");
            _oilsymbol.Add("XBRUSD");
            _oilsymbol.Add("XTIUSD");
            AboveCross = false;
            BelowCross = false;
            // Currency_Highlight has a public parameter that it's BarsAgo.
            currency = Indicators.GetIndicator<Wave_Currency_Highlight>(FirstSymbol, SecondSymbol, Period, Distance, Ratio, Magnify);
            // Currency_Sub_Highlight has a public parameter that it's SIG.
            currency_sub = Indicators.GetIndicator<Wave_Currency_Sub_Highlight>(FirstSymbol, SecondSymbol, Period, Distance, Ratio, Magnify);
            string _currencysymbol = (FirstSymbol.Substring(0, 3) == "USD" ? FirstSymbol.Substring(3, 3) : FirstSymbol.Substring(0, 3)) + (SecondSymbol.Substring(0, 3) == "USD" ? SecondSymbol.Substring(3, 3) : SecondSymbol.Substring(0, 3));
            Print("The currency of the current transaction is : " + _currencysymbol + ".");
            AboveLabel = "Above" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();
            BelowLabel = "Below" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();
            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);
            if (Symbol.Code == _currencysymbol)
            {
                SymbolExist = true;
                Print(_currencysymbol + " exists.");
                _symbol = MarketData.GetSymbol(_currencysymbol);
                double slippage = 2;
                //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
                initBuy = new OrderParams(TradeType.Buy, _symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSell = new OrderParams(TradeType.Sell, _symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
            }
            else
            {
                SymbolExist = false;
                Print(_currencysymbol + " doesn't exist.");
                double slippage = 2;
                //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
                initBuyF = new OrderParams(TradeType.Buy, _firstsymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSellF = new OrderParams(TradeType.Sell, _firstsymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initBuyS = new OrderParams(TradeType.Buy, _secondsymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSellS = new OrderParams(TradeType.Sell, _secondsymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
            }
        }

        protected override void OnTick()
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

            chartdraw();

            if (Pos_above.Count != 0)
                foreach (var p in Pos_above)
                {
                    if (!list_mark.Contains(p.Comment.Substring(15)))
                        list_mark.Add(p.Comment.Substring(15));
                }
            if (Pos_below.Count != 0)
                foreach (var p in Pos_below)
                {
                    if (!list_mark.Contains(p.Comment.Substring(15)))
                        list_mark.Add(p.Comment.Substring(15));
                }
            if (IsTrade)
            {
                #region Open
                if (SymbolExist)
                {
                    if (opensignal() == "above")
                    {
                        initSell.Volume = _symbol.NormalizeVolume(Init_Volume * Math.Pow(2, Pos_above.Count), RoundingMode.ToNearest);
                        initSell.Label = AboveLabel;
                        initSell.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_above)) + "-" + string.Format("{0:000}", Pos_above.Count + 1) + "-" + currency_sub.Mark;
                        this.executeOrder(initSell);
                        AboveCross = false;
                    }
                    if (opensignal() == "below")
                    {
                        initBuy.Volume = _symbol.NormalizeVolume(Init_Volume * Math.Pow(2, Pos_below.Count), RoundingMode.ToNearest);
                        initBuy.Label = BelowLabel;
                        initBuy.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_below)) + "-" + string.Format("{0:000}", Pos_below.Count + 1) + "-" + currency_sub.Mark;
                        this.executeOrder(initBuy);
                        BelowCross = false;
                    }
                }
                else
                {
                    double first_R = 1;
                    double second_R = 1;
                    if (_metalssymbol.Contains(FirstSymbol))
                        first_R = 1000;
                    if (_metalssymbol.Contains(SecondSymbol))
                        second_R = 1000;
                    if (_oilsymbol.Contains(FirstSymbol))
                        first_R = 10;
                    if (_oilsymbol.Contains(SecondSymbol))
                        second_R = 10;
                    double _firstvolume = Init_Volume / first_R;
                    double _secondvolume = Init_Volume / second_R;
                    if (Ratio >= 1)
                    {
                        _firstvolume = _firstsymbol.NormalizeVolume(Init_Volume / first_R, RoundingMode.ToNearest);
                        _secondvolume = _secondsymbol.NormalizeVolume(Init_Volume * Ratio / second_R, RoundingMode.ToNearest);
                    }
                    else
                    {
                        _firstvolume = _firstsymbol.NormalizeVolume(Init_Volume / Ratio / first_R, RoundingMode.ToNearest);
                        _secondvolume = _secondsymbol.NormalizeVolume(Init_Volume / second_R, RoundingMode.ToNearest);
                    }
                    if (opensignal() == "above")
                    {
                        initSellF.Volume = _firstvolume * Math.Pow(2, Math.Floor((double)Pos_above.Count / 2));
                        initSellF.Label = AboveLabel;
                        initSellF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_above)) + "-" + string.Format("{0:000}", Pos_above.Count + 1) + "-" + currency_sub.Mark;
                        this.executeOrder(initSellF);
                        initBuyS.Volume = _secondvolume * Math.Pow(2, Math.Floor((double)Pos_above.Count / 2));
                        initBuyS.Label = AboveLabel;
                        initBuyS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_above)) + "-" + string.Format("{0:000}", Pos_above.Count + 2) + "-" + currency_sub.Mark;
                        this.executeOrder(initBuyS);
                        AboveCross = false;
                    }
                    if (opensignal() == "below")
                    {
                        initBuyF.Volume = _firstvolume * Math.Pow(2, Math.Floor((double)Pos_below.Count / 2));
                        initBuyF.Label = BelowLabel;
                        initBuyF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_below)) + "-" + string.Format("{0:000}", Pos_below.Count + 1) + "-" + currency_sub.Mark;
                        this.executeOrder(initBuyF);
                        initSellS.Volume = _secondvolume * Math.Pow(2, Math.Floor((double)Pos_below.Count / 2));
                        initSellS.Label = BelowLabel;
                        initSellS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo(Pos_below)) + "-" + string.Format("{0:000}", Pos_below.Count + 2) + "-" + currency_sub.Mark;
                        this.executeOrder(initSellS);
                        BelowCross = false;
                    }
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
            {
                //crossago = Convert.ToInt16(pos[0].Comment.Substring(7, 3)) + 5;
                var c = 0;
                foreach (var p in pos)
                {
                    if (c == 0)
                        c = Convert.ToInt16(p.Comment.Substring(7, 3)) + 5;
                    if (c < Convert.ToInt16(p.Comment.Substring(7, 3)) + 5)
                        c = Convert.ToInt16(p.Comment.Substring(7, 3)) + 5;
                }
                crossago = c;
            }
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
            {
                lastPosTime.Add(Pos_above[0].EntryTime.AddHours(timer));
            }
            if (Pos_below.Count != 0)
            {
                lastPosTime.Add(Pos_below[0].EntryTime.AddHours(timer));
            }
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-timer) : lastPosTime.Max();
            #endregion
            if (DateTime.Compare(now, Pos_LastTime) > 0 && !list_mark.Contains(currency_sub.Mark))
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
                {
                    if (pos.Label == null)
                        continue;
                    if (!_currency.Contains(pos.SymbolCode + ": " + ((_oilsymbol.Contains(pos.SymbolCode) || _metalssymbol.Contains(pos.SymbolCode)) ? this.TotalLots(pos.Label, MarketData.GetSymbol(pos.SymbolCode)) : MarketData.GetSymbol(pos.SymbolCode).VolumeToQuantity(this.TotalLots(pos.Label, MarketData.GetSymbol(pos.SymbolCode))))))
                        _currency.Add(pos.SymbolCode + ": " + ((_oilsymbol.Contains(pos.SymbolCode) || _metalssymbol.Contains(pos.SymbolCode)) ? this.TotalLots(pos.Label, MarketData.GetSymbol(pos.SymbolCode)) : MarketData.GetSymbol(pos.SymbolCode).VolumeToQuantity(this.TotalLots(pos.Label, MarketData.GetSymbol(pos.SymbolCode)))));
                }
            ChartObjects.RemoveAllObjects();
            ChartObjects.DrawText("info1", "\t\t" + this.Account.Number + " - " + Pos_LastTime, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info2/1/1", "\n\t\tB:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info2/1/2", "\n\t\t     " + Math.Round(this.Account.Balance), StaticPosition.TopLeft, getcolors(Math.Round(this.Account.Balance)));
            ChartObjects.DrawText("info2/2/1", "\n\t\t\t\tE:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info2/2/2", "\n\t\t\t\t     " + Math.Round(this.Account.Equity), StaticPosition.TopLeft, getcolors(Math.Round(this.Account.Equity)));
            ChartObjects.DrawText("info2/3/1", "\n\t\t\t\t\t\tN:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info2/3/2", "\n\t\t\t\t\t\t     " + Math.Round(this.Account.UnrealizedNetProfit), StaticPosition.TopLeft, getcolors(Math.Round(this.Account.UnrealizedNetProfit)));
            ChartObjects.DrawText("info2/4/1", "\n\t\t\t\t\t\t\t\tM:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info2/4/2", "\n\t\t\t\t\t\t\t\t     " + Math.Round(this.Account.Margin), StaticPosition.TopLeft, getcolors(Math.Round(this.Account.Margin)));
            //ChartObjects.DrawText("info3", "\n\n\t\tSR: " + Math.Round(SR) + "\t\tSA: " + Math.Round(SA) + "\t\tSIG: " + currency_sub.SIG + "\t\tRatio: " + Ratio + "\t\tMagnify: " + Magnify, StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("info3/1/1", "\n\n\t\tSR:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info3/1/2", "\n\n\t\t       " + Math.Round(SR), StaticPosition.TopLeft, getcolors(Math.Round(SR)));
            ChartObjects.DrawText("info3/2/1", "\n\n\t\t\t\tSA:", StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info3/2/2", "\n\n\t\t\t\t       " + Math.Round(SA), StaticPosition.TopLeft, getcolors(Math.Round(SA)));
            if (currency_sub.SIG == null)
                ChartObjects.DrawText("info3/3", "\n\n\t\t\t\t\t\tSIG: " + "null", StaticPosition.TopLeft, NoCorel);
            else
                ChartObjects.DrawText("info3/3", "\n\n\t\t\t\t\t\tSIG: " + currency_sub.SIG, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info3/4", "\n\n\t\t\t\t\t\t\t\tRatio: " + Ratio, StaticPosition.TopLeft, NoCorel);
            ChartObjects.DrawText("info3/5", "\n\n\t\t\t\t\t\t\t\t\t\tMagnify: " + Magnify, StaticPosition.TopLeft, NoCorel);
            int i = 0;
            string si = null;
            string t = null;
            string tt = "\t\t";
            foreach (string c in _currency)
            {
                i++;
                si = "_C" + i.ToString();
                if (i <= 10)
                    ChartObjects.DrawText(si, "\n\n\n\t\t" + t + c, StaticPosition.TopLeft, NoCorel);
                if (i > 10)
                {
                    t = null;
                    ChartObjects.DrawText(si, "\n\n\n\n\t\t" + t + c, StaticPosition.TopLeft, NoCorel);
                }
                t += tt;
            }
        }

        private Colors getcolors(double dou)
        {
            Colors col = Colors.White;
            //color1 = (Result1.LastValue > 0.8) ? PCorel : (Result1.LastValue < -0.8) ? NCorel : NoCorel;
            if (dou >= 0)
                col = PCorel;
            else
                col = NCorel;
            return col;
        }
    }
}
