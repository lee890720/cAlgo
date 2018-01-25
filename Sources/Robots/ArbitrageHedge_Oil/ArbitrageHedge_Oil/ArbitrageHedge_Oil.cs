﻿using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge_Oil : Robot
    {
        #region Parameter
        [Parameter("INIT_Volume", DefaultValue = 100, MinValue = 100)]
        public double Init_Volume { get; set; }

        [Parameter(DefaultValue = "XBRUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "XTIUSD")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 1)]
        public int _timer { get; set; }

        [Parameter(DefaultValue = 80)]
        public double _break { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = 25)]
        public double Distance { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Ratio { get; set; }

        [Parameter(DefaultValue = 0.35)]
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

            string _currencysymbol = (FirstSymbol.Substring(0, 3) == "USD" ? FirstSymbol.Substring(3, 3) : FirstSymbol.Substring(0, 3)) + (SecondSymbol.Substring(0, 3) == "USD" ? SecondSymbol.Substring(3, 3) : SecondSymbol.Substring(0, 3));
            Print("The currency of the current transaction is : " + _currencysymbol + ".");
            AboveLabel = "Above" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();
            BelowLabel = "Below" + "-" + _currencysymbol + "-" + MarketSeries.TimeFrame.ToString();

            _firstsymbol = MarketData.GetSymbol(FirstSymbol);
            _secondsymbol = MarketData.GetSymbol(SecondSymbol);

            #region OrderParams
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
                    initSellF.Volume = _firstsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_above.Length / 2)), RoundingMode.ToNearest);
                    initSellF.Label = AboveLabel;
                    initSellF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_above.Length + 1) + "-" + currency_sub.Mark + "-nul000";
                    this.executeOrder(initSellF);
                    initBuyS.Volume = _secondsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_above.Length / 2)), RoundingMode.ToNearest);
                    initBuyS.Label = AboveLabel;
                    initBuyS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_above.Length + 2) + "-" + currency_sub.Mark + "-nul000";
                    this.executeOrder(initBuyS);
                    AboveCross = false;
                }
                if (OpenSignal() == "above_br")
                {
                    initSellF.Volume = _firstsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_above.Length / 2)), RoundingMode.ToNearest);
                    initSellF.Label = AboveLabel;
                    initSellF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_above.Length + 1) + "-" + currency_sub.Mark + "-br_" + string.Format("{0:000}", (_break + GetBreak(AboveLabel)));
                    this.executeOrder(initSellF);
                    initBuyS.Volume = _secondsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_above.Length / 2)), RoundingMode.ToNearest);
                    initBuyS.Label = AboveLabel;
                    initBuyS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_above.Length + 2) + "-" + currency_sub.Mark + "-br_" + string.Format("{0:000}", (_break + GetBreak(AboveLabel)));
                    this.executeOrder(initBuyS);
                }
                #endregion
                #region Below
                if (OpenSignal() == "below")
                {
                    initBuyF.Volume = _firstsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_below.Length / 2)), RoundingMode.ToNearest);
                    initBuyF.Label = BelowLabel;
                    initBuyF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_below.Length + 1) + "-" + currency_sub.Mark + "-nul000";
                    this.executeOrder(initBuyF);
                    initSellS.Volume = _secondsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_below.Length / 2)), RoundingMode.ToNearest);
                    initSellS.Label = BelowLabel;
                    initSellS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_below.Length + 2) + "-" + currency_sub.Mark + "-nul000";
                    this.executeOrder(initSellS);
                    BelowCross = false;
                }
                if (OpenSignal() == "below_br")
                {
                    initBuyF.Volume = _firstsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_below.Length / 2)), RoundingMode.ToNearest);
                    initBuyF.Label = BelowLabel;
                    initBuyF.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_below.Length + 1) + "-" + currency_sub.Mark + "-br_" + string.Format("{0:000}", (_break + GetBreak(BelowLabel)));
                    this.executeOrder(initBuyF);
                    initSellS.Volume = _secondsymbol.NormalizeVolume(Init_Volume * Math.Pow(2, Math.Floor((double)Pos_below.Length / 2)), RoundingMode.ToNearest);
                    initSellS.Label = BelowLabel;
                    initSellS.Comment = string.Format("{0:000000}", Math.Round(UR)) + "-" + string.Format("{0:000}", CrossAgo()) + "-" + string.Format("{0:000}", Pos_below.Length + 2) + "-" + currency_sub.Mark + "-br_" + string.Format("{0:000}", (_break + GetBreak(BelowLabel)));
                    this.executeOrder(initSellS);
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
            var Pos_LastTime = lastPosTime.Count() == 0 ? DateTime.UtcNow.AddHours(-_timer) : lastPosTime.Max();
            #endregion

            if (DateTime.Compare(now, Pos_LastTime) < 0)
                return null;

            if (SR > _break + GetBreak(AboveLabel))
                return signal = "above_br";
            if (SR < -(_break + GetBreak(BelowLabel)))
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
            double br = 0;
            if (poss.Length != 0)
            {
                foreach (var p in poss)
                {
                    if (p.Comment.Substring(29, 3) == "br_")
                    {
                        br += Distance;
                    }
                }
            }
            return br;
        }
    }
}