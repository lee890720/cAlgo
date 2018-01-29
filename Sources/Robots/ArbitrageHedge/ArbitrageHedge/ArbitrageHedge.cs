using cAlgo.API;
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
        #region Parameter
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public double Init_Volume { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 1)]
        public int _timer { get; set; }

        [Parameter(DefaultValue = 100)]
        public double _break { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 20)]
        public double Distance { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Magnify { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsTrade { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsBreak { get; set; }

        private Currency_Highlight currency;
        private Currency_Sub_Highlight currency_sub;
        private bool AboveCross;
        private bool BelowCross;
        private string AboveLabel, BelowLabel;
        private Symbol _symbol;
        private List<string> list_mark = new List<string>();
        private OrderParams initBuy, initSell;
        #endregion

        protected override void OnStart()
        {
            // Currency_Highlight has two public parameters that were BarsAgo and _ratio.
            currency = Indicators.GetIndicator<Currency_Highlight>(FirstSymbol, SecondSymbol, Period, Distance, Ratio, Magnify);
            // Currency_Sub_Highlight has three public parameters that they were SIG, BarsAgo_Sub and Mark.
            currency_sub = Indicators.GetIndicator<Currency_Sub_Highlight>(FirstSymbol, SecondSymbol, Period, Distance, Ratio, Magnify);

            AboveCross = false;
            BelowCross = false;

            string _currencysymbol = (FirstSymbol.Substring(0, 3) == "USD" ? FirstSymbol.Substring(3, 3) : FirstSymbol.Substring(0, 3));
            _currencysymbol += (SecondSymbol.Substring(0, 3) == "USD" ? SecondSymbol.Substring(3, 3) : SecondSymbol.Substring(0, 3));
            Print("The currency of the current transaction is : " + _currencysymbol + ".");
            AboveLabel = "Above" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();
            BelowLabel = "Below" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();

            #region OrderParams
            if (Symbol.Code == _currencysymbol)
            {
                Print(_currencysymbol + " exists.");
                _symbol = MarketData.GetSymbol(_currencysymbol);
                initBuy = new OrderParams(TradeType.Buy, _symbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSell = new OrderParams(TradeType.Sell, _symbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
            }
            else
                this.Stop();
            #endregion
        }

        protected override void OnTick()
        {
            #region Parameter
            var UR = currency.Result.LastValue;
            var UA = currency.Average.LastValue;
            var SR = currency_sub.Result.LastValue;
            var SA = currency_sub.Average.LastValue;

            Position[] Pos_above = this.GetPositions(AboveLabel);
            Position[] Pos_below = this.GetPositions(BelowLabel);
            #endregion

            #region Cross
            if (Pos_above.Length == 0)
                AboveCross = true;
            else
            {
                if (SR > SA)
                    AboveCross = true;
            }
            if (Pos_below.Length == 0)
                BelowCross = true;
            else
            {
                if (SR < SA)
                    BelowCross = true;
            }
            #endregion

            #region Close
            if (Pos_above.Length != 0)
            {
                if (GetClose(AboveLabel))
                {
                    if (SR <= Distance / 5)
                        this.closeAllLabel(AboveLabel);
                }
                else
                {
                    if (SR <= 0)
                        this.closeAllLabel(AboveLabel);
                }
            }
            if (Pos_below.Length != 0)
            {
                if (GetClose(BelowLabel))
                {
                    if (SR >= -Distance / 5)
                        this.closeAllLabel(BelowLabel);
                }
                else
                {
                    if (SR >= 0)
                        this.closeAllLabel(BelowLabel);
                }
            }
            //Risk
            if (this.Risk())
            {
                Print("There is a risk for the current symbol.");
                var poss = Pos_above.Length == 0 ? Pos_below : Pos_above;
                if (poss.Length >= 2)
                {
                    if (this.FirstPosition(poss).NetProfit + this.LastPosition(poss).NetProfit > 0)
                    {
                        this.ClosePosition(this.FirstPosition(poss));
                        this.ClosePosition(this.LastPosition(poss));
                    }
                }
            }
            #endregion

            #region Mark
            if (Pos_above.Length != 0)
                foreach (var p in Pos_above)
                {
                    if (!list_mark.Contains(p.Comment.Substring(15, 13)))
                        list_mark.Add(p.Comment.Substring(15, 13));
                }
            if (Pos_below.Length != 0)
                foreach (var p in Pos_below)
                {
                    if (!list_mark.Contains(p.Comment.Substring(15, 13)))
                        list_mark.Add(p.Comment.Substring(15, 13));
                }
            #endregion

            if (IsTrade)
            {
                #region Open
                #region Above
                if (OpenSignal() == "above")
                {
                    var _initvolume = Init_Volume;
                    if (Pos_above.Length != 0)
                    {
                        _initvolume = this.LastPosition(Pos_above).Volume * 2;
                    }
                    initSell.Volume = _symbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initSell.Label = AboveLabel;
                    initSell.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initSell.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initSell.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    initSell.Comment += currency_sub.Mark + "<";
                    initSell.Comment += "nul000" + "<";
                    initSell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initSell.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initSell.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initSell.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initSell);
                    AboveCross = false;
                }
                if (OpenSignal() == "above_br" && IsBreak)
                {
                    var _initvolume = Init_Volume;
                    if (Pos_above.Length != 0)
                    {
                        _initvolume = this.LastPosition(Pos_above).Volume;
                    }
                    initSell.Volume = _symbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initSell.Label = AboveLabel;
                    initSell.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initSell.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initSell.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    initSell.Comment += currency_sub.Mark + "<";
                    initSell.Comment += "br_" + string.Format("{0:000}", GetBreak(AboveLabel) + Distance) + "<";
                    initSell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initSell.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initSell.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initSell.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initSell);
                    //AboveCross = false;
                }
                #endregion
                #region Below
                if (OpenSignal() == "below")
                {
                    var _initvolume = Init_Volume;
                    if (Pos_below.Length != 0)
                    {
                        _initvolume = this.LastPosition(Pos_below).Volume * 2;
                    }
                    initBuy.Volume = _symbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initBuy.Label = BelowLabel;
                    initBuy.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initBuy.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initBuy.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    initBuy.Comment += currency_sub.Mark + "<";
                    initBuy.Comment += "nul000" + "<";
                    initBuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initBuy.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initBuy.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initBuy.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initBuy);
                    BelowCross = false;
                }
                if (OpenSignal() == "below_br" && IsBreak)
                {
                    var _initvolume = Init_Volume;
                    if (Pos_below.Length != 0)
                    {
                        _initvolume = this.LastPosition(Pos_below).Volume;
                    }
                    initBuy.Volume = _symbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initBuy.Label = BelowLabel;
                    initBuy.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initBuy.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initBuy.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    initBuy.Comment += currency_sub.Mark + "<";
                    initBuy.Comment += "br_" + string.Format("{0:000}", GetBreak(BelowLabel) + Distance) + "<";
                    initBuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initBuy.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initBuy.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initBuy.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initBuy);
                    //BelowCross = false;
                }
                #endregion
                #endregion
            }
        }

        private string OpenSignal()
        {
            #region Parameter
            string signal = null;
            Position[] Pos_above = this.GetPositions(AboveLabel);
            Position[] Pos_below = this.GetPositions(BelowLabel);
            var UR = currency.Result.LastValue;
            var UA = currency.Average.LastValue;
            var SR = currency_sub.Result.LastValue;
            var SA = currency_sub.Average.LastValue;
            var now = DateTime.UtcNow;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_above.Length != 0)
            {
                lastPosTime.Add(this.LastPosition(Pos_above).EntryTime.AddHours(_timer));
            }
            if (Pos_below.Length != 0)
            {
                lastPosTime.Add(this.LastPosition(Pos_below).EntryTime.AddHours(_timer));
            }
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-_timer) : lastPosTime.Max();
            #endregion

            if (DateTime.Compare(now, Pos_LastTime) < 0)
                return null;
            if (IsBreak)
            {
                if (SR >= GetBreak(AboveLabel))
                    return signal = "above_br";
                if (SR <= -GetBreak(BelowLabel))
                    return signal = "below_br";
            }
            var sig = currency_sub.SIG;
            if (sig == null)
            {
                return signal;
            }

            if (!list_mark.Contains(currency_sub.Mark))
            {
                if (sig == "above" && AboveCross)
                {
                    signal = "above";
                    if (Pos_above.Length != 0)
                    {
                        if (UR - CrossAgo() < Convert.ToDouble(this.LastPosition(Pos_above).Comment.Substring(0, 6)))
                            signal = null;
                    }
                }
                if (sig == "below" && BelowCross)
                {
                    signal = "below";
                    if (Pos_below.Length != 0)
                    {
                        if (UR + CrossAgo() > Convert.ToDouble(this.LastPosition(Pos_below).Comment.Substring(0, 6)))
                            signal = null;
                    }
                }
            }
            return signal;
        }

        private double CrossAgo()
        {
            return Distance;
        }

        private bool GetClose(string label)
        {
            var poss = this.GetPositions(label, _symbol);
            if (poss.Length != 0)
            {
                MarketSeries _marketseries = MarketData.GetSeries(_symbol, TimeFrame);
                int barsago = _marketseries.barsAgo(this.FirstPosition(poss));
                if (barsago > 24 || poss.Length > 1)
                    return true;
            }
            return false;
        }

        private double GetBreak(string label)
        {
            var poss = this.GetPositions(label);
            var sr = Math.Abs(currency_sub.Result.LastValue);
            double br = _break;
            if (br < sr)
                br = Math.Floor(sr);
            if (poss.Length != 0)
            {
                foreach (var p in poss)
                {
                    if (p.Comment.Length > 35)
                    {
                        if (p.Comment.Substring(29, 3) == "br_")
                        {
                            if (br < Convert.ToDouble(p.Comment.Substring(32, 3)))
                                br = Convert.ToDouble(p.Comment.Substring(32, 3));
                        }
                    }
                }
            }
            return br;
        }
    }
}
