﻿using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using cAlgo.Indicators;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ArbitrageHedge : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public int Init_Volume { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }

        [Parameter(DefaultValue = "GBPUSD")]
        public string Symbol2 { get; set; }

        [Parameter(DefaultValue = 30)]
        public int Distance { get; set; }

        private Symbol OtherSymbol;
        private string MaAbove;
        private string MaBelow;
        private OrderParams initBuyEur, initBuyGbp, initSellEur, initSellGbp;
        private MultiCurrency MC;
        protected override void OnStart()
        {
            MaAbove = "above" + Symbol.Code + "And" + Symbol2;
            MaBelow = "below" + Symbol.Code + "And" + Symbol2;
            MC = Indicators.GetIndicator<MultiCurrency>(Period, Symbol2);
            OtherSymbol = MarketData.GetSymbol(Symbol2);
            double slippage = 2;
            //maximun slippage in point,if order execution imposes a higher slipage, the order is not executed.
            initBuyEur = new OrderParams(TradeType.Buy, Symbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initBuyGbp = new OrderParams(TradeType.Buy, OtherSymbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellEur = new OrderParams(TradeType.Sell, Symbol, Init_Volume, MaBelow, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            initSellGbp = new OrderParams(TradeType.Sell, OtherSymbol, Init_Volume, MaAbove, null, null, slippage, null, null, new System.Collections.Generic.List<double> 
            {
                            });
        }

        protected override void OnTick()
        {
            chartdraw();
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            var subtract = Math.Abs(result - average).ToString();
            initBuyEur.Comment = subtract;
            initBuyGbp.Comment = subtract;
            initSellEur.Comment = subtract;
            initSellGbp.Comment = subtract;

            if (opensignal() == "buyeur")
            {
                this.executeOrder(initBuyEur);
                this.executeOrder(initSellGbp);
            }
            if (opensignal() == "selleur")
            {
                this.executeOrder(initSellEur);
                this.executeOrder(initBuyGbp);
            }
            if (closesignal() == "closebuy")
            {
                this.closeAllLabel(MaAbove);
            }
            if (closesignal() == "closesell")
            {
                this.closeAllLabel(MaBelow);
            }
        }
        private string opensignal()
        {
            List<Position> MaAbovePos = new List<Position>(this.GetPositions(MaAbove));
            List<Position> MaBelowPos = new List<Position>(this.GetPositions(MaBelow));
            List<Position> positions = MaAbovePos;
            positions.AddRange(MaBelowPos);
            positions.Reverse();
            double aboveaverage = 0;
            double belowaverage = 0;
            if (MaAbovePos.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in MaAbovePos)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                aboveaverage = totalCom / MaAbovePos.Count;
            }
            if (MaBelowPos.Count != 0)
            {
                double totalCom = 0;
                foreach (var pos in MaBelowPos)
                {
                    totalCom += Convert.ToDouble(pos.Comment);
                }
                belowaverage = totalCom / MaBelowPos.Count;
            }
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            string sig = null;
            if (positions.Count == 0)
            {
                if (result > average + Distance)
                    sig = "buyeur";
                if (result < average - Distance)
                    sig = "selleur";
            }
            else
            {
                var now = DateTime.UtcNow;
                if (DateTime.Compare(positions[0].EntryTime.AddHours(1), now) < 0)
                {
                    if (result > average + aboveaverage)
                        sig = "buyeur";
                    if (result < average - belowaverage)
                        sig = "selleur";
                }
            }
            return sig;
        }
        private string closesignal()
        {
            string sig = null;
            List<Position> MaAbovePos = new List<Position>(this.GetPositions(MaAbove));
            List<Position> MaBelowPos = new List<Position>(this.GetPositions(MaBelow));
            List<Position> positions = MaAbovePos;
            positions.AddRange(MaBelowPos);
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            if (positions.Count != 0)
            {
                if (result >= average)
                    sig = "closesell";
                if (result <= average)
                    sig = "closebuy";
            }
            return sig;
        }
        private void chartdraw()
        {
            List<Position> MaAbovePos = new List<Position>(this.GetPositions(MaAbove));
            List<Position> MaBelowPos = new List<Position>(this.GetPositions(MaBelow));
            var result = MC.Result.LastValue;
            var average = MC.Average.LastValue;
            var subtract = Math.Round(result - average);
            double subAbove = 0;
            double subBelow = 0;
            if (MaAbovePos.Count != 0)
            {
                double total = 0;
                foreach (var pos in MaAbovePos)
                {
                    if (pos.SymbolCode == Symbol.Code)
                        total -= pos.EntryPrice;
                    if (pos.SymbolCode == Symbol2)
                        total += pos.EntryPrice;
                }
                subAbove = Math.Round(total / MaAbovePos.Count * 2 / Symbol.PipSize - average);
            }
            if (MaBelowPos.Count != 0)
            {
                double total = 0;
                foreach (var pos in MaBelowPos)
                {
                    if (pos.SymbolCode == Symbol.Code)
                        total -= pos.EntryPrice;
                    if (pos.SymbolCode == Symbol2)
                        total += pos.EntryPrice;
                }
                subBelow = Math.Round(total / MaBelowPos.Count * 2 / Symbol.PipSize - average);
            }
            if (subAbove <= 0)
            {
                this.closeAllLabel(MaAbove);
            }
            if (subBelow >= 0)
            {
                this.closeAllLabel(MaBelow);
            }
            ChartObjects.DrawText("subtract", "Sub: " + subtract.ToString() + "    A: " + subAbove.ToString() + "    B: " + subBelow.ToString(), StaticPosition.TopLeft, Colors.White);
        }
    }
}