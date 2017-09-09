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
    public class ArbitrageHedge_aud_cad_jpy : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        [Parameter(DefaultValue = true)]
        public bool IsTrade { get; set; }

        private Symbol cadaudSymbol, jpyaudSymbol, jpycadSymbol;
        private string cadaudAbove, jpyaudAbove, jpycadAbove;
        private string cadaudBelow, jpyaudBelow, jpycadBelow;
        private OrderParams initBuycadaud, initBuyjpyaud, initBuyjpycad, initSellcadaud, initSelljpyaud, initSelljpycad;
        private USD_CADAUD usd_cadaud;
        private USD_JPYAUD usd_jpyaud;
        private USD_JPYCAD usd_jpycad;
        protected override void OnStart()
        {
            cadaudSymbol = MarketData.GetSymbol("AUDCAD");
            jpyaudSymbol = MarketData.GetSymbol("AUDJPY");
            jpycadSymbol = MarketData.GetSymbol("CADJPY");
            cadaudAbove = "Above" + cadaudSymbol.Code;
            jpyaudAbove = "Above" + jpyaudSymbol.Code;
            jpycadAbove = "Above" + jpycadSymbol.Code;
            cadaudBelow = "Below" + cadaudSymbol.Code;
            jpyaudBelow = "Below" + jpyaudSymbol.Code;
            jpycadBelow = "Below" + jpycadSymbol.Code;
            usd_cadaud = Indicators.GetIndicator<USD_CADAUD>();
            usd_jpyaud = Indicators.GetIndicator<USD_JPYAUD>();
            usd_jpycad = Indicators.GetIndicator<USD_JPYCAD>();
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuycadaud = new OrderParams(TradeType.Buy, cadaudSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuyjpyaud = new OrderParams(TradeType.Buy, jpyaudSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuyjpycad = new OrderParams(TradeType.Buy, jpycadSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellcadaud = new OrderParams(TradeType.Sell, cadaudSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSelljpyaud = new OrderParams(TradeType.Sell, jpyaudSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSelljpycad = new OrderParams(TradeType.Sell, jpycadSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
            if (IsTrade)
            {
                var RS_cadaud = usd_cadaud.Result.LastValue;
                var AV_cadaud = usd_cadaud.Average.LastValue;
                var RS_jpyaud = usd_jpyaud.Result.LastValue;
                var AV_jpyaud = usd_jpyaud.Average.LastValue;
                var RS_jpycad = usd_jpycad.Result.LastValue;
                var AV_jpycad = usd_jpycad.Average.LastValue;
                List<Position> Pos_cadaudabove = new List<Position>(this.GetPositions(cadaudAbove));
                List<Position> Pos_cadaudbelow = new List<Position>(this.GetPositions(cadaudBelow));
                List<Position> Pos_jpyaudabove = new List<Position>(this.GetPositions(jpyaudAbove));
                List<Position> Pos_jpyaudbelow = new List<Position>(this.GetPositions(jpyaudBelow));
                List<Position> Pos_jpycadabove = new List<Position>(this.GetPositions(jpycadAbove));
                List<Position> Pos_jpycadbelow = new List<Position>(this.GetPositions(jpycadBelow));
                var Sub_cadaud = string.Format("{0:000000}", Math.Round(RS_cadaud)) + Math.Round(RS_cadaud - AV_cadaud).ToString();
                var Sub_jpyaud = string.Format("{0:000000}", Math.Round(RS_jpyaud)) + Math.Round(RS_jpyaud - AV_jpyaud).ToString();
                var Sub_jpycad = string.Format("{0:000000}", Math.Round(RS_jpycad)) + Math.Round(RS_jpycad - AV_jpycad).ToString();
                if (opensignal() == "Above_CADAUD")
                {
                    initBuycadaud.Label = cadaudAbove;
                    initBuycadaud.Comment = Sub_cadaud;
                    this.executeOrder(initBuycadaud);
                }
                if (opensignal() == "Below_CADAUD")
                {
                    initSellcadaud.Label = cadaudBelow;
                    initSellcadaud.Comment = Sub_cadaud;
                    this.executeOrder(initSellcadaud);
                }
                if (opensignal() == "Above_JPYAUD")
                {
                    initBuyjpyaud.Label = jpyaudAbove;
                    initBuyjpyaud.Comment = Sub_jpyaud;
                    this.executeOrder(initBuyjpyaud);
                }
                if (opensignal() == "Below_JPYAUD")
                {
                    initSelljpyaud.Label = jpyaudBelow;
                    initSelljpyaud.Comment = Sub_jpyaud;
                    this.executeOrder(initSelljpyaud);
                }
                if (opensignal() == "Above_JPYCAD")
                {
                    initBuyjpycad.Label = jpycadAbove;
                    initBuyjpycad.Comment = Sub_jpycad;
                    this.executeOrder(initBuyjpycad);
                }
                if (opensignal() == "Below_JPYCAD")
                {
                    initSelljpycad.Label = jpycadBelow;
                    initSelljpycad.Comment = Sub_jpycad;
                    this.executeOrder(initSelljpycad);
                }
                if (Pos_cadaudabove.Count != 0)
                    if (RS_cadaud <= AV_cadaud)
                        this.closeAllLabel(cadaudAbove);
                if (Pos_cadaudbelow.Count != 0)
                    if (RS_cadaud >= AV_cadaud)
                        this.closeAllLabel(cadaudBelow);
                if (Pos_jpyaudabove.Count != 0)
                    if (RS_jpyaud <= AV_jpyaud)
                        this.closeAllLabel(jpyaudAbove);
                if (Pos_jpyaudbelow.Count != 0)
                    if (RS_jpyaud >= AV_jpyaud)
                        this.closeAllLabel(jpyaudBelow);
                if (Pos_jpycadabove.Count != 0)
                    if (RS_jpycad <= AV_jpycad)
                        this.closeAllLabel(jpycadAbove);
                if (Pos_jpycadbelow.Count != 0)
                    if (RS_jpycad >= AV_jpycad)
                        this.closeAllLabel(jpycadBelow);
            }
        }
        private string opensignal()
        {
            #region Parameter
            string signal = null;
            string cadaudsignal = null;
            string jpyaudsignal = null;
            string jpycadsignal = null;
            int usdcad = 0;
            int usdjpy = 0;
            int audusd = 0;
            double cadaud = 0;
            double jpyaud = 0;
            double jpycad = 0;
            var RS_cadaud = usd_cadaud.Result.LastValue;
            var AV_cadaud = usd_cadaud.Average.LastValue;
            var RS_jpyaud = usd_jpyaud.Result.LastValue;
            var AV_jpyaud = usd_jpyaud.Average.LastValue;
            var RS_jpycad = usd_jpycad.Result.LastValue;
            var AV_jpycad = usd_jpycad.Average.LastValue;
            List<Position> Pos_cadaudabove = new List<Position>(this.GetPositions(cadaudAbove));
            List<Position> Pos_cadaudbelow = new List<Position>(this.GetPositions(cadaudBelow));
            List<Position> Pos_jpyaudabove = new List<Position>(this.GetPositions(jpyaudAbove));
            List<Position> Pos_jpyaudbelow = new List<Position>(this.GetPositions(jpyaudBelow));
            List<Position> Pos_jpycadabove = new List<Position>(this.GetPositions(jpycadAbove));
            List<Position> Pos_jpycadbelow = new List<Position>(this.GetPositions(jpycadBelow));
            List<Position> Pos_all = new List<Position>();
            Pos_all.AddRange(Pos_cadaudabove);
            Pos_all.AddRange(Pos_cadaudbelow);
            Pos_all.AddRange(Pos_jpyaudabove);
            Pos_all.AddRange(Pos_jpyaudbelow);
            Pos_all.AddRange(Pos_jpycadabove);
            Pos_all.AddRange(Pos_jpycadbelow);
            Pos_cadaudabove.Reverse();
            Pos_cadaudbelow.Reverse();
            Pos_jpyaudabove.Reverse();
            Pos_jpyaudbelow.Reverse();
            Pos_jpycadabove.Reverse();
            Pos_jpycadbelow.Reverse();
            var now = DateTime.UtcNow;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_cadaudabove.Count != 0)
                lastPosTime.Add(Pos_cadaudabove[0].EntryTime.AddHours(1));
            if (Pos_cadaudbelow.Count != 0)
                lastPosTime.Add(Pos_cadaudbelow[0].EntryTime.AddHours(1));
            if (Pos_jpyaudabove.Count != 0)
                lastPosTime.Add(Pos_jpyaudabove[0].EntryTime.AddHours(1));
            if (Pos_jpyaudbelow.Count != 0)
                lastPosTime.Add(Pos_jpyaudbelow[0].EntryTime.AddHours(1));
            if (Pos_jpycadabove.Count != 0)
                lastPosTime.Add(Pos_jpycadabove[0].EntryTime.AddHours(1));
            if (Pos_jpycadbelow.Count != 0)
                lastPosTime.Add(Pos_jpycadbelow[0].EntryTime.AddHours(1));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-2) : lastPosTime.Max();
            var distance = Distance;
            double AV_cadaudabove = distance;
            double AV_cadaudbelow = -distance;
            double AV_jpyaudabove = distance;
            double AV_jpyaudbelow = -distance;
            double AV_jpycadabove = distance;
            double AV_jpycadbelow = -distance;
            double AV_above = distance;
            double AV_below = -distance;
            List<double> av = new List<double>();
            #endregion
            #region CADAUD
            if (Pos_cadaudabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_cadaudabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_cadaudabove = totalCom / Pos_cadaudabove.Count;
            }
            if (Pos_cadaudbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_cadaudbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_cadaudbelow = totalCom / Pos_cadaudbelow.Count;
            }
            #endregion
            #region JPYAUD
            if (Pos_jpyaudabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_jpyaudabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_jpyaudabove = totalCom / Pos_jpyaudabove.Count;
            }
            if (Pos_jpyaudbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_jpyaudbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_jpyaudbelow = totalCom / Pos_jpyaudbelow.Count;
            }
            #endregion
            #region JPYCAD
            if (Pos_jpycadabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_jpycadabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_jpycadabove = totalCom / Pos_jpycadabove.Count;
            }
            if (Pos_jpycadbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_jpycadbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                AV_jpycadbelow = totalCom / Pos_jpycadbelow.Count;
            }
            #endregion
            av.AddRange(new double[] 
            {
                AV_cadaudabove,
                AV_cadaudbelow,
                AV_jpyaudabove,
                AV_jpyaudbelow,
                AV_jpycadabove,
                AV_jpycadbelow
            });
            AV_above = av.Max();
            AV_below = av.Min();
            int Open_A = 0;
            int Open_B = 0;
            if (RS_cadaud >= AV_cadaud)
                Open_A++;
            if (RS_cadaud < AV_cadaud)
                Open_B++;
            if (RS_jpyaud >= AV_jpyaud)
                Open_A++;
            if (RS_jpyaud < AV_jpyaud)
                Open_B++;
            if (RS_jpycad >= AV_jpycad)
                Open_A++;
            if (RS_jpycad < AV_jpycad)
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
                if (RS_cadaud > AV_cadaud + AV_above)
                    cadaudsignal = "Above_CADAUD";
                if (RS_cadaud < AV_cadaud + AV_below)
                    cadaudsignal = "Below_CADAUD";
                if (RS_jpyaud > AV_jpyaud + AV_above)
                    jpyaudsignal = "Above_JPYAUD";
                if (RS_jpyaud < AV_jpyaud + AV_below)
                    jpyaudsignal = "Below_JPYAUD";
                if (RS_jpycad > AV_jpycad + AV_above)
                    jpycadsignal = "Above_JPYCAD";
                if (RS_jpycad < AV_jpycad + AV_below)
                    jpycadsignal = "Below_JPYCAD";
            }
            if (cadaudsignal == "Above_CADAUD")
            {
                usdcad++;
                audusd++;
            }
            if (cadaudsignal == "Below_CADAUD")
            {
                usdcad--;
                audusd--;
            }
            if (jpyaudsignal == "Above_JPYAUD")
            {
                usdjpy++;
                audusd++;
            }
            if (jpyaudsignal == "Below_JPYAUD")
            {
                usdjpy--;
                audusd--;
            }
            if (jpycadsignal == "Above_JPYCAD")
            {
                usdjpy++;
                usdcad--;
            }
            if (jpycadsignal == "Below_JPYCAD")
            {
                usdjpy--;
                usdcad++;
            }
            if (usdcad == 0)
            {
                cadaudsignal = null;
                jpycadsignal = null;
            }
            if (usdjpy == 0)
            {
                jpyaudsignal = null;
                jpycadsignal = null;
            }
            if (audusd == 0)
            {
                cadaudsignal = null;
                jpyaudsignal = null;
            }
            if (cadaudsignal != null)
            {
                cadaud = Math.Abs(RS_cadaud - AV_cadaud);
            }
            if (jpyaudsignal != null)
            {
                jpyaud = Math.Abs(RS_jpyaud - AV_jpyaud);
            }
            if (jpycadsignal != null)
            {
                jpycad = Math.Abs(RS_jpycad - AV_jpycad);
            }
            List<double> abc = new List<double> 
            {
                cadaud,
                jpyaud,
                jpycad
            };
            var abcmax = abc.Max();
            if (abcmax == cadaud)
                signal = cadaudsignal;
            if (abcmax == jpyaud)
                signal = jpyaudsignal;
            if (abcmax == jpycad)
                signal = jpycadsignal;
            return signal;
            #endregion
        }
        private void chartdraw()
        {
            var RS_cadaud = usd_cadaud.Result.LastValue;
            var AV_cadaud = usd_cadaud.Average.LastValue;
            var RS_jpyaud = usd_jpyaud.Result.LastValue;
            var AV_jpyaud = usd_jpyaud.Average.LastValue;
            var RS_jpycad = usd_jpycad.Result.LastValue;
            var AV_jpycad = usd_jpycad.Average.LastValue;
            List<Position> Pos_cadaudabove = new List<Position>(this.GetPositions(cadaudAbove));
            List<Position> Pos_cadaudbelow = new List<Position>(this.GetPositions(cadaudBelow));
            List<Position> Pos_jpyaudabove = new List<Position>(this.GetPositions(jpyaudAbove));
            List<Position> Pos_jpyaudbelow = new List<Position>(this.GetPositions(jpyaudBelow));
            List<Position> Pos_jpycadabove = new List<Position>(this.GetPositions(jpycadAbove));
            List<Position> Pos_jpycadbelow = new List<Position>(this.GetPositions(jpycadBelow));
            Pos_cadaudabove.Reverse();
            Pos_cadaudbelow.Reverse();
            Pos_jpyaudabove.Reverse();
            Pos_jpyaudbelow.Reverse();
            Pos_jpycadabove.Reverse();
            Pos_jpycadbelow.Reverse();
            var Sub_cadaud = Math.Round(RS_cadaud - AV_cadaud).ToString();
            var Sub_jpyaud = Math.Round(RS_jpyaud - AV_jpyaud).ToString();
            var Sub_jpycad = Math.Round(RS_jpycad - AV_jpycad).ToString();
            double Sub_cadaudabove = 0;
            double Sub_cadaudbelow = 0;
            double Sub_jpyaudabove = 0;
            double Sub_jpyaudbelow = 0;
            double Sub_jpycadabove = 0;
            double Sub_jpycadbelow = 0;
            int distance = Distance;
            double AV_cadaudabove = distance;
            double AV_cadaudbelow = -distance;
            double AV_jpyaudabove = distance;
            double AV_jpyaudbelow = -distance;
            double AV_jpycadabove = distance;
            double AV_jpycadbelow = -distance;
            List<DateTime> lastPosTime = new List<DateTime>();
            if (Pos_cadaudabove.Count != 0)
                lastPosTime.Add(Pos_cadaudabove[0].EntryTime.AddHours(1));
            if (Pos_cadaudbelow.Count != 0)
                lastPosTime.Add(Pos_cadaudbelow[0].EntryTime.AddHours(1));
            if (Pos_jpyaudabove.Count != 0)
                lastPosTime.Add(Pos_jpyaudabove[0].EntryTime.AddHours(1));
            if (Pos_jpyaudbelow.Count != 0)
                lastPosTime.Add(Pos_jpyaudbelow[0].EntryTime.AddHours(1));
            if (Pos_jpycadabove.Count != 0)
                lastPosTime.Add(Pos_jpycadabove[0].EntryTime.AddHours(1));
            if (Pos_jpycadbelow.Count != 0)
                lastPosTime.Add(Pos_jpycadbelow[0].EntryTime.AddHours(1));
            var Pos_LastTime = lastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-2) : lastPosTime.Max();

            #region CADAUD
            if (Pos_cadaudabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_cadaudabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_cadaudabove = Math.Round(total / Pos_cadaudabove.Count - AV_cadaud);
                AV_cadaudabove = Math.Round(totalCom / Pos_cadaudabove.Count);
            }
            if (Pos_cadaudbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_cadaudbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_cadaudbelow = Math.Round(total / Pos_cadaudbelow.Count - AV_cadaud);
                AV_cadaudbelow = Math.Round(totalCom / Pos_cadaudbelow.Count);
            }
            #endregion
            #region JPYAUD
            if (Pos_jpyaudabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_jpyaudabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_jpyaudabove = Math.Round(total / Pos_jpyaudabove.Count - AV_jpyaud);
                AV_jpyaudabove = Math.Round(totalCom / Pos_jpyaudabove.Count);
            }
            if (Pos_jpyaudbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_jpyaudbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));

                }
                Sub_jpyaudbelow = Math.Round(total / Pos_jpyaudbelow.Count - AV_jpyaud);
                AV_jpyaudbelow = Math.Round(totalCom / Pos_jpyaudbelow.Count);
            }
            #endregion
            #region JPYCAD
            if (Pos_jpycadabove.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_jpycadabove)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_jpycadabove = Math.Round(total / Pos_jpycadabove.Count - AV_jpycad);
                AV_jpycadabove = Math.Round(totalCom / Pos_jpycadabove.Count);
            }
            if (Pos_jpycadbelow.Count != 0)
            {
                double total = 0;
                double totalCom = 0;
                foreach (var pos in Pos_jpycadbelow)
                {
                    total += Convert.ToDouble(pos.Comment.Substring(0, 6));
                    totalCom += Convert.ToDouble(pos.Comment.Substring(6, pos.Comment.Length - 6));
                }
                Sub_jpycadbelow = Math.Round(total / Pos_jpycadbelow.Count - AV_jpycad);
                AV_jpycadbelow = Math.Round(totalCom / Pos_jpycadbelow.Count);
            }
            #endregion
            double marginlevel = 0;
            if (this.Account.MarginLevel.HasValue)
                marginlevel = Math.Round((double)this.Account.MarginLevel);
            ChartObjects.DrawText("info", this.Account.Number + "-" + Symbol.VolumeToQuantity(this.TotalLots()) + "\t\tEquity\t" + this.Account.Equity + "\t\tMargin\t" + this.Account.Margin + "\t\tLevel\t" + marginlevel + "%\t\tProfit\t" + Math.Round(this.Account.UnrealizedNetProfit, 2) + "\t" + Pos_LastTime, StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("cadaud", "\nSub_CADAUD\t" + Sub_cadaud.ToString() + "\tCADAUD_A\t" + Sub_cadaudabove.ToString() + "\t" + AV_cadaudabove.ToString() + "\t" + Pos_cadaudabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(cadaudAbove), 2) + "\tCADAUD_B\t" + Sub_cadaudbelow.ToString() + "\t" + AV_cadaudbelow.ToString() + "\t" + Pos_cadaudbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(cadaudBelow), 2), StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("jpyaud", "\n\nSub_JPYAUD\t" + Sub_jpyaud.ToString() + "\tJPYAUD_A\t" + Sub_jpyaudabove.ToString() + "\t" + AV_jpyaudabove.ToString() + "\t" + Pos_jpyaudabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(jpyaudAbove), 2) + "\tJPYAUD_B\t" + Sub_jpyaudbelow.ToString() + "\t" + AV_jpyaudbelow.ToString() + "\t" + Pos_jpyaudbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(jpyaudBelow), 2), StaticPosition.TopLeft, Colors.White);
            ChartObjects.DrawText("jpycad", "\n\n\nSub_JPYCAD\t" + Sub_jpycad.ToString() + "\tJPYCAD_A\t" + Sub_jpycadabove.ToString() + "\t" + AV_jpycadabove.ToString() + "\t" + Pos_jpycadabove.Count.ToString() + "\t" + Math.Round(this.TotalProfits(jpycadAbove), 2) + "\tJPYCAD_B\t" + Sub_jpycadbelow.ToString() + "\t" + AV_jpycadbelow.ToString() + "\t" + Pos_jpycadbelow.Count.ToString() + "\t" + Math.Round(this.TotalProfits(jpycadBelow), 2), StaticPosition.TopLeft, Colors.White);
        }
    }
}
