﻿using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedgeUSD : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        private Symbol eurSymbol, gbpSymbol, chfSymbol;
        private string eurchfAbove, gbpchfAbove, gbpeurAbove;
        private string eurchfBelow, gbpchfBelow, gbpeurBelow;
        private OrderParams initBuyeur, initBuygbp, initBuychf, initSelleur, initSellgbp, initSellchf;
        private USD_EURCHF usd_eurchf;
        private USD_GBPCHF usd_gbpchf;
        private USD_GBPEUR usd_gbpeur;
        protected override void OnStart()
        {
            eurSymbol = MarketData.GetSymbol("EURUSD");
            gbpSymbol = MarketData.GetSymbol("GBPUSD");
            chfSymbol = MarketData.GetSymbol("USDCHF");
            eurchfAbove = "Above" + eurSymbol.Code + "And" + chfSymbol.Code;
            gbpchfAbove = "Above" + gbpSymbol.Code + "And" + chfSymbol.Code;
            gbpeurAbove = "Above" + gbpSymbol.Code + "And" + eurSymbol.Code;
            eurchfBelow = "Below" + eurSymbol.Code + "And" + chfSymbol.Code;
            gbpchfBelow = "Below" + gbpSymbol.Code + "And" + chfSymbol.Code;
            gbpeurBelow = "Below" + gbpSymbol.Code + "And" + eurSymbol.Code;
            usd_eurchf = Indicators.GetIndicator<USD_EURCHF>();
            usd_gbpchf = Indicators.GetIndicator<USD_GBPCHF>();
            usd_gbpeur = Indicators.GetIndicator<USD_GBPEUR>();
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuyeur = new OrderParams(TradeType.Buy, eurSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuygbp = new OrderParams(TradeType.Buy, gbpSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuychf = new OrderParams(TradeType.Buy, chfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSelleur = new OrderParams(TradeType.Sell, eurSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellgbp = new OrderParams(TradeType.Sell, gbpSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellchf = new OrderParams(TradeType.Sell, chfSymbol, Init_Volume, null, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
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
            var Sub_eurchf = Math.Round(RS_eurchf - AV_eurchf).ToString();
            var Sub_gbpchf = Math.Round(RS_gbpchf - AV_gbpchf).ToString();
            var Sub_gbpeur = Math.Round(RS_gbpeur - AV_gbpeur).ToString();
            if (opensignal() == "BuyEURAndBuyCHF")
            {
                initBuyeur.Label = eurchfBelow;
                initBuychf.Label = eurchfBelow;
                initBuyeur.Comment = Sub_eurchf;
                initBuychf.Comment = Sub_eurchf;
                this.executeOrder(initBuyeur);
                this.executeOrder(initBuychf);
            }
            if (opensignal() == "SellEURAndSellCHF")
            {
                initSelleur.Label = eurchfAbove;
                initSellchf.Label = eurchfAbove;
                initSelleur.Comment = Sub_eurchf;
                initSellchf.Comment = Sub_eurchf;
                this.executeOrder(initSelleur);
                this.executeOrder(initSellchf);
            }
            if (opensignal() == "BuyGBPAndBuyCHF")
            {
                initBuygbp.Label = gbpchfBelow;
                initBuychf.Label = gbpchfBelow;
                initBuygbp.Comment = Sub_gbpchf;
                initBuychf.Comment = Sub_gbpchf;
                this.executeOrder(initBuygbp);
                this.executeOrder(initBuychf);
            }
            if (opensignal() == "SellGBPAndSellCHF")
            {
                initSellgbp.Label = gbpchfAbove;
                initSellchf.Label = gbpchfAbove;
                initSellgbp.Comment = Sub_gbpchf;
                initSellchf.Comment = Sub_gbpchf;
                this.executeOrder(initSellgbp);
                this.executeOrder(initSellchf);
            }
            if (opensignal() == "BuyGBPAndSellEUR")
            {
                initBuygbp.Label = gbpeurBelow;
                initSelleur.Label = gbpeurBelow;
                initBuygbp.Comment = Sub_gbpeur;
                initSelleur.Comment = Sub_gbpeur;
                this.executeOrder(initBuygbp);
                this.executeOrder(initSelleur);
            }
            if (opensignal() == "SellGBPAndBuyEUR")
            {
                initSellgbp.Label = gbpeurAbove;
                initBuyeur.Label = gbpeurAbove;
                initSellgbp.Comment = Sub_gbpeur;
                initBuyeur.Comment = Sub_gbpeur;
                this.executeOrder(initSellgbp);
                this.executeOrder(initBuyeur);
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
            var now = DateTime.UtcNow;
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
            double AV_eurchfabove = Distance;
            double AV_eurchfbelow = -Distance;
            double AV_gbpchfabove = Distance;
            double AV_gbpchfbelow = -Distance;
            double AV_gbpeurabove = Distance;
            double AV_gbpeurbelow = -Distance;
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
            #endregion
            #region EURCHF
            if (Pos_eurchfabove.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_eurchfabove)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                AV_eurchfabove = totalCom / Pos_eurchfabove.Count;
            }
            if (Pos_eurchfbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_eurchfbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
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
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                AV_gbpchfabove = totalCom / Pos_gbpchfabove.Count;
            }
            if (Pos_gbpchfbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpchfbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
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
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                AV_gbpeurabove = totalCom / Pos_gbpeurabove.Count;
            }
            if (Pos_gbpeurbelow.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in Pos_gbpeurbelow)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                AV_gbpeurbelow = totalCom / Pos_gbpeurbelow.Count;
            }
            #endregion
            #region Signal
            if (DateTime.Compare(Pos_LastTime, now) < 0)
            {
                if (RS_eurchf > AV_eurchf + AV_eurchfabove)
                    eurchfsignal = "SellEURAndSellCHF";
                if (RS_eurchf < AV_eurchf + AV_eurchfbelow)
                    eurchfsignal = "BuyEURAndBuyCHF";
                if (RS_gbpchf > AV_gbpchf + AV_gbpchfabove)
                    gbpchfsignal = "SellGBPAndSellCHF";
                if (RS_gbpchf < AV_gbpchf + AV_gbpchfbelow)
                    gbpchfsignal = "BuyGBPAndBuyCHF";
                if (RS_gbpeur > AV_gbpeur + AV_gbpeurabove)
                    gbpeursignal = "SellGBPAndBuyEUR";
                if (RS_gbpeur < AV_gbpeur + AV_gbpeurbelow)
                    gbpeursignal = "BuyGBPAndSellEUR";
            }
            if (eurchfsignal == "BuyEURAndBuyCHF")
            {
                eurusd++;
                usdchf++;
            }
            if (eurchfsignal == "SellEURAndSellCHF")
            {
                usdchf--;
                eurusd--;
            }
            if (gbpchfsignal == "BuyGBPAndBuyCHF")
            {
                gbpusd++;
                usdchf++;
            }
            if (gbpchfsignal == "SellGBPAndSellCHF")
            {
                usdchf--;
                gbpusd--;
            }
            if (gbpeursignal == "BuyGBPAndSellEUR")
            {
                gbpusd++;
                eurusd--;
            }
            if (gbpeursignal == "SellGBPAndBuyEUR")
            {
                eurusd++;
                gbpusd--;
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
            var Sub_eurchf = Math.Round(RS_eurchf - AV_eurchf).ToString();
            var Sub_gbpchf = Math.Round(RS_gbpchf - AV_gbpchf).ToString();
            var Sub_gbpeur = Math.Round(RS_gbpeur - AV_gbpeur).ToString();
            double Sub_eurchfabove = 0;
            double Sub_eurchfbelow = 0;
            double Sub_gbpchfabove = 0;
            double Sub_gbpchfbelow = 0;
            double Sub_gbpeurabove = 0;
            double Sub_gbpeurbelow = 0;
            #region EURCHF
            if (Pos_eurchfabove.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_eurchfabove)
                {
                    if (pos.SymbolCode == chfSymbol.Code)
                        total -= 1 / pos.EntryPrice;
                    if (pos.SymbolCode == eurSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_eurchfabove = Math.Round(total / Pos_eurchfabove.Count * 2 / 0.0001 + 10000 - AV_eurchf);
            }
            if (Pos_eurchfbelow.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_eurchfbelow)
                {
                    if (pos.SymbolCode == chfSymbol.Code)
                        total -= 1 / pos.EntryPrice;
                    if (pos.SymbolCode == eurSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_eurchfbelow = Math.Round(total / Pos_eurchfbelow.Count * 2 / 0.0001 + 10000 - AV_eurchf);
            }
            ChartObjects.DrawText("eurchf", "Sub_EURCHF     " + Sub_eurchf.ToString() + "       EURCHF_A       " + Sub_eurchfabove.ToString() + "      EURCHF_B      " + Sub_eurchfbelow.ToString(), StaticPosition.TopLeft, Colors.White);
            #endregion
            #region GBPCHF
            if (Pos_gbpchfabove.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_gbpchfabove)
                {
                    if (pos.SymbolCode == chfSymbol.Code)
                        total -= 1 / pos.EntryPrice;
                    if (pos.SymbolCode == gbpSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_gbpchfabove = Math.Round(total / Pos_gbpchfabove.Count * 2 / 0.0001 + 10000 - AV_gbpchf);
            }
            if (Pos_gbpchfbelow.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_gbpchfbelow)
                {
                    if (pos.SymbolCode == chfSymbol.Code)
                        total -= 1 / pos.EntryPrice;
                    if (pos.SymbolCode == gbpSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_gbpchfbelow = Math.Round(total / Pos_gbpchfbelow.Count * 2 / 0.0001 + 10000 - AV_gbpchf);
            }
            ChartObjects.DrawText("gbpchf", "\nSub_GBPCHF       " + Sub_gbpchf.ToString() + "       GBPCHF_A       " + Sub_gbpchfabove.ToString() + "       GBPCHF_B        " + Sub_gbpchfbelow.ToString(), StaticPosition.TopLeft, Colors.White);
            #endregion
            #region GBPEUR
            if (Pos_gbpeurabove.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_gbpeurabove)
                {
                    if (pos.SymbolCode == eurSymbol.Code)
                        total -= pos.EntryPrice;
                    if (pos.SymbolCode == gbpSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_gbpeurabove = Math.Round(total / Pos_gbpeurabove.Count * 2 / 0.0001 + 10000 - AV_gbpeur);
            }
            if (Pos_gbpeurbelow.Count != 0)
            {
                double total = 0;
                foreach (var pos in Pos_gbpeurbelow)
                {
                    if (pos.SymbolCode == eurSymbol.Code)
                        total -= pos.EntryPrice;
                    if (pos.SymbolCode == gbpSymbol.Code)
                        total += pos.EntryPrice;
                }
                Sub_gbpeurbelow = Math.Round(total / Pos_gbpeurbelow.Count * 2 / 0.0001 + 10000 - AV_gbpeur);
            }
            ChartObjects.DrawText("gbpeur", "\n\nSub_GBPEUR      " + Sub_gbpeur.ToString() + "       GBPEUR_A        " + Sub_gbpeurabove.ToString() + "      GBPEUR_B        " + Sub_gbpeurbelow.ToString(), StaticPosition.TopLeft, Colors.White);
            #endregion
        }
    }
}