using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge_chf_eur_gbp : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        [Parameter(DefaultValue = true)]
        public bool IsTrade { get; set; }

        private Symbol eurchfSymbol, gbpchfSymbol, gbpeurSymbol;
        private string eurchfAbove, gbpchfAbove, gbpeurAbove;
        private string eurchfBelow, gbpchfBelow, gbpeurBelow;
        private OrderParams initBuyeurchf, initBuygbpchf, initBuygbpeur, initSelleurchf, initSellgbpchf, initSellgbpeur;
        private USD_EURCHF usd_eurchf;
        private USD_GBPCHF usd_gbpchf;
        private USD_GBPEUR usd_gbpeur;
        protected override void OnStart()
        {
            eurchfSymbol = MarketData.GetSymbol("EURCHF");
            gbpchfSymbol = MarketData.GetSymbol("GBPCHF");
            gbpeurSymbol = MarketData.GetSymbol("EURGBP");
            eurchfAbove = "Above" + eurchfSymbol.Code;
            gbpchfAbove = "Above" + gbpchfSymbol.Code;
            gbpeurAbove = "Above" + gbpeurSymbol.Code;
            eurchfBelow = "Below" + eurchfSymbol.Code;
            gbpchfBelow = "Below" + gbpchfSymbol.Code;
            gbpeurBelow = "Below" + gbpeurSymbol.Code;
            usd_eurchf = Indicators.GetIndicator<USD_EURCHF>();
            usd_gbpchf = Indicators.GetIndicator<USD_GBPCHF>();
            usd_gbpeur = Indicators.GetIndicator<USD_GBPEUR>();
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuyeurchf = new OrderParams(TradeType.Buy, eurchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuygbpchf = new OrderParams(TradeType.Buy, gbpchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuygbpeur = new OrderParams(TradeType.Buy, gbpeurSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSelleurchf = new OrderParams(TradeType.Sell, eurchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellgbpchf = new OrderParams(TradeType.Sell, gbpchfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellgbpeur = new OrderParams(TradeType.Sell, gbpeurSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
            if (IsTrade)
            {
                var RS_eurchf = usd_eurchf.Result.LastValue;
                var AV_eurchf = usd_eurchf.Average.LastValue;
                var RS_gbpchf = usd_gbpchf.Result.LastValue;
                var AV_gbpchf = usd_gbpchf.Average.LastValue;
                var RS_gbpeur = usd_gbpeur.Result.LastValue;
                var AV_gbpeur = usd_gbpeur.Average.LastValue;
                List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
                List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
                List<Position> Pos_gbpchfabove = new List<Position>(this.GetPositions(gbpchfAbove));
                List<Position> Pos_gbpchfbelow = new List<Position>(this.GetPositions(gbpchfBelow));
                List<Position> Pos_gbpeurabove = new List<Position>(this.GetPositions(gbpeurAbove));
                List<Position> Pos_gbpeurbelow = new List<Position>(this.GetPositions(gbpeurBelow));
                var Sub_eurchf = string.Format("{0:000000}", Math.Round(RS_eurchf)) + Math.Round(RS_eurchf - AV_eurchf).ToString();
                var Sub_gbpchf = string.Format("{0:000000}", Math.Round(RS_gbpchf)) + Math.Round(RS_gbpchf - AV_gbpchf).ToString();
                var Sub_gbpeur = string.Format("{0:000000}", Math.Round(RS_gbpeur)) + Math.Round(RS_gbpeur - AV_gbpeur).ToString();
                if (opensignal() == "Above_EURCHF")
                {
                    initSelleurchf.Label = eurchfAbove;
                    initSelleurchf.Comment = Sub_eurchf;
                    this.executeOrder(initSelleurchf);
                }
                if (opensignal() == "Below_EURCHF")
                {
                    initBuyeurchf.Label = eurchfBelow;
                    initBuyeurchf.Comment = Sub_eurchf;
                    this.executeOrder(initBuyeurchf);
                }
                if (opensignal() == "Above_GBPCHF")
                {
                    initSellgbpchf.Label = gbpchfAbove;
                    initSellgbpchf.Comment = Sub_gbpchf;
                    this.executeOrder(initSellgbpchf);
                }
                if (opensignal() == "Below_GBPCHF")
                {
                    initBuygbpchf.Label = gbpchfBelow;
                    initBuygbpchf.Comment = Sub_gbpchf;
                    this.executeOrder(initBuygbpchf);
                }
                if (opensignal() == "Above_GBPEUR")
                {
                    initBuygbpeur.Label = gbpeurAbove;
                    initBuygbpeur.Comment = Sub_gbpeur;
                    this.executeOrder(initBuygbpeur);
                }
                if (opensignal() == "Below_GBPEUR")
                {
                    initSellgbpeur.Label = gbpeurBelow;
                    initSellgbpeur.Comment = Sub_gbpeur;
                    this.executeOrder(initSellgbpeur);
                }
                if (Pos_eurchfabove.Count != 0)
                    if (RS_eurchf <= AV_eurchf)
                        this.closeAllLabel(eurchfAbove);
                if (Pos_eurchfbelow.Count != 0)
                    if (RS_eurchf >= AV_eurchf)
                        this.closeAllLabel(eurchfBelow);
                if (Pos_gbpchfabove.Count != 0)
                    if (RS_gbpchf <= AV_gbpchf)
                        this.closeAllLabel(gbpchfAbove);
                if (Pos_gbpchfbelow.Count != 0)
                    if (RS_gbpchf >= AV_gbpchf)
                        this.closeAllLabel(gbpchfBelow);
                if (Pos_gbpeurabove.Count != 0)
                    if (RS_gbpeur <= AV_gbpeur)
                        this.closeAllLabel(gbpeurAbove);
                if (Pos_gbpeurbelow.Count != 0)
                    if (RS_gbpeur >= AV_gbpeur)
                        this.closeAllLabel(gbpeurBelow);
            }
        }
        private string opensignal()
        {
            #region Parameter
            string signal = null;
            string eurchfsignal = null;
            string gbpchfsignal = null;
            string gbpeursignal = null;
            int eurusd = 0;
            int gbpusd = 0;
            int usdchf = 0;
            double eurchf = 0;
            double gbpchf = 0;
            double gbpeur = 0;
            var RS_eurchf = usd_eurchf.Result.LastValue;
            var AV_eurchf = usd_eurchf.Average.LastValue;
            var RS_gbpchf = usd_gbpchf.Result.LastValue;
            var AV_gbpchf = usd_gbpchf.Average.LastValue;
            var RS_gbpeur = usd_gbpeur.Result.LastValue;
            var AV_gbpeur = usd_gbpeur.Average.LastValue;
            List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
            List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
            List<Position> Pos_gbpchfabove = new List<Position>(this.GetPositions(gbpchfAbove));
            List<Position> Pos_gbpchfbelow = new List<Position>(this.GetPositions(gbpchfBelow));
            List<Position> Pos_gbpeurabove = new List<Position>(this.GetPositions(gbpeurAbove));
            List<Position> Pos_gbpeurbelow = new List<Position>(this.GetPositions(gbpeurBelow));
            List<Position> Pos_all = new List<Position>();
            Pos_all.AddRange(Pos_eurchfabove);
            Pos_all.AddRange(Pos_eurchfbelow);
            Pos_all.AddRange(Pos_gbpchfabove);
            Pos_all.AddRange(Pos_gbpchfbelow);
            Pos_all.AddRange(Pos_gbpeurabove);
            Pos_all.AddRange(Pos_gbpeurbelow);
            Pos_eurchfabove.Reverse();
            Pos_eurchfbelow.Reverse();
            Pos_gbpchfabove.Reverse();
            Pos_gbpchfbelow.Reverse();
            Pos_gbpeurabove.Reverse();
            Pos_gbpeurbelow.Reverse();
            var now = DateTime.UtcNow;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_eurchfabove.Count != 0)
                lastPosTime.Add(Pos_eurchfabove[0].EntryTime.AddHours(1));
            if (Pos_eurchfbelow.Count != 0)
                lastPosTime.Add(Pos_eurchfbelow[0].EntryTime.AddHours(1));
            if (Pos_gbpchfabove.Count != 0)
                lastPosTime.Add(Pos_gbpchfabove[0].EntryTime.AddHours(1));
            if (Pos_gbpchfbelow.Count != 0)
                lastPosTime.Add(Pos_gbpchfbelow[0].EntryTime.AddHours(1));
            if (Pos_gbpeurabove.Count != 0)
                lastPosTime.Add(Pos_gbpeurabove[0].EntryTime.AddHours(1));
            if (Pos_gbpeurbelow.Count != 0)
                lastPosTime.Add(Pos_gbpeurbelow[0].EntryTime.AddHours(1));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-2) : lastPosTime.Max();
            var distance = Distance;
            double AV_eurchfabove = distance;
            double AV_eurchfbelow = -distance;
            double AV_gbpchfabove = distance;
            double AV_gbpchfbelow = -distance;
            double AV_gbpeurabove = distance;
            double AV_gbpeurbelow = -distance;
            double AV_above = distance;
            double AV_below = -distance;
            List<double> av = new List<double>();
            #endregion
            #region EURCHF
            if (Pos_eurchfabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_eurchfabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_eurchfabove = totalCom / Pos_eurchfabove.Count;
            }
            if (Pos_eurchfbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_eurchfbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_eurchfbelow = totalCom / Pos_eurchfbelow.Count;
            }
            #endregion
            #region GBPCHF
            if (Pos_gbpchfabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpchfabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_gbpchfabove = totalCom / Pos_gbpchfabove.Count;
            }
            if (Pos_gbpchfbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpchfbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_gbpchfbelow = totalCom / Pos_gbpchfbelow.Count;
            }
            #endregion
            #region GBPEUR
            if (Pos_gbpeurabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpeurabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_gbpeurabove = totalCom / Pos_gbpeurabove.Count;
            }
            if (Pos_gbpeurbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpeurbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_gbpeurbelow = totalCom / Pos_gbpeurbelow.Count;
            }
            #endregion
            av.AddRange(new double[] 
            {
                AV_eurchfabove,
                AV_eurchfbelow,
                AV_gbpchfabove,
                AV_gbpchfbelow,
                AV_gbpeurabove,
                AV_gbpeurbelow
            });
            AV_above = av.Max();
            AV_below = av.Min();
            int Open_A = 0;
            int Open_B = 0;
            if (RS_eurchf >= AV_eurchf)
                Open_A++;
            if (RS_eurchf < AV_eurchf)
                Open_B++;
            if (RS_gbpchf >= AV_gbpchf)
                Open_A++;
            if (RS_gbpchf < AV_gbpchf)
                Open_B++;
            if (RS_gbpeur >= AV_gbpeur)
                Open_A++;
            if (RS_gbpeur < AV_gbpeur)
                Open_B++;
            if (Open_A == 3)
                AV_below = -10000;
            if (Open_B == 3)
                AV_above = 10000;
            if (Open_A == 2 && Open_B == 1)
                AV_above = 10000;
            if (Open_A == 1 && Open_B == 2)
                AV_below = -10000;
            #region Signal
            if (DateTime.Compare(Pos_LastTime, now) < 0)
            {
                if (RS_eurchf > AV_eurchf + AV_above)
                    eurchfsignal = "Above_EURCHF";
                if (RS_eurchf < AV_eurchf + AV_below)
                    eurchfsignal = "Below_EURCHF";
                if (RS_gbpchf > AV_gbpchf + AV_above)
                    gbpchfsignal = "Above_GBPCHF";
                if (RS_gbpchf < AV_gbpchf + AV_below)
                    gbpchfsignal = "Below_GBPCHF";
                if (RS_gbpeur > AV_gbpeur + AV_above)
                    gbpeursignal = "Above_GBPEUR";
                if (RS_gbpeur < AV_gbpeur + AV_below)
                    gbpeursignal = "Below_GBPEUR";
            }
            if (eurchfsignal == "Above_EURCHF")
            {
                eurusd--;
                usdchf--;
            }
            if (eurchfsignal == "Below_EURCHF")
            {
                eurusd++;
                usdchf++;
            }
            if (gbpchfsignal == "Above_GBPCHF")
            {
                gbpusd--;
                usdchf--;
            }
            if (gbpchfsignal == "Below_GBPCHF")
            {
                gbpusd++;
                usdchf++;
            }
            if (gbpeursignal == "Above_GBPEUR")
            {
                gbpusd--;
                eurusd++;
            }
            if (gbpeursignal == "Below_GBPEUR")
            {
                gbpusd++;
                eurusd--;
            }
            if (eurusd == 0)
            {
                eurchfsignal = null;
                gbpeursignal = null;
            }
            if (gbpusd == 0)
            {
                gbpchfsignal = null;
                gbpeursignal = null;
            }
            if (usdchf == 0)
            {
                eurchfsignal = null;
                gbpchfsignal = null;
            }
            if (eurchfsignal != null)
            {
                eurchf = Math.Abs(RS_eurchf - AV_eurchf);
            }
            if (gbpchfsignal != null)
            {
                gbpchf = Math.Abs(RS_gbpchf - AV_gbpchf);
            }
            if (gbpeursignal != null)
            {
                gbpeur = Math.Abs(RS_gbpeur - AV_gbpeur);
            }
            List<double> abc = new List<double> 
            {
                eurchf,
                gbpchf,
                gbpeur
            };
            var abcmax = abc.Max();
            if (abcmax == eurchf)
                signal = eurchfsignal;
            if (abcmax == gbpchf)
                signal = gbpchfsignal;
            if (abcmax == gbpeur)
                signal = gbpeursignal;
            return signal;
            #endregion
        }
        private void chartdraw()
        {
            var RS_eurchf = usd_eurchf.Result.LastValue;
            var AV_eurchf = usd_eurchf.Average.LastValue;
            var RS_gbpchf = usd_gbpchf.Result.LastValue;
            var AV_gbpchf = usd_gbpchf.Average.LastValue;
            var RS_gbpeur = usd_gbpeur.Result.LastValue;
            var AV_gbpeur = usd_gbpeur.Average.LastValue;
            List<Position> Pos_eurchfabove = new List<Position>(this.GetPositions(eurchfAbove));
            List<Position> Pos_eurchfbelow = new List<Position>(this.GetPositions(eurchfBelow));
            List<Position> Pos_gbpchfabove = new List<Position>(this.GetPositions(gbpchfAbove));
            List<Position> Pos_gbpchfbelow = new List<Position>(this.GetPositions(gbpchfBelow));
            List<Position> Pos_gbpeurabove = new List<Position>(this.GetPositions(gbpeurAbove));
            List<Position> Pos_gbpeurbelow = new List<Position>(this.GetPositions(gbpeurBelow));
            Pos_eurchfabove.Reverse();
            Pos_eurchfbelow.Reverse();
            Pos_gbpchfabove.Reverse();
            Pos_gbpchfbelow.Reverse();
            Pos_gbpeurabove.Reverse();
            Pos_gbpeurbelow.Reverse();
            var Sub_eurchf = Math.Round(RS_eurchf - AV_eurchf).ToString();
            var Sub_gbpchf = Math.Round(RS_gbpchf - AV_gbpchf).ToString();
            var Sub_gbpeur = Math.Round(RS_gbpeur - AV_gbpeur).ToString();
            double Sub_eurchfabove = 0;
            double Sub_eurchfbelow = 0;
            double Sub_gbpchfabove = 0;
            double Sub_gbpchfbelow = 0;
            double Sub_gbpeurabove = 0;
            double Sub_gbpeurbelow = 0;
            int distance = Distance;
            double AV_eurchfabove = distance;
            double AV_eurchfbelow = -distance;
            double AV_gbpchfabove = distance;
            double AV_gbpchfbelow = -distance;
            double AV_gbpeurabove = distance;
            double AV_gbpeurbelow = -distance;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_eurchfabove.Count != 0)
                lastPosTime.Add(Pos_eurchfabove[0].EntryTime.AddHours(1));
            if (Pos_eurchfbelow.Count != 0)
                lastPosTime.Add(Pos_eurchfbelow[0].EntryTime.AddHours(1));
            if (Pos_gbpchfabove.Count != 0)
                lastPosTime.Add(Pos_gbpchfabove[0].EntryTime.AddHours(1));
            if (Pos_gbpchfbelow.Count != 0)
                lastPosTime.Add(Pos_gbpchfbelow[0].EntryTime.AddHours(1));
            if (Pos_gbpeurabove.Count != 0)
                lastPosTime.Add(Pos_gbpeurabove[0].EntryTime.AddHours(1));
            if (Pos_gbpeurbelow.Count != 0)
                lastPosTime.Add(Pos_gbpeurbelow[0].EntryTime.AddHours(1));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-2) : lastPosTime.Max();
            #region EURCHF
            if (Pos_eurchfabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_eurchfabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_eurchfabove = Math.Round(total / Pos_eurchfabove.Count - AV_eurchf);
                AV_eurchfabove = Math.Round(totalCom / Pos_eurchfabove.Count);
            }
            if (Pos_eurchfbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_eurchfbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_eurchfbelow = Math.Round(total / Pos_eurchfbelow.Count - AV_eurchf);
                AV_eurchfbelow = Math.Round(totalCom / Pos_eurchfbelow.Count);
            }
            #endregion
            #region GBPCHF
            if (Pos_gbpchfabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_gbpchfabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_gbpchfabove = Math.Round(total / Pos_gbpchfabove.Count - AV_gbpchf);
                AV_gbpchfabove = Math.Round(totalCom / Pos_gbpchfabove.Count);
            }
            if (Pos_gbpchfbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_gbpchfbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));

                }
                Sub_gbpchfbelow = Math.Round(total / Pos_gbpchfbelow.Count - AV_gbpchf);
                AV_gbpchfbelow = Math.Round(totalCom / Pos_gbpchfbelow.Count);
            }
            #endregion
            #region GBPEUR
            if (Pos_gbpeurabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_gbpeurabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_gbpeurabove = Math.Round(total / Pos_gbpeurabove.Count - AV_gbpeur);
                AV_gbpeurabove = Math.Round(totalCom / Pos_gbpeurabove.Count);
            }
            if (Pos_gbpeurbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_gbpeurbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_gbpeurbelow = Math.Round(total / Pos_gbpeurbelow.Count - AV_gbpeur);
                AV_gbpeurbelow = Math.Round(totalCom / Pos_gbpeurbelow.Count);
            }
            #endregion
            double marginlevel = 0;
            if (this.Account.MarginLevel.HasValue)
                marginlevel = Math.Round((double)this.Account.MarginLevel);
            ChartObjects.DrawText("info", this.Account.Number + "-" + Symbol.VolumeToQuantity(this.TotalLots()) + "\t\tEquity\t" + this.Account.Equity + "\t\tMargin\t" + this.Account.Margin + "\t\tLevel\t" + marginlevel + "%\t\tProfit\t" + Math.Round(this.Account.UnrealizedNetProfit, 2) + "\t" + Pos_LastTime, StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("eurchf", "\nSub_EURCHF\t" + Sub_eurchf.ToString() + "\tEURCHF_A\t" + Sub_eurchfabove.ToString() + "\t" + AV_eurchfabove.ToString() + "\t" + Pos_eurchfabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(eurchfAbove), 2) + "\tEURCHF_B\t" + Sub_eurchfbelow.ToString() + "\t" + AV_eurchfbelow.ToString() + "\t" + Pos_eurchfbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(eurchfBelow), 2), StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("gbpchf", "\n\nSub_GBPCHF\t" + Sub_gbpchf.ToString() + "\tGBPCHF_A\t" + Sub_gbpchfabove.ToString() + "\t" + AV_gbpchfabove.ToString() + "\t" + Pos_gbpchfabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(gbpchfAbove), 2) + "\tGBPCHF_B\t" + Sub_gbpchfbelow.ToString() + "\t" + AV_gbpchfbelow.ToString() + "\t" + Pos_gbpchfbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(gbpchfBelow), 2), StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("gbpeur", "\n\n\nSub_GBPEUR\t" + Sub_gbpeur.ToString() + "\tGBPEUR_A\t" + Sub_gbpeurabove.ToString() + "\t" + AV_gbpeurabove.ToString() + "\t" + Pos_gbpeurabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(gbpeurAbove), 2) + "\tGBPEUR_B\t" + Sub_gbpeurbelow.ToString() + "\t" + AV_gbpeurbelow.ToString() + "\t" + Pos_gbpeurbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(gbpeurBelow), 2), StaticPosition.TopLeft, Colors.White);
        }
    }
}
