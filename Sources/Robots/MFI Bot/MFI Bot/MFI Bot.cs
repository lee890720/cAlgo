using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;


/*

Version 1.0
Developed by afhacker (Ahmad Noman Musleh)
Email : afhackermubasher@gmail.com

*/


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MFIBot : Robot
    {
        [Parameter("ATR Periods", DefaultValue = 20)]
        public int atrPeriod { get; set; }

        [Parameter("ATR Multiplier", DefaultValue = 3)]
        public int atrMultiplier { get; set; }

        [Parameter("MFI Periods", DefaultValue = 20)]
        public int moneyFlowIndexPeriods { get; set; }

        [Parameter("Buy Above", DefaultValue = 40)]
        public int moneyFlowIndexBuy { get; set; }

        [Parameter("Sell Below", DefaultValue = 70)]
        public int moneyFlowIndexSell { get; set; }

        [Parameter("Time Filter", DefaultValue = false)]
        public bool timeFilter { get; set; }

        [Parameter("Start Hour", DefaultValue = 7, MinValue = 0, MaxValue = 23)]
        public int startHour { get; set; }

        [Parameter("End Hour", DefaultValue = 13, MinValue = 0, MaxValue = 23)]
        public int endHour { get; set; }

        [Parameter("Order Distance", DefaultValue = 1, MinValue = 1)]
        public double orderDistance { get; set; }

        [Parameter("% Risk Per Trade", DefaultValue = 1, MinValue = 0.1, MaxValue = 10.0)]
        public double riskPercentage { get; set; }




        private AverageTrueRange atr;
        private HeikenAshi ha;
        private MoneyFlowIndex moneyFlowIndex;
        private string label;

        protected override void OnStart()
        {
            label = "MoneyFlowIndexBot";
            if (IsBacktesting)
                label += " BT";

            atr = Indicators.AverageTrueRange(atrPeriod, MovingAverageType.Exponential);
            ha = Indicators.GetIndicator<HeikenAshi>(1);
            moneyFlowIndex = Indicators.MoneyFlowIndex(moneyFlowIndexPeriods);
        }



        protected override void OnBar()
        {

            foreach (var order in PendingOrders)
            {
                if (order.Label == label && order.SymbolCode == Symbol.Code)
                {
                    CancelPendingOrder(order);
                }
            }


            bool heikenAshiOk = false;
            bool bullishSignal = false;
            bool bearishSignal = false;
            if (ha.Close.Last(1) > ha.Open.Last(1) && ha.Close.Last(2) < ha.Open.Last(2))
            {
                heikenAshiOk = true;
                bullishSignal = true;
            }
            else if (ha.Close.Last(1) < ha.Open.Last(1) && ha.Close.Last(2) > ha.Open.Last(2))
            {
                heikenAshiOk = true;
                bearishSignal = true;

            }


            // Money Flow Index Filter
            bool moneyFlowIndexOk = false;
            if (bullishSignal && moneyFlowIndex.Result.Last(1) > moneyFlowIndexBuy && moneyFlowIndex.Result.Last(2) < moneyFlowIndexBuy)
            {
                moneyFlowIndexOk = true;
            }
            else if (bearishSignal && moneyFlowIndex.Result.Last(1) < moneyFlowIndexSell && moneyFlowIndex.Result.Last(2) > moneyFlowIndexSell)
            {
                moneyFlowIndexOk = true;
            }



            // Time Filter
            bool isTimeCorrect = false;
            if (timeFilter)
                isTimeCorrect = timeFilterCheck();
            else
                isTimeCorrect = true;


            // Placing The stop order
            if (heikenAshiOk && moneyFlowIndexOk && isTimeCorrect)
            {
                // Order Attributes
                double stopLoss = Math.Round((atr.Result.LastValue * Math.Pow(10, Symbol.Digits - 1)) * atrMultiplier, 1);

                long posVolume = PositionVolume(stopLoss);

                if (bullishSignal)
                {
                    PlaceStopOrder(TradeType.Buy, Symbol, posVolume, ha.High.Last(1) + (Symbol.PipSize * orderDistance), label, stopLoss, stopLoss * 2);
                }
                else if (bearishSignal)
                {
                    PlaceStopOrder(TradeType.Sell, Symbol, posVolume, ha.Low.Last(1) - (Symbol.PipSize * orderDistance), label, stopLoss, stopLoss * 2);
                }

            }
        }



        // Position volume calculator
        private long PositionVolume(double stopLossInPips)
        {
            double costPerPip = (double)((int)(Symbol.PipValue * 10000000)) / 100;
            double positionSizeForRisk = Math.Round((Account.Balance * riskPercentage / 100) / (stopLossInPips * costPerPip), 2);

            if (positionSizeForRisk < 0.01)
                positionSizeForRisk = 0.01;
            return Symbol.QuantityToVolume(positionSizeForRisk);

        }


        // Checking the opening time of candle
        private bool timeFilterCheck()
        {
            bool timeOk = false;
            if (timeFilter && MarketSeries.OpenTime.Last(1).Hour >= startHour && MarketSeries.OpenTime.Last(1).Hour <= endHour)
                timeOk = true;
            else if (!timeFilter)
                timeOk = true;

            if (timeOk)
                return true;
            else
                return false;
        }

    }
}
