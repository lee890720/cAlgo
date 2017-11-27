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

        [Parameter(DefaultValue = 2)]
        public int timer { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsTrade { get; set; }

        private Symbol eurchfSymbol;
        private string eurchfAbove;
        private string eurchfBelow;
        private OrderParams initBuy, initSell;
        private Highlight_USD_EURCHF usd_eurchf;
        private Highlight_Sub_EURCHF sub_eurchf;
        private bool AboveCross;
        private bool BelowCross;

        protected override void OnStart()
        {
            AboveCross = false;
            BelowCross = false;
            usd_eurchf = Indicators.GetIndicator<Highlight_USD_EURCHF>(Period, Distance);
            sub_eurchf = Indicators.GetIndicator<Highlight_Sub_EURCHF>(Period, Distance);
            eurchfSymbol = MarketData.GetSymbol("EURCHF");
            eurchfAbove = "Above" + eurchfSymbol.Code;
            eurchfBelow = "Below" + eurchfSymbol.Code;
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuy = new OrderParams(TradeType.Buy, eurchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSell = new OrderParams(TradeType.Sell, eurchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
            if (IsTrade)
            {
                #region Parameter
                var UR = usd_eurchf.Result.LastValue;
                var UA = usd_eurchf.Average.LastValue;
                var SR = sub_eurchf.Result.LastValue;
                var SA = sub_eurchf.Average.LastValue;

                List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
                List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
                //var comment = string.Format("{0:000000}", Math.Round(UR)) + Math.Round(SR).ToString();
                #endregion

                #region Open
                if (opensignal() == "above")
                {
                    initSell.Volume = Init_Volume * Math.Pow(2, Pos_eurchfabove.Count);
                    initSell.Label = eurchfAbove;
                    initSell.Comment = "above" + Pos_eurchfabove.Count + 1;
                    this.executeOrder(initSell);
                    AboveCross = false;
                }
                if (opensignal() == "below")
                {
                    initBuy.Volume = Init_Volume * Math.Pow(2, Pos_eurchfbelow.Count);
                    initBuy.Label = eurchfBelow;
                    initBuy.Comment = "below" + Pos_eurchfbelow.Count + 1;
                    this.executeOrder(initBuy);
                    BelowCross = false;
                }
                #endregion

                #region Close
                if (Pos_eurchfabove.Count != 0)
                    if (UR <= UA)
                        this.closeAllLabel(eurchfAbove);
                if (Pos_eurchfbelow.Count != 0)
                    if (UR >= UA)
                        this.closeAllLabel(eurchfBelow);
                #endregion

                #region Cross
                if (Pos_eurchfabove.Count == 0)
                    AboveCross = true;
                else
                {
                    if (SR > SA)
                        AboveCross = true;
                }
                if (Pos_eurchfbelow.Count == 0)
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
            var UR = usd_eurchf.Result.LastValue;
            var UA = usd_eurchf.Average.LastValue;
            var SR = sub_eurchf.Result.LastValue;
            var SA = sub_eurchf.Average.LastValue;
            var sig = sub_eurchf.SIG;
            if (sig == null)
            {
                return signal;
            }

            List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
            List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
            Pos_eurchfabove.Reverse();
            Pos_eurchfbelow.Reverse();

            var now = DateTime.UtcNow;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_eurchfabove.Count != 0)
                lastPosTime.Add(Pos_eurchfabove[0].EntryTime.AddHours(timer));
            if (Pos_eurchfbelow.Count != 0)
                lastPosTime.Add(Pos_eurchfbelow[0].EntryTime.AddHours(timer));
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
            var UR = usd_eurchf.Result.LastValue;
            var UA = usd_eurchf.Average.LastValue;
            var SR = sub_eurchf.Result.LastValue;
            var SA = sub_eurchf.Average.LastValue;

            List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
            List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
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
            ChartObjects.DrawText("info2", "\nEquity\t" + Math.Round(this.Account.Equity) + "\t\tProfit\t" + Math.Round(this.Account.UnrealizedNetProfit) + "\t\tMargin\t" + Math.Round(this.Account.Margin) + "\t\tLevel\t" + marginlevel + "%", StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("eurchf", "\n\nSub_EURCHF\t" + Math.Round(SR).ToString(), StaticPosition.TopLeft, Colors.White);
        }
    }
}
