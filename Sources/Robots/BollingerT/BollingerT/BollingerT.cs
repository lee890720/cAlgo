using System;
using System.Linq;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{

    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Bollinger : Robot
    {
        #region Parameter
        [Parameter("INIT Volume", DefaultValue = 1000, MinValue = 1)]
        public int Init_Volume { get; set; }

        [Parameter("Lot(s)/10000USD", DefaultValue = 5, MinValue = 0.01)]
        public double per { get; set; }

        [Parameter("IsOnTick", DefaultValue = false)]
        public bool IsOnTick { get; set; }

        [Parameter("IsOnBar", DefaultValue = false)]
        public bool IsOnBar { get; set; }

        [Parameter("IsBollDouble", DefaultValue = false)]
        public bool IsBollDouble { get; set; }

        [Parameter("IsRisingAndFalling", DefaultValue = false)]
        public bool IsRisingAndFalling { get; set; }

        [Parameter("IsClosePosAmendment", DefaultValue = false)]
        public bool IsClosePosAmendment { get; set; }

        [Parameter("Band Height (pips)", DefaultValue = 1, MinValue = 0)]
        public double BandHeightPips { get; set; }

        [Parameter("Consolidation Periods", DefaultValue = 5)]
        public int ConsolidationPeriods { get; set; }

        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Bollinger Bands Periods", DefaultValue = 40)]
        public int Periods { get; set; }

        [Parameter("Bollinger Bands Deviations", DefaultValue = 2)]
        public double Deviations { get; set; }

        [Parameter("Bands Deviations Amendment", DefaultValue = 0.5)]
        public double Deviations_Amendment { get; set; }

        [Parameter("Bollinger Bands MA Type")]
        public MovingAverageType MAType { get; set; }

        #endregion
        #region
        BollingerBands boll;
        BollingerBands bollhalf;
        BollingerBands boll_increase;
        BollingerBands boll_decrease;
        string b_buylabel = "Bollinger_buy";
        string b_selllabel = "Bollinger_sell";
        string t_buylabel = "Buy_Trend";
        string t_selllabel = "Sell_Trend";
        int Open_Consolidation, Close_Consolidation;
        int tick_buy, tick_sell;
        bool tick_Isbuy, tick_Issell;
        #endregion
        protected override void OnStart()
        {
            boll = Indicators.BollingerBands(Source, Periods, Deviations, MAType);
            bollhalf = Indicators.BollingerBands(Source, Periods / 2, Deviations, MAType);
            boll_increase = Indicators.BollingerBands(Source, Periods, Deviations + Deviations_Amendment, MAType);
            boll_decrease = Indicators.BollingerBands(Source, Periods, Deviations - Deviations_Amendment, MAType);
            Open_Consolidation = ConsolidationPeriods;
            Close_Consolidation = ConsolidationPeriods;
            tick_buy = ConsolidationPeriods;
            tick_sell = ConsolidationPeriods;
            tick_Isbuy = false;
            tick_Issell = false;
        }
        protected override void OnTick()
        {
            if (!IsOnTick)
                return;
            base.OnTick();
            var midmin = boll.Main.LastValue < bollhalf.Main.LastValue ? boll.Main.LastValue : bollhalf.Main.LastValue;
            var midmax = boll.Main.LastValue > bollhalf.Main.LastValue ? boll.Main.LastValue : bollhalf.Main.LastValue;
            var uppermin = boll.Top.LastValue < bollhalf.Top.LastValue ? boll.Top.LastValue : bollhalf.Top.LastValue;
            var uppermax = boll.Top.LastValue > bollhalf.Top.LastValue ? boll.Top.LastValue : bollhalf.Top.LastValue;
            var lowermin = boll.Bottom.LastValue < bollhalf.Bottom.LastValue ? boll.Bottom.LastValue : bollhalf.Bottom.LastValue;
            var lowermax = boll.Bottom.LastValue > bollhalf.Bottom.LastValue ? boll.Bottom.LastValue : bollhalf.Bottom.LastValue;
            var t_buyposs = this.GetPositions(t_buylabel);
            var t_sellposs = this.GetPositions(t_selllabel);
            if (Symbol.Mid() >= midmax && Symbol.Mid() <= midmax + 5 * Symbol.PipSize && tick_buy >= ConsolidationPeriods)
            {
                if (t_buyposs.Length == 0)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, t_buylabel);
                    tick_buy = 0;
                    return;
                }
                if (t_buyposs.Length != 0 && this.TotalProfits(t_buylabel) < 0 && this.AveragePrice(t_buylabel) <= uppermax)
                {
                    ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, t_buylabel);
                    tick_buy = 0;
                    return;
                }
                if (t_buyposs.Length != 0 && this.TotalProfits(t_buylabel) < 0 && this.AveragePrice(t_buylabel) > uppermax)
                {
                    long volume = (long)this.MartingaleLot(t_buylabel, uppermax);
                    ExecuteMarketOrder(TradeType.Buy, Symbol, volume, t_buylabel);
                    tick_buy = 0;
                    return;
                }
            }
            if (Symbol.Mid() <= midmin && Symbol.Mid() >= midmin - 5 * Symbol.PipSize && tick_sell >= ConsolidationPeriods)
            {
                if (t_sellposs.Length == 0)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, t_selllabel);
                    tick_sell = 0;
                    return;
                }
                if (t_sellposs.Length != 0 && this.TotalProfits(t_selllabel) < 0 && this.AveragePrice(t_selllabel) >= lowermax)
                {
                    ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, t_selllabel);
                    tick_sell = 0;
                    return;
                }
                if (t_sellposs.Length != 0 && this.TotalProfits(t_selllabel) < 0 && this.AveragePrice(t_selllabel) < lowermax)
                {
                    long volume = (long)this.MartingaleLot(t_selllabel, uppermax);
                    ExecuteMarketOrder(TradeType.Sell, Symbol, volume, t_selllabel);
                    tick_sell = 0;
                    return;
                }
            }
            if ((Symbol.Mid() <= midmax || Symbol.Mid() >= uppermax) && this.TotalProfits(t_buylabel) > 0)
            {
                this.closeAllBuyPositions(t_buylabel);
            }
            if ((Symbol.Mid() >= midmin || Symbol.Mid() <= lowermin) && this.TotalProfits(t_selllabel) > 0)
            {
                this.closeAllSellPositions(t_selllabel);
            }
        }
        protected override void OnBar()
        {
            var t_uppermax = boll.Top.Last(1) > bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var t_lowermin = boll.Bottom.Last(1) < bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            tick_buy++;
            tick_sell++;
            if (Symbol.Mid() > t_uppermax)
                tick_Isbuy = true;
            else
                tick_Isbuy = false;
            if (Symbol.Mid() < t_lowermin)
                tick_Issell = true;
            else
                tick_Issell = false;
            if (!IsOnBar)
                return;
            #region parameter
            var upper = boll.Top.Last(1);
            var lower = boll.Bottom.Last(1);
            var midmin = boll.Main.Last(1) < bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var midmax = boll.Main.Last(1) > bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var uppermin = boll.Top.Last(1) < bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var uppermax = boll.Top.Last(1) > bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var lowermin = boll.Bottom.Last(1) < bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var lowermax = boll.Bottom.Last(1) > bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var upper_increase = boll_increase.Top.Last(1);
            var lower_increase = boll_increase.Bottom.Last(1);
            var upper_decrease = boll_decrease.Top.Last(1);
            var lower_decrease = boll_decrease.Bottom.Last(1);
            var balancelot = this.BalanceLots(per);
            var b_buyposs = this.GetPositions(b_buylabel);
            var b_sellposs = this.GetPositions(b_selllabel);
            #endregion
            #region define open_consolidation and close_consolidation
            if (upper - lower >= BandHeightPips * Symbol.PipSize)
            {
                Open_Consolidation = Open_Consolidation + 1;
            }
            else
            {
                Open_Consolidation = 0;
            }
            if (upper - lower >= BandHeightPips * Symbol.PipSize)
            {
                Close_Consolidation = Close_Consolidation + 1;
            }
            else
            {
                Close_Consolidation = 0;
            }
            #endregion
            #region Close Position(b_buylabel and b_selllabel)
            if (Close_Consolidation >= ConsolidationPeriods)
            {
                //Close BuyPositions
                if (b_buyposs.Length != 0)
                {
                    double goalprice = 0;
                    if (!IsBollDouble)
                    {
                        if (IsClosePosAmendment)
                            goalprice = upper_decrease;
                        if (!IsClosePosAmendment)
                            goalprice = upper;
                    }
                    else
                    {
                        if (!IsClosePosAmendment)
                            goalprice = uppermax;
                        if (IsClosePosAmendment)
                            goalprice = uppermin;
                    }
                    if (Symbol.Mid() > goalprice)
                    {
                        this.closeAllBuyPositions(b_buylabel);
                    }
                }
                //Close SellPositions
                if (b_sellposs.Length != 0)
                {
                    double goalprice = 0;
                    if (!IsBollDouble)
                    {
                        if (IsClosePosAmendment)
                            goalprice = lower_decrease;
                        if (!IsClosePosAmendment)
                            goalprice = lower;
                    }
                    else
                    {
                        if (!IsClosePosAmendment)
                            goalprice = lowermin;
                        if (IsClosePosAmendment)
                            goalprice = lowermax;
                    }
                    if (Symbol.Mid() < goalprice)
                    {
                        this.closeAllSellPositions(b_selllabel);
                    }
                }
            }
            #endregion
            #region Open Position(b_buylabel and b_selllabel)
            if (Open_Consolidation >= ConsolidationPeriods)
            {
                //Open b_selllabel
                bool isboll_sell = Symbol.Mid() > upper && !IsRisingAndFalling && !IsBollDouble;
                bool isbollDev_sell = Symbol.Mid() > upper_decrease && !boll.Bottom.IsFalling() && IsRisingAndFalling && !IsBollDouble;
                bool isbollDou_sell = Symbol.Mid() > uppermax && IsBollDouble && !IsRisingAndFalling;
                bool isboll_buy = Symbol.Mid() < lower && !IsRisingAndFalling && !IsBollDouble;
                bool isbollDev_buy = Symbol.Mid() < lower_decrease && !boll.Top.IsRising() && IsRisingAndFalling && !IsBollDouble;
                bool isbollDou_buy = Symbol.Mid() < lowermin && IsBollDouble && !IsRisingAndFalling;
                if (isboll_sell || isbollDev_sell || isbollDou_sell)
                {
                    double goalprice = 0;
                    if (isboll_sell)
                        goalprice = lower;
                    if (isbollDev_sell)
                        goalprice = lower_decrease;
                    if (isbollDou_sell)
                        goalprice = lowermax;
                    if (b_sellposs.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellposs.Length != 0 && this.AveragePrice(b_selllabel) >= goalprice)
                    {
                        ExecuteMarketOrder(TradeType.Sell, Symbol, Init_Volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_sellposs.Length != 0 && this.AveragePrice(b_selllabel) < goalprice)
                    {
                        long volume = this.MartingaleLot(b_selllabel, goalprice);
                        if (volume > balancelot - this.TotalLots(b_selllabel))
                            volume = (long)(balancelot - this.TotalLots(b_selllabel));
                        ExecuteMarketOrder(TradeType.Sell, Symbol, volume, b_selllabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
                //Open b_buylabel
                if (isboll_buy || isbollDev_buy || isbollDou_buy)
                {
                    double goalprice = 0;
                    if (isboll_buy)
                        goalprice = upper;
                    if (isbollDev_buy)
                        goalprice = upper_decrease;
                    if (isbollDou_buy)
                        goalprice = uppermin;
                    if (b_buyposs.Length == 0)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buyposs.Length != 0 && this.AveragePrice(b_buylabel) <= goalprice)
                    {
                        ExecuteMarketOrder(TradeType.Buy, Symbol, Init_Volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                    if (b_buyposs.Length != 0 && this.AveragePrice(b_buylabel) > goalprice)
                    {
                        long volume = this.MartingaleLot(b_buylabel, goalprice);
                        if (volume > balancelot - this.TotalLots(b_buylabel))
                            volume = (long)(balancelot - this.TotalLots(b_buylabel));
                        ExecuteMarketOrder(TradeType.Buy, Symbol, volume, b_buylabel);
                        Open_Consolidation = 0;
                        return;
                    }
                }
            }
            #endregion
        }
    }
}
