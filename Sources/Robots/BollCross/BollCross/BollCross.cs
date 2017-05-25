#region 程序概要
//通过双布林带中线下单
//中线分为上中线和下中线
//下单模式两种：OnTick或OnBar
//默认参数：40/20/2或48/24/2
//以上下线为平均价进行加倍
#endregion
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using cAlgo.Strategies;
using System;
using System.Collections.Generic;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Bollinger : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

        [Parameter("Consolidation", DefaultValue = 5)]
        public int Consolidation { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Boll Periods", DefaultValue = 40)]
        public int Periods { get; set; }

        [Parameter("Boll Deviations", DefaultValue = 2)]
        public double Deviations { get; set; }

        [Parameter("Boll MA Type")]
        public MovingAverageType MAType { get; set; }

        #endregion
        #region
        string buylabel = "Bollinger_buy";
        string selllabel = "Bollinger_sell";
        DoubleBollStrategy Boll;
        OrderParams initBuyOP, initSellOP;
        string isclose = null;
        #endregion
        protected override void OnStart()
        {
            Boll = new DoubleBollStrategy(this, Source, Periods, Deviations, MAType);
            double slippage = 2;
            // maximum slippage in point, if order execution imposes a higher slippage, the order is not executed.
            initBuyOP = new OrderParams(TradeType.Buy, Symbol, Init_Volume, buylabel, null, null, slippage, null, null, new List<double> 
            {
                            });
            initSellOP = new OrderParams(TradeType.Sell, Symbol, Init_Volume, selllabel, null, null, slippage, null, null, new List<double> 
            {
                            });
        }
        protected override void OnBar()
        {
            List<Position> buypositions = new List<Position>(this.GetPositions(buylabel));
            List<Position> sellpositions = new List<Position>(this.GetPositions(selllabel));
            buypositions.Reverse();
            sellpositions.Reverse();
            #region Close
            //Close BuyPositions
            if (buypositions.Count != 0 && Boll.signal1().isSell())
                isclose = "buy";
            if (isclose == "buy" && Boll.signal3().isBuy())
            {
                this.closeAllBuyPositions(buylabel);
                buypositions.Clear();
                isclose = null;
            }
            //Close SellPositions
            if (sellpositions.Count != 0 && Boll.signal1().isBuy())
                isclose = "sell";
            if (isclose == "sell" && Boll.signal3().isSell())
            {
                this.closeAllSellPositions(selllabel);
                sellpositions.Clear();
                isclose = null;
            }
            #endregion
            #region Open Position(buylabel and selllabel)
            //Open b_buylabel
            if (Boll.signal2().isBuy())
            {
                if (buypositions.Count == 0)
                {
                    this.executeOrder(initBuyOP);
                    return;
                }
                else if (buypositions.Count != 0 && buypositions[0].EntryPrice > Symbol.Mid() && MarketSeries.barsAgo(buypositions[0]) >= Consolidation)
                {
                    var goalprice = Boll.uppermax;
                    if (this.AveragePrice(buylabel) < goalprice)
                    {
                        this.executeOrder(initBuyOP);
                        return;
                    }
                    //else
                    //{
                    //    OrderParams MarBuyOP = new OrderParams(initBuyOP);
                    //    MarBuyOP.Volume = this.MartingaleLot(buylabel, goalprice);
                    //    this.executeOrder(MarBuyOP);
                    //    return;
                    //}
                }
            }
            else if (Boll.signal1().isBuy())
            {
                if (buypositions.Count != 0 && buypositions[0].EntryPrice > Symbol.Mid() && MarketSeries.barsAgo(buypositions[0]) >= Consolidation)
                {
                    var goalprice = Boll.uppermax;
                    if (this.AveragePrice(buylabel) > goalprice)
                    {
                        OrderParams MarBuyOP = new OrderParams(initBuyOP);
                        MarBuyOP.Volume = this.MartingaleLot(buylabel, goalprice);
                        this.executeOrder(MarBuyOP);
                        return;
                    }
                }
            }
            //Open b_selllabel
            if (Boll.signal2().isSell())
            {
                if (sellpositions.Count == 0)
                {
                    this.executeOrder(initSellOP);
                    return;
                }
                else if (sellpositions.Count != 0 && sellpositions[0].EntryPrice < Symbol.Mid() && MarketSeries.barsAgo(sellpositions[0]) >= Consolidation)
                {
                    var goalprice = Boll.lowermin;
                    if (this.AveragePrice(selllabel) > goalprice)
                    {
                        this.executeOrder(initSellOP);
                        return;
                    }
                    //else
                    //{
                    //    OrderParams MarSellOP = new OrderParams(initSellOP);
                    //    MarSellOP.Volume = this.MartingaleLot(selllabel, goalprice);
                    //    this.executeOrder(MarSellOP);
                    //    return;
                    //}
                }
            }
            else if (Boll.signal1().isSell())
            {
                if (sellpositions.Count != 0 && sellpositions[0].EntryPrice < Symbol.Mid() && MarketSeries.barsAgo(sellpositions[0]) >= Consolidation)
                {
                    var goalprice = Boll.lowermin;
                    if (this.AveragePrice(selllabel) < goalprice)
                    {
                        OrderParams MarSellOP = new OrderParams(initSellOP);
                        MarSellOP.Volume = this.MartingaleLot(selllabel, goalprice);
                        this.executeOrder(MarSellOP);
                        return;
                    }
                }
            }
            #endregion
        }
    }
}
