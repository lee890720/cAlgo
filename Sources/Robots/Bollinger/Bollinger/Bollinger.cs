#region 程序概要
//通过布林带上下线下单，默认参数：40/2/0.5
//以上下线为平均价进行加倍
#endregion
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using cAlgo.Strategies;
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

        [Parameter("Dev Amendment", DefaultValue = 0.5)]
        public double D_Amendment { get; set; }

        [Parameter("Boll MA Type")]
        public MovingAverageType MAType { get; set; }

        #endregion
        #region
        string buylabel = "Bollinger_buy";
        string selllabel = "Bollinger_sell";
        BollingerStrategy Boll_Open, Boll_Close;
        OrderParams initBuyOP, initSellOP;
        #endregion
        protected override void OnStart()
        {
            Boll_Open = new BollingerStrategy(this, Source, Periods, Deviations, MAType);
            Boll_Close = new BollingerStrategy(this, Source, Periods, Deviations - D_Amendment, MAType);
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
            buypositions.Reverse();
            List<Position> sellpositions = new List<Position>(this.GetPositions(selllabel));
            sellpositions.Reverse();
            #region Close
            //Close BuyPositions
            if (buypositions.Count != 0)
            {
                if (Boll_Close.signal().isSell())
                {
                    this.closeAllBuyPositions(buylabel);
                }
            }
            //Close SellPositions
            if (sellpositions.Count != 0)
            {
                if (Boll_Close.signal().isBuy())
                {
                    this.closeAllSellPositions(selllabel);
                }
            }
            #endregion
            #region Open Position(buylabel and selllabel)
            //Open b_buylabel
            if (Boll_Open.signal().isBuy())
            {
                if (buypositions.Count == 0)
                {
                    this.executeOrder(initBuyOP);
                    return;
                }
                else if (buypositions.Count != 0 && MarketSeries.barsAgo(buypositions[0]) >= Consolidation)
                {
                    var goalprice = Boll_Open.boll.Top.Last(1);
                    if (this.AveragePrice(buylabel) < goalprice)
                    {
                        this.executeOrder(initBuyOP);
                        return;
                    }
                    else
                    {
                        OrderParams MarBuyOP = new OrderParams(initBuyOP);
                        MarBuyOP.Volume = this.MartingaleLot(buylabel, goalprice);
                        this.executeOrder(MarBuyOP);
                        return;
                    }
                }
            }
            //Open b_selllabel
            if (Boll_Open.signal().isSell())
            {
                if (sellpositions.Count == 0)
                {
                    this.executeOrder(initSellOP);
                    return;
                }
                else if (sellpositions.Count != 0 && MarketSeries.barsAgo(sellpositions[0]) >= Consolidation)
                {
                    var goalprice = Boll_Open.boll.Bottom.Last(1);
                    if (this.AveragePrice(selllabel) > goalprice)
                    {
                        this.executeOrder(initSellOP);
                        return;
                    }
                    else
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
