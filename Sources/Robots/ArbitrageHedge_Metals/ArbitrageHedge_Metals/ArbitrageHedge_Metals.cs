using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge_Metals : Robot
    {
        #region Parameter
        [Parameter("INIT_Volume", DefaultValue = 1, MinValue = 1)]
        public double Init_Volume { get; set; }

        [Parameter(DefaultValue = "XAUUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "XAGUSD")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 1)]
        public int _timer { get; set; }

        [Parameter(DefaultValue = 100)]
        public double _break { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 25)]
        public double Distance { get; set; }

        [Parameter(DefaultValue = 0.0077)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 0.132)]
        public double Magnify { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsTrade { get; set; }

        private Currency_Highlight currency;
        private Currency_Sub_Highlight currency_sub;
        private bool AboveCross;
        private bool BelowCross;
        private string AboveLabel, BelowLabel;
        private Symbol _firstsymbol, _secondsymbol;
        private List<string> list_mark = new List<string>();
        private OrderParams initBuyF, initBuyS, initSellF, initSellS;
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

            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);

            #region OrderParams
            if (Symbol.Code == FirstSymbol)
            {
                initBuyF = new OrderParams(TradeType.Buy, _firstsymbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSellF = new OrderParams(TradeType.Sell, _firstsymbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initBuyS = new OrderParams(TradeType.Buy, _secondsymbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
                {
                                    });
                initSellS = new OrderParams(TradeType.Sell, _secondsymbol, Init_Volume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
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
                    var poss = this.GetPositions(AboveLabel, _firstsymbol);
                    if (poss.Length != 0)
                    {
                        _initvolume = this.LastPosition(poss).Volume * 2;
                    }
                    initSellF.Volume = _firstsymbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initSellF.Label = AboveLabel;
                    initSellF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initSellF.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initSellF.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    initSellF.Comment += currency_sub.Mark + "<";
                    initSellF.Comment += "nul000" + "<";
                    initSellF.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initSellF.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initSellF.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initSellF.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initSellF);
                    if (LastResult.IsSuccessful)
                    {
                        initBuyS.Volume = _secondsymbol.NormalizeVolume(_initvolume * Math.Round(1 / Ratio), RoundingMode.ToNearest);
                        initBuyS.Label = AboveLabel;
                        initBuyS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                        initBuyS.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                        initBuyS.Comment += string.Format("{0:000}", Pos_above.Length + 2) + "<";
                        initBuyS.Comment += currency_sub.Mark + "<";
                        initBuyS.Comment += "nul000" + "<";
                        initBuyS.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                        initBuyS.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                        initBuyS.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                        initBuyS.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                        this.executeOrder(initBuyS);
                        AboveCross = false;
                    }
                }
                if (OpenSignal() == "above_br")
                {
                    var _initvolume = Init_Volume;
                    var poss = this.GetPositions(AboveLabel, _firstsymbol);
                    if (poss.Length != 0)
                    {
                        _initvolume = this.LastPosition(poss).Volume;
                    }
                    initSellF.Volume = _firstsymbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initSellF.Label = AboveLabel;
                    initSellF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initSellF.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initSellF.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    initSellF.Comment += currency_sub.Mark + "<";
                    initSellF.Comment += "br_" + string.Format("{0:000}", GetBreak(AboveLabel) + Distance) + "<";
                    initSellF.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initSellF.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initSellF.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initSellF.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initSellF);
                    if (LastResult.IsSuccessful)
                    {
                        initBuyS.Volume = _secondsymbol.NormalizeVolume(_initvolume * Math.Round(1 / Ratio), RoundingMode.ToNearest);
                        initBuyS.Label = AboveLabel;
                        initBuyS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                        initBuyS.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                        initBuyS.Comment += string.Format("{0:000}", Pos_above.Length + 2) + "<";
                        initBuyS.Comment += currency_sub.Mark + "<";
                        initBuyS.Comment += "br_" + string.Format("{0:000}", GetBreak(AboveLabel) + Distance) + "<";
                        initBuyS.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                        initBuyS.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                        initBuyS.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                        initBuyS.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                        this.executeOrder(initBuyS);
                    }
                }
                #endregion
                #region Below
                if (OpenSignal() == "below")
                {
                    var _initvolume = Init_Volume;
                    var poss = this.GetPositions(BelowLabel, _firstsymbol);
                    if (poss.Length != 0)
                    {
                        _initvolume = this.LastPosition(poss).Volume * 2;
                    }
                    initBuyF.Volume = _firstsymbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initBuyF.Label = BelowLabel;
                    initBuyF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initBuyF.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initBuyF.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    initBuyF.Comment += currency_sub.Mark + "<";
                    initBuyF.Comment += "nul000" + "<";
                    initBuyF.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initBuyF.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initBuyF.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initBuyF.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initBuyF);
                    if (LastResult.IsSuccessful)
                    {
                        initSellS.Volume = _secondsymbol.NormalizeVolume(_initvolume * Math.Round(1 / Ratio), RoundingMode.ToNearest);
                        initSellS.Label = BelowLabel;
                        initSellS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                        initSellS.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                        initSellS.Comment += string.Format("{0:000}", Pos_below.Length + 2) + "<";
                        initSellS.Comment += currency_sub.Mark + "<";
                        initSellS.Comment += "nul000" + "<";
                        initSellS.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                        initSellS.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                        initSellS.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                        initSellS.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                        this.executeOrder(initSellS);
                        BelowCross = false;
                    }
                }
                if (OpenSignal() == "below_br")
                {
                    var _initvolume = Init_Volume;
                    var poss = this.GetPositions(BelowLabel, _firstsymbol);
                    if (poss.Length != 0)
                    {
                        _initvolume = this.LastPosition(poss).Volume;
                    }
                    initBuyF.Volume = _firstsymbol.NormalizeVolume(_initvolume, RoundingMode.ToNearest);
                    initBuyF.Label = BelowLabel;
                    initBuyF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                    initBuyF.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                    initBuyF.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    initBuyF.Comment += currency_sub.Mark + "<";
                    initBuyF.Comment += "br_" + string.Format("{0:000}", GetBreak(BelowLabel) + Distance) + "<";
                    initBuyF.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    initBuyF.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                    initBuyF.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                    initBuyF.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                    this.executeOrder(initBuyF);
                    if (LastResult.IsSuccessful)
                    {
                        initSellS.Volume = _secondsymbol.NormalizeVolume(_initvolume * Math.Round(1 / Ratio), RoundingMode.ToNearest);
                        initSellS.Label = BelowLabel;
                        initSellS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "<";
                        initSellS.Comment += string.Format("{0:000}", CrossAgo()) + "<";
                        initSellS.Comment += string.Format("{0:000}", Pos_below.Length + 2) + "<";
                        initSellS.Comment += currency_sub.Mark + "<";
                        initSellS.Comment += "br_" + string.Format("{0:000}", GetBreak(BelowLabel) + Distance) + "<";
                        initSellS.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                        initSellS.Comment += "D_" + string.Format("{0:000}", Distance) + "<";
                        initSellS.Comment += "R_" + Ratio.ToString("0.0000").Substring(0, 6) + "<";
                        initSellS.Comment += "M_" + Magnify.ToString("0.0000").Substring(0, 6) + "<";
                        this.executeOrder(initSellS);
                    }
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

            if (SR >= GetBreak(AboveLabel))
                return signal = "above_br";
            if (SR <= -GetBreak(BelowLabel))
                return signal = "below_br";

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
                        if (UR + CrossAgo() > Convert.ToDouble(this.LastPosition(Pos_above).Comment.Substring(0, 6)))
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
            var poss = this.GetPositions(label, _secondsymbol);
            if (poss.Length != 0)
            {
                MarketSeries _marketseries = MarketData.GetSeries(_secondsymbol, TimeFrame);
                int barsago = _marketseries.barsAgo(this.FirstPosition(poss));
                if (barsago > 24 || poss.Length > 1)
                    return true;
            }
            return false;
        }

        private double GetBreak(string label)
        {
            var poss = this.GetPositions(label, _secondsymbol);
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
