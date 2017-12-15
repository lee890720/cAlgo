using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge_EURCHF_H : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

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
        private EURCHF_Highlight eurchf;
        private EURCHF_Sub_Highlight eurchf_sub;
        private bool AboveCross;
        private bool BelowCross;

        protected override void OnStart()
        {
            AboveCross = false;
            BelowCross = false;
            eurchf = Indicators.GetIndicator<EURCHF_Highlight>(Period, Distance);
            eurchf_sub = Indicators.GetIndicator<EURCHF_Sub_Highlight>(Period, Distance);
            symbol = MarketData.GetSymbol("EURCHF");
            AboveLabel = "Above" + symbol.Code;
            BelowLabel = "Below" + symbol.Code;
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuy = new OrderParams(TradeType.Buy, symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSell = new OrderParams(TradeType.Sell, symbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
            if (IsTrade)
            {
                #region Parameter
                var UR = eurchf.Result.LastValue;
                var UA = eurchf.Average.LastValue;
                var SR = eurchf_sub.Result.LastValue;
                var SA = eurchf_sub.Average.LastValue;

                List<Position> Pos_above = new List<Position>(this.GetPositions(AboveLabel));
                List<Position> Pos_below = new List<Position>(this.GetPositions(BelowLabel));
                //var comment = string.Format("{0:000000}", Math.Round(UR)) + Math.Round(SR).ToString();
                #endregion

                #region Open
                if (opensignal() == "above")
                {
                    initSell.Volume = Init_Volume * Math.Pow(2, Pos_above.Count);
                    initSell.Label = AboveLabel;
                    initSell.Comment = "above" + Pos_above.Count + 1;
                    this.executeOrder(initSell);
                    AboveCross = false;
                }
                if (opensignal() == "below")
                {
                    initBuy.Volume = Init_Volume * Math.Pow(2, Pos_below.Count);
                    initBuy.Label = BelowLabel;
                    initBuy.Comment = "below" + Pos_below.Count + 1;
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

        private string opensignal()
        {
            #region Parameter
            string signal = null;
            var UR = eurchf.Result.LastValue;
            var UA = eurchf.Average.LastValue;
            var SR = eurchf_sub.Result.LastValue;
            var SA = eurchf_sub.Average.LastValue;
            var sig = eurchf_sub.SIG;
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

                }
                if (sig == "below" && BelowCross)
                {
                    signal = "below";
                }
            }
            return signal;
        }

        private void chartdraw()
        {
            #region Parameter
            var UR = eurchf.Result.LastValue;
            var UA = eurchf.Average.LastValue;
            var SR = eurchf_sub.Result.LastValue;
            var SA = eurchf_sub.Average.LastValue;

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
            ChartObjects.DrawText("info1", this.Account.Number + " - " + Symbol.VolumeToQuantity(this.TotalLots()) + " - " + Pos_LastTime + "    " + opensignal(), StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("info2", "\nEquity\t" + this.Account.Equity + "\t\tProfit\t" + Math.Round(this.Account.UnrealizedNetProfit) + "\t\tMargin\t" + Math.Round(this.Account.Margin) + "\t\tLevel\t" + marginlevel + "%", StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("eurchf", "\n\nSub_EURCHF\t" + Math.Round(SR).ToString(), StaticPosition.TopLeft, Colors.White);
        }
    }
}
