/////////////////////////////////////////////////////
//                                                 //
//  This Robot is created with Bot Studio          //
//  Website: http://forexsb.com/bot-studio.php     //
//  Support: http://forexsb.com/forum              //
//                                                 //
/////////////////////////////////////////////////////

using System;
using System.Linq;
using cAlgo;
using cAlgo.API;
using BotStudio.Core;
using BotStudio.Entities;
using BotStudio.Enums;
using BotStudio.Interfaces;
using BotStudio.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class BfRobot : Robot
    {
        [Parameter("--- Robot Input ---", DefaultValue = "----")]
        public string RobotInput { get; set; }

        [Parameter("Quantity (lots)", DefaultValue = 5.0, MinValue = 0.01, Step = 0.01)]
        public double Quantity { get; set; }

        [Parameter("Stop Loss (pips)", DefaultValue = 635)]
        public int StopLossInPips { get; set; }

        [Parameter("Take Profit (pips)", DefaultValue = 985)]
        public int TakeProfitInPips { get; set; }

        [Parameter("Robot Id", DefaultValue = "56477012")]
        public string RobotId { get; set; }

        [Parameter("Bars Required", DefaultValue = 200, MinValue = 200)]
        public int BarsRequired { get; set; }

        [Parameter("--- Commodity Channel Index ---", DefaultValue = "----")]
        public string Indicator0 { get; set; }

        [Parameter("Smoothing period", DefaultValue = 46, MinValue = 2, MaxValue = 200, Step = 1)]
        public int Indicator0Param0 { get; set; }

        [Parameter("Level", DefaultValue = 100, MinValue = -1000, MaxValue = 1000, Step = 1)]
        public int Indicator0Param1 { get; set; }

        [Parameter("Multiplier", DefaultValue = 0.083, MinValue = 0.0, MaxValue = 1.0, Step = 0.001)]
        public double Indicator0Param2 { get; set; }

        [Parameter("--- Moving Averages Crossover ---", DefaultValue = "----")]
        public string Indicator1 { get; set; }

        [Parameter("Fast MA period", DefaultValue = 58, MinValue = 1, MaxValue = 200, Step = 1)]
        public int Indicator1Param0 { get; set; }

        [Parameter("Slow MA period", DefaultValue = 14, MinValue = 1, MaxValue = 200, Step = 1)]
        public int Indicator1Param1 { get; set; }

        [Parameter("Fast MA shift", DefaultValue = 79, MinValue = 0, MaxValue = 100, Step = 1)]
        public int Indicator1Param2 { get; set; }

        [Parameter("Slow MA shift", DefaultValue = 39, MinValue = 0, MaxValue = 100, Step = 1)]
        public int Indicator1Param3 { get; set; }


        protected override void OnStart()
        {
            if (!InitBotStudio())
                Stop();
        }

        protected override void OnBar()
        {
            if (MarketSeries.Close.Count < BarsRequired)
                return;

            IDataSet dataSet = GetDataSet();
            Position position = Positions.Find(RobotId, Symbol);
            if (position != null)
            {
                TradeDirection tradeDirection = trader.CalculateExitSignals(dataSet);
                if (position.TradeType == TradeType.Buy && (tradeDirection == TradeDirection.Short || tradeDirection == TradeDirection.Both))
                    ClosePosition(position);
                else if (position.TradeType == TradeType.Sell && (tradeDirection == TradeDirection.Long || tradeDirection == TradeDirection.Both))
                    ClosePosition(position);
            }

            position = Positions.Find(RobotId, Symbol);
            if (position == null)
            {
                long tradeVolume = Symbol.QuantityToVolume(Quantity);
                TradeDirection tradeDirection = trader.CalculateEntrySignals(dataSet);
                if (tradeDirection == TradeDirection.Long)
                    ExecuteMarketOrder(TradeType.Buy, Symbol, tradeVolume, RobotId, StopLossInPips, TakeProfitInPips);
                else if (tradeDirection == TradeDirection.Short)
                    ExecuteMarketOrder(TradeType.Sell, Symbol, tradeVolume, RobotId, StopLossInPips, TakeProfitInPips);
            }
        }

        private ITrader trader;
        private DataPeriod period;

        private bool InitBotStudio()
        {
            if (MarketSeries.TimeFrame == TimeFrame.Minute)
                period = DataPeriod.M1;
            else if (MarketSeries.TimeFrame == TimeFrame.Minute5)
                period = DataPeriod.M5;
            else if (MarketSeries.TimeFrame == TimeFrame.Minute15)
                period = DataPeriod.M15;
            else if (MarketSeries.TimeFrame == TimeFrame.Minute30)
                period = DataPeriod.M30;
            else if (MarketSeries.TimeFrame == TimeFrame.Hour)
                period = DataPeriod.H1;
            else if (MarketSeries.TimeFrame == TimeFrame.Hour4)
                period = DataPeriod.H4;
            else if (MarketSeries.TimeFrame == TimeFrame.Daily)
                period = DataPeriod.D1;
            else
            {
                Print("This Robot works only with the following time frames: M1, M5, M15, M30, H1, H4, D1.");
                return false;
            }

            IStrategyManager strategyManager = new StrategyManager();
            IStrategy strategy = strategyManager.CreateStrategy(this);
            trader = new Trader(strategy);

            IDataSet dataSet = GetDataSet();
            BarsRequired = strategy.GetRequiredBars(dataSet);

            int barsAvailable = MarketSeries.Close.Count;
            if (barsAvailable < BarsRequired)
                Print("The Robot needs minimum {0} bars, but only {1} bars are available. The first {2} bars will be skipped.", BarsRequired, barsAvailable, BarsRequired - barsAvailable);

            return true;
        }

        private IDataSet GetDataSet()
        {
            IDataSet dataSet = new DataSet(MarketSeries.SymbolCode, period, BarsRequired);
            int startBar = MarketSeries.Close.Count - BarsRequired;
            for (int index = 0; index < BarsRequired; index++)
            {
                Bar bar = new Bar();
                bar.Time = MarketSeries.OpenTime[startBar + index];
                bar.Open = MarketSeries.Open[startBar + index];
                bar.High = MarketSeries.High[startBar + index];
                bar.Low = MarketSeries.Low[startBar + index];
                bar.Close = MarketSeries.Close[startBar + index];
                bar.Volume = (int)MarketSeries.TickVolume[startBar + index];
                dataSet.UpdateBar(index, bar);
            }

            dataSet.Bid = Symbol.Bid;
            dataSet.Ask = Symbol.Ask;
            dataSet.ServerTime = Server.Time;
            dataSet.Point = Symbol.TickSize;
            dataSet.Digits = Symbol.Digits;

            return dataSet;
        }
    }
}

namespace BotStudio.Entities
{
    public struct Bar
    {
        public DateTime Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public int Volume { get; set; }
    }

    public class CheckParam
    {
        public CheckParam()
        {
            Caption = String.Empty;
            Enabled = false;
            Checked = false;
            ToolTip = String.Empty;
        }

        public string Caption { get; set; }
        public bool Checked { get; set; }
        public bool Enabled { get; set; }
        public string ToolTip { get; set; }
    }

    public class DataSet : IDataSet
    {
        public DataSet(string symbol, DataPeriod period, int bars)
        {
            Bars = bars;
            Time = new DateTime[bars];
            Open = new double[bars];
            High = new double[bars];
            Low = new double[bars];
            Close = new double[bars];
            Volume = new int[bars];

            Symbol = symbol;
            Period = period;
        }

        public string Symbol { get; private set; }
        public DataPeriod Period { get; private set; }
        public double Point { get; set; }
        public int Digits { get; set; }
        public int Bars { get; private set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public DateTime ServerTime { get; set; }

        public DateTime[] Time { get; private set; }
        public double[] Open { get; private set; }
        public double[] High { get; private set; }
        public double[] Low { get; private set; }
        public double[] Close { get; private set; }
        public int[] Volume { get; private set; }

        public void UpdateBar(int index, Bar bar)
        {
            if (index >= Bars)
                throw new IndexOutOfRangeException("index");

            Time[index] = bar.Time;
            Open[index] = bar.Open;
            High[index] = bar.High;
            Low[index] = bar.Low;
            Close[index] = bar.Close;
            Volume[index] = bar.Volume;
        }
    }

    public class IndicatorComp
    {
        public IndicatorComp()
        {
            CompName = "Not defined";
            DataType = IndComponentType.NotDefined;
            ChartType = IndChartType.NoChart;
            ChartColor = Colors.Red;
            FirstBar = 0;
            UsePreviousBar = 0;
            ShowInDynInfo = true;
            Value = new double[0];
        }

        public string CompName { get; set; }
        public IndComponentType DataType { get; set; }
        public IndChartType ChartType { get; set; }
        public Colors ChartColor { get; set; }
        public int FirstBar { get; set; }
        public int UsePreviousBar { get; set; }
        public bool ShowInDynInfo { get; set; }
        public double[] Value { get; set; }
    }

    public class IndicatorParam : IIndicatorParam
    {
        public IndicatorParam()
        {
            SlotNumber = 0;
            IsDefined = false;
            SlotType = SlotTypes.NotDefined;
            IndicatorName = String.Empty;
            ListParam = new ListParam[5];
            NumParam = new NumericParam[6];
            CheckParam = new CheckParam[2];

            for (int i = 0; i < 5; i++)
                ListParam[i] = new ListParam();

            for (int i = 0; i < 6; i++)
                NumParam[i] = new NumericParam();

            for (int i = 0; i < 2; i++)
                CheckParam[i] = new CheckParam();
        }

        private int SlotNumber { get; set; }
        private bool IsDefined { get; set; }
        public SlotTypes SlotType { get; set; }
        public string IndicatorName { get; set; }
        public ListParam[] ListParam { get; private set; }
        public NumericParam[] NumParam { get; private set; }
        public CheckParam[] CheckParam { get; private set; }
    }

    public class ListParam
    {
        public ListParam()
        {
            Caption = String.Empty;
            ItemList = new[] 
            {
                ""
            };
            Index = 0;
            Text = String.Empty;
            Enabled = false;
            ToolTip = String.Empty;
        }

        public string Caption { get; set; }
        public string[] ItemList { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }
        public bool Enabled { get; set; }
        public string ToolTip { get; set; }
    }

    public class NumericParam
    {
        public NumericParam()
        {
            Caption = String.Empty;
            Value = 0;
            Min = 0;
            Max = 100;
            Point = 0;
            Enabled = false;
            ToolTip = String.Empty;
        }

        public string Caption { get; set; }
        public double Value { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public int Point { get; set; }
        public bool Enabled { get; set; }
        public string ToolTip { get; set; }
    }
}

namespace BotStudio.Enums
{
    public enum BandIndLogic
    {
        The_bar_opens_below_the_Upper_Band,
        The_bar_opens_above_the_Upper_Band,
        The_bar_opens_below_the_Lower_Band,
        The_bar_opens_above_the_Lower_Band,
        The_bar_opens_below_the_Upper_Band_after_opening_above_it,
        The_bar_opens_above_the_Upper_Band_after_opening_below_it,
        The_bar_opens_below_the_Lower_Band_after_opening_above_it,
        The_bar_opens_above_the_Lower_Band_after_opening_below_it,
        It_does_not_act_as_a_filter
    }

    public enum BasePrice
    {
        Open,
        High,
        Low,
        Close,
        Median,
        Typical,
        Weighted
    }

    public enum DataPeriod
    {
        M1 = 1,
        M5 = 5,
        M15 = 15,
        M30 = 30,
        H1 = 60,
        H4 = 240,
        D1 = 1440
    }

    public enum IndChartType
    {
        NoChart,
        Line,
        Dot,
        Histogram,
        Level,
        CloudUp,
        CloudDown
    }

    public enum IndComponentType
    {
        NotDefined,
        IndicatorValue,
        AllowOpenLong,
        AllowOpenShort,
        ForceCloseLong,
        ForceCloseShort,
        ForceClose,
        Other
    }

    public enum IndicatorLogic
    {
        The_indicator_rises,
        The_indicator_falls,
        The_indicator_is_higher_than_the_level_line,
        The_indicator_is_lower_than_the_level_line,
        The_indicator_crosses_the_level_line_upward,
        The_indicator_crosses_the_level_line_downward,
        The_indicator_changes_its_direction_upward,
        The_indicator_changes_its_direction_downward,
        It_does_not_act_as_a_filter
    }

    public enum MAMethod
    {
        Simple,
        Weighted,
        Exponential,
        Smoothed
    }

    [Flags()]
    public enum SlotTypes : short
    {
        NotDefined = 0,
        OpenFilter = 2,
        CloseFilter = 8
    }

    public enum TradeDirection
    {
        None,
        Long,
        Short,
        Both
    }
}

namespace BotStudio.Interfaces
{
    public interface IDataSet
    {
        string Symbol { get; }
        DataPeriod Period { get; }
        double Point { get; set; }
        int Digits { get; set; }

        int Bars { get; }
        DateTime[] Time { get; }
        double[] Open { get; }
        double[] High { get; }
        double[] Low { get; }
        double[] Close { get; }
        int[] Volume { get; }

        double Bid { get; set; }
        double Ask { get; set; }
        DateTime ServerTime { get; set; }

        void UpdateBar(int index, Bar bar);
    }

    public interface IBfIndicator
    {
        string IndicatorName { get; }
        string IndicatorVersion { get; set; }
        string IndicatorAuthor { get; set; }
        string IndicatorDescription { get; set; }

        SlotTypes SlotType { get; }
        IIndicatorParam IndParam { get; set; }
        IndicatorComp[] Component { get; }

        bool SeparatedChart { get; }
        double[] SpecialValues { get; }
        double SeparatedChartMinValue { get; }
        double SeparatedChartMaxValue { get; }

        IDataSet DataSet { get; set; }

        void Initialize(SlotTypes slotType);
        void Calculate(IDataSet dataSet);
    }

    public interface IIndicatorParam
    {
        string IndicatorName { get; set; }

        SlotTypes SlotType { get; set; }
        ListParam[] ListParam { get; }
        NumericParam[] NumParam { get; }
        CheckParam[] CheckParam { get; }
    }

    public interface IStrategy
    {
        IBfIndicator[] OpenFilter { get; set; }
        IBfIndicator[] CloseFilter { get; set; }
        int GetRequiredBars(IDataSet dataSet);
    }

    public interface IStrategyManager
    {
        IStrategy CreateStrategy(BfRobot robot);
    }

    public interface ITrader
    {
        TradeDirection CalculateEntrySignals(IDataSet dataSet);
        TradeDirection CalculateExitSignals(IDataSet dataSet);
    }
}

namespace BotStudio.Indicators
{
    public partial class Indicator : IBfIndicator
    {
        private string indicatorName;
        private SlotTypes slotType;

        public Indicator()
        {
            IndParam = new IndicatorParam();

            IndicatorName = string.Empty;
            PossibleSlots = SlotTypes.NotDefined;
            SlotType = SlotTypes.NotDefined;

            SeparatedChart = false;
            SeparatedChartMinValue = double.MaxValue;
            SeparatedChartMaxValue = double.MinValue;
            SpecialValues = new double[0];

            IsDiscreteValues = false;
            CustomIndicator = false;
            IsBacktester = true;
            WarningMessage = string.Empty;
            AllowClosingFilters = false;

            IndicatorAuthor = "Forex Software Ltd";
            IndicatorVersion = "1.0";

            Component = new IndicatorComp[] 
            {
                            };
        }

        protected SlotTypes PossibleSlots { private get; set; }
        protected bool IsDiscreteValues { private get; set; }
        protected DataPeriod Period
        {
            get { return DataSet.Period; }
        }
        protected double Point
        {
            get { return DataSet.Point; }
        }
        protected int Digits
        {
            get { return DataSet.Digits; }
        }
        protected int Bars
        {
            get { return DataSet.Bars; }
        }
        protected DateTime[] Time
        {
            get { return DataSet.Time; }
        }
        protected double[] Open
        {
            get { return DataSet.Open; }
        }
        protected double[] High
        {
            get { return DataSet.High; }
        }
        protected double[] Low
        {
            get { return DataSet.Low; }
        }
        protected double[] Close
        {
            get { return DataSet.Close; }
        }
        protected int[] Volume
        {
            get { return DataSet.Volume; }
        }
        protected DateTime ServerTime
        {
            get { return DataSet.ServerTime; }
        }
        public IDataSet DataSet { get; set; }
        public string IndicatorName
        {
            get { return indicatorName; }
            protected set
            {
                indicatorName = value;
                IndParam.IndicatorName = value;
            }
        }
        public IIndicatorParam IndParam { get; set; }
        public SlotTypes SlotType
        {
            get { return slotType; }
            protected set
            {
                slotType = value;
                IndParam.SlotType = value;
            }
        }
        public bool SeparatedChart { get; protected set; }
        public IndicatorComp[] Component { get; protected set; }
        public double[] SpecialValues { get; protected set; }
        public double SeparatedChartMinValue { get; protected set; }
        public double SeparatedChartMaxValue { get; protected set; }
        public bool CustomIndicator { get; set; }
        public string WarningMessage { get; protected set; }
        public bool AllowClosingFilters { get; protected set; }
        public string IndicatorVersion { get; set; }
        public string IndicatorAuthor { get; set; }
        public string IndicatorDescription { get; set; }
        public bool IsBacktester { get; set; }

        public virtual void Initialize(SlotTypes slotType)
        {
        }

        public virtual void Calculate(IDataSet dataSet)
        {
        }

        protected double[] Price(BasePrice priceType)
        {
            var price = new double[Bars];

            switch (priceType)
            {
                case BasePrice.Open:
                    price = Open;
                    break;
                case BasePrice.High:
                    price = High;
                    break;
                case BasePrice.Low:
                    price = Low;
                    break;
                case BasePrice.Close:
                    price = Close;
                    break;
                case BasePrice.Median:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar]) / 2;
                    break;
                case BasePrice.Typical:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar] + Close[bar]) / 3;
                    break;
                case BasePrice.Weighted:
                    for (int bar = 0; bar < Bars; bar++)
                        price[bar] = (Low[bar] + High[bar] + 2 * Close[bar]) / 4;
                    break;
            }
            return price;
        }

        protected double[] MovingAverage(int period, int shift, MAMethod maMethod, double[] source)
        {
            var movingAverage = new double[Bars];

            if (period <= 1 && shift == 0)
                return source;

            if (period > Bars || period + shift <= 0 || period + shift > Bars)
            {
                return movingAverage;
            }

            for (int bar = 0; bar < period + shift - 1; bar++)
                movingAverage[bar] = 0;

            double sum = 0;
            for (int bar = shift; bar < period + shift; bar++)
                sum += source[bar];

            movingAverage[period + shift - 1] = sum / period;
            int lastBar = Math.Min(Bars, Bars - shift);

            switch (maMethod)
            {
                case MAMethod.Simple:
                    for (int bar = period; bar < lastBar; bar++)
                        movingAverage[bar + shift] = movingAverage[bar + shift - 1] + source[bar] / period - source[bar - period] / period;
                    break;
                case MAMethod.Exponential:
                    {
                        double pr = 2.0 / (period + 1);
                        for (int bar = period; bar < lastBar; bar++)
                            movingAverage[bar + shift] = source[bar] * pr + movingAverage[bar + shift - 1] * (1 - pr);
                    }
                    break;
                case MAMethod.Weighted:
                    {
                        double weight = period * (period + 1) / 2.0;

                        for (int bar = period; bar < lastBar; bar++)
                        {
                            sum = 0;
                            for (int i = 0; i < period; i++)
                                sum += source[bar - i] * (period - i);
                            movingAverage[bar + shift] = sum / weight;
                        }
                    }
                    break;
                case MAMethod.Smoothed:
                    for (int bar = period; bar < lastBar; bar++)
                        movingAverage[bar + shift] = (movingAverage[bar + shift - 1] * (period - 1) + source[bar]) / period;
                    break;
            }

            for (int bar = Bars + shift; bar < Bars; bar++)
                movingAverage[bar] = 0;

            return movingAverage;
        }

        protected static double Sigma
        {
            get { return 1E-07; }
        }

        protected void OscillatorLogic(int firstBar, int previous, double[] adIndValue, double levelLong, double levelShort, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort, IndicatorLogic indLogic)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case IndicatorLogic.The_indicator_rises:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - previous;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                        if (!IsDiscreteValues)
                        {
                            bool isNoChange = true;
                            while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                            {
                                isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                                baseBar--;
                            }
                        }

                        indCompLong.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_falls:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - previous;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                        if (!IsDiscreteValues)
                        {
                            bool isNoChange = true;
                            while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                            {
                                isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                                baseBar--;
                            }
                        }

                        indCompLong.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_higher_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = adIndValue[bar - previous] > levelLong + sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[bar - previous] < levelShort - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_lower_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = adIndValue[bar - previous] < levelLong - sigma ? 1 : 0;
                        indCompShort.Value[bar] = adIndValue[bar - previous] > levelShort + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - previous - 1;
                        while (Math.Abs(adIndValue[baseBar] - levelLong) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = (adIndValue[baseBar] < levelLong - sigma && adIndValue[bar - previous] > levelLong + sigma) ? 1 : 0;
                        indCompShort.Value[bar] = (adIndValue[baseBar] > levelShort + sigma && adIndValue[bar - previous] < levelShort - sigma) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - previous - 1;
                        while (Math.Abs(adIndValue[baseBar] - levelLong) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = (adIndValue[baseBar] > levelLong + sigma && adIndValue[bar - previous] < levelLong - sigma) ? 1 : 0;
                        indCompShort.Value[bar] = (adIndValue[baseBar] < levelShort - sigma && adIndValue[bar - previous] > levelShort + sigma) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - previous;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int iBar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[iBar2]) < sigma && iBar2 > firstBar)
                        {
                            iBar2--;
                        }

                        indCompLong.Value[bar] = (adIndValue[iBar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                        indCompShort.Value[bar] = (adIndValue[iBar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - previous;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int iBar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[iBar2]) < sigma && iBar2 > firstBar)
                        {
                            iBar2--;
                        }

                        indCompLong.Value[bar] = (adIndValue[iBar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                        indCompShort.Value[bar] = (adIndValue[iBar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                    }
                    break;
                default:

                    return;
            }
        }

        protected void NoDirectionOscillatorLogic(int firstBar, int previous, double[] adIndValue, double dLevel, ref IndicatorComp indComp, IndicatorLogic indLogic)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case IndicatorLogic.The_indicator_rises:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - previous;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];
                        bool isNoChange = true;

                        while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                        {
                            isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                            baseBar--;
                        }

                        indComp.Value[bar] = adIndValue[baseBar] < adIndValue[currentBar] - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_falls:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int currentBar = bar - previous;
                        int baseBar = currentBar - 1;
                        bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];
                        bool isNoChange = true;

                        while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                        {
                            isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                            baseBar--;
                        }

                        indComp.Value[bar] = adIndValue[baseBar] > adIndValue[currentBar] + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_higher_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indComp.Value[bar] = adIndValue[bar - previous] > dLevel + sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_is_lower_than_the_level_line:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indComp.Value[bar] = adIndValue[bar - previous] < dLevel - sigma ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - previous - 1;
                        while (Math.Abs(adIndValue[baseBar] - dLevel) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indComp.Value[bar] = (adIndValue[baseBar] < dLevel - sigma && adIndValue[bar - previous] > dLevel + sigma) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_crosses_the_level_line_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - previous - 1;
                        while (Math.Abs(adIndValue[baseBar] - dLevel) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indComp.Value[bar] = (adIndValue[baseBar] > dLevel + sigma && adIndValue[bar - previous] < dLevel - sigma) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_upward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - previous;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                        {
                            bar2--;
                        }

                        indComp.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                    }
                    break;

                case IndicatorLogic.The_indicator_changes_its_direction_downward:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int bar0 = bar - previous;
                        int bar1 = bar0 - 1;
                        while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                        {
                            bar1--;
                        }

                        int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                        while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                        {
                            bar2--;
                        }

                        indComp.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                    }
                    break;
                default:
                    return;
            }
        }

        protected void BandIndicatorLogic(int firstBar, int previous, double[] adUpperBand, double[] adLowerBand, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort, BandIndLogic indLogic)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            switch (indLogic)
            {
                case BandIndLogic.The_bar_opens_below_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] < adUpperBand[bar - previous] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] > adLowerBand[bar - previous] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Upper_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] > adUpperBand[bar - previous] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] < adLowerBand[bar - previous] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] < adLowerBand[bar - previous] - sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] > adUpperBand[bar - previous] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Lower_Band:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        indCompLong.Value[bar] = Open[bar] > adLowerBand[bar - previous] + sigma ? 1 : 0;
                        indCompShort.Value[bar] = Open[bar] < adUpperBand[bar - previous] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Upper_Band_after_opening_above_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] < adUpperBand[bar - previous] - sigma && Open[baseBar] > adUpperBand[baseBar - previous] + sigma ? 1 : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] > adLowerBand[bar - previous] + sigma && Open[baseBar] < adLowerBand[baseBar - previous] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Upper_Band_after_opening_below_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] > adUpperBand[bar - previous] + sigma && Open[baseBar] < adUpperBand[baseBar - previous] - sigma ? 1 : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] < adLowerBand[bar - previous] - sigma && Open[baseBar] > adLowerBand[baseBar - previous] + sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_below_the_Lower_Band_after_opening_above_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] < adLowerBand[bar - previous] - sigma && Open[baseBar] > adLowerBand[baseBar - previous] + sigma ? 1 : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] > adUpperBand[bar - previous] + sigma && Open[baseBar] < adUpperBand[baseBar - previous] - sigma ? 1 : 0;
                    }
                    break;

                case BandIndLogic.The_bar_opens_above_the_Lower_Band_after_opening_below_it:
                    for (int bar = firstBar; bar < Bars; bar++)
                    {
                        int baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adLowerBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompLong.Value[bar] = Open[bar] > adLowerBand[bar - previous] + sigma && Open[baseBar] < adLowerBand[baseBar - previous] - sigma ? 1 : 0;

                        baseBar = bar - 1;
                        while (Math.Abs(Open[baseBar] - adUpperBand[baseBar - previous]) < sigma && baseBar > firstBar)
                        {
                            baseBar--;
                        }

                        indCompShort.Value[bar] = Open[bar] < adUpperBand[bar - previous] - sigma && Open[baseBar] > adUpperBand[baseBar - previous] + sigma ? 1 : 0;
                    }
                    break;
                default:
                    return;
            }
        }

        protected void IndicatorRisesLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                int baseBar = currentBar - 1;
                bool isNoChange = true;
                bool isHigher = adIndValue[currentBar] > adIndValue[baseBar];

                while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                {
                    isNoChange = (isHigher == (adIndValue[baseBar + 1] > adIndValue[baseBar]));
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] > adIndValue[baseBar] + sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adIndValue[baseBar] - sigma ? 1 : 0;
            }
        }

        protected void IndicatorFallsLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                int baseBar = currentBar - 1;
                bool isNoChange = true;
                bool isLower = adIndValue[currentBar] < adIndValue[baseBar];

                while (Math.Abs(adIndValue[currentBar] - adIndValue[baseBar]) < sigma && isNoChange && baseBar > firstBar)
                {
                    isNoChange = (isLower == (adIndValue[baseBar + 1] < adIndValue[baseBar]));
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] < adIndValue[baseBar] - sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adIndValue[baseBar] + sigma ? 1 : 0;
            }
        }

        protected void IndicatorIsHigherThanAnotherIndicatorLogic(int firstBar, int previous, double[] adIndValue, double[] adAnotherIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                indCompLong.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma ? 1 : 0;
            }
        }

        protected void IndicatorIsLowerThanAnotherIndicatorLogic(int firstBar, int previous, double[] adIndValue, double[] adAnotherIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                indCompLong.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma ? 1 : 0;
            }
        }

        protected void IndicatorCrossesAnotherIndicatorUpwardLogic(int firstBar, int previous, double[] adIndValue, double[] adAnotherIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                int baseBar = currentBar - 1;
                while (Math.Abs(adIndValue[baseBar] - adAnotherIndValue[baseBar]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma && adIndValue[baseBar] < adAnotherIndValue[baseBar] - sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma && adIndValue[baseBar] > adAnotherIndValue[baseBar] + sigma ? 1 : 0;
            }
        }

        protected void IndicatorChangesItsDirectionUpward(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            for (int bar = firstBar; bar < Bars; bar++)
            {
                int bar0 = bar - previous;
                int bar1 = bar0 - 1;
                while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                {
                    bar1--;
                }

                int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                {
                    bar2--;
                }

                indCompLong.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                indCompShort.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
            }
        }

        protected void IndicatorChangesItsDirectionDownward(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            for (int bar = firstBar; bar < Bars; bar++)
            {
                int bar0 = bar - previous;
                int bar1 = bar0 - 1;
                while (Math.Abs(adIndValue[bar0] - adIndValue[bar1]) < sigma && bar1 > firstBar)
                {
                    bar1--;
                }

                int bar2 = bar1 - 1 > firstBar ? bar1 - 1 : firstBar;
                while (Math.Abs(adIndValue[bar1] - adIndValue[bar2]) < sigma && bar2 > firstBar)
                {
                    bar2--;
                }

                indCompLong.Value[bar] = (adIndValue[bar2] < adIndValue[bar1] && adIndValue[bar1] > adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
                indCompShort.Value[bar] = (adIndValue[bar2] > adIndValue[bar1] && adIndValue[bar1] < adIndValue[bar0] && bar1 == bar0 - 1) ? 1 : 0;
            }
        }

        protected void IndicatorCrossesAnotherIndicatorDownwardLogic(int firstBar, int previous, double[] adIndValue, double[] adAnotherIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int currentBar = bar - previous;
                int baseBar = currentBar - 1;
                while (Math.Abs(adIndValue[baseBar] - adAnotherIndValue[baseBar]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = adIndValue[currentBar] < adAnotherIndValue[currentBar] - sigma && adIndValue[baseBar] > adAnotherIndValue[baseBar] + sigma ? 1 : 0;
                indCompShort.Value[bar] = adIndValue[currentBar] > adAnotherIndValue[currentBar] + sigma && adIndValue[baseBar] < adAnotherIndValue[baseBar] - sigma ? 1 : 0;
            }
        }

        protected void BarOpensAboveIndicatorLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Open[bar] > adIndValue[bar - previous] + sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] < adIndValue[bar - previous] - sigma ? 1 : 0;
            }
        }

        protected void BarOpensBelowIndicatorLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                indCompLong.Value[bar] = Open[bar] < adIndValue[bar - previous] - sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] > adIndValue[bar - previous] + sigma ? 1 : 0;
            }
        }

        protected void BarOpensAboveIndicatorAfterOpeningBelowLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int baseBar = bar - 1;
                while (Math.Abs(Open[baseBar] - adIndValue[baseBar - previous]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = Open[bar] > adIndValue[bar - previous] + sigma && Open[baseBar] < adIndValue[baseBar - previous] - sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] < adIndValue[bar - previous] - sigma && Open[baseBar] > adIndValue[baseBar - previous] + sigma ? 1 : 0;
            }
        }

        protected void BarOpensBelowIndicatorAfterOpeningAboveLogic(int firstBar, int previous, double[] adIndValue, ref IndicatorComp indCompLong, ref IndicatorComp indCompShort)
        {
            double sigma = 1E-07;
            firstBar = Math.Max(firstBar, 2);

            for (int bar = firstBar; bar < Bars; bar++)
            {
                int baseBar = bar - 1;
                while (Math.Abs(Open[baseBar] - adIndValue[baseBar - previous]) < sigma && baseBar > firstBar)
                {
                    baseBar--;
                }

                indCompLong.Value[bar] = Open[bar] < adIndValue[bar - previous] - sigma && Open[baseBar] > adIndValue[baseBar - previous] + sigma ? 1 : 0;
                indCompShort.Value[bar] = Open[bar] > adIndValue[bar - previous] + sigma && Open[baseBar] < adIndValue[baseBar - previous] - sigma ? 1 : 0;
            }
        }
    }
}

namespace BotStudio.Core
{
    public class Strategy : IStrategy
    {
        public IBfIndicator[] OpenFilter { get; set; }
        public IBfIndicator[] CloseFilter { get; set; }

        public int GetRequiredBars(IDataSet dataSet)
        {
            int firstBar = 2;
            foreach (IBfIndicator indicator in OpenFilter)
            {
                indicator.Calculate(dataSet);
                foreach (IndicatorComp component in indicator.Component)
                    if (component.FirstBar > firstBar)
                        firstBar = component.FirstBar;
            }

            foreach (IBfIndicator indicator in CloseFilter)
            {
                indicator.Calculate(dataSet);
                foreach (IndicatorComp component in indicator.Component)
                    if (component.FirstBar > firstBar)
                        firstBar = component.FirstBar;
            }

            return (int)Math.Ceiling(firstBar * 1.3 / 100) * 100;
        }
    }

    public class StrategyManager : IStrategyManager
    {
        public IStrategy CreateStrategy(BfRobot robot)
        {
            Strategy strategy = new Strategy();
            strategy.OpenFilter = new BotStudio.Indicators.Indicator[1];
            strategy.CloseFilter = new BotStudio.Indicators.Indicator[1];

            strategy.OpenFilter[0] = new BotStudio.Indicators.Store.CommodityChannelIndex();
            strategy.OpenFilter[0].Initialize(SlotTypes.OpenFilter);

            strategy.OpenFilter[0].IndParam.ListParam[0].Enabled = true;
            strategy.OpenFilter[0].IndParam.ListParam[0].Caption = "Logic";
            strategy.OpenFilter[0].IndParam.ListParam[0].Index = 6;
            strategy.OpenFilter[0].IndParam.ListParam[0].Text = "CCI changes its direction upward";

            strategy.OpenFilter[0].IndParam.ListParam[1].Enabled = true;
            strategy.OpenFilter[0].IndParam.ListParam[1].Caption = "Smoothing method";
            strategy.OpenFilter[0].IndParam.ListParam[1].Index = 0;
            strategy.OpenFilter[0].IndParam.ListParam[1].Text = "Simple";

            strategy.OpenFilter[0].IndParam.ListParam[2].Enabled = true;
            strategy.OpenFilter[0].IndParam.ListParam[2].Caption = "Base price";
            strategy.OpenFilter[0].IndParam.ListParam[2].Index = 5;
            strategy.OpenFilter[0].IndParam.ListParam[2].Text = "Typical";

            strategy.OpenFilter[0].IndParam.NumParam[0].Enabled = true;
            strategy.OpenFilter[0].IndParam.NumParam[0].Caption = "Smoothing period";
            strategy.OpenFilter[0].IndParam.NumParam[0].Value = robot.Indicator0Param0;

            strategy.OpenFilter[0].IndParam.NumParam[1].Enabled = true;
            strategy.OpenFilter[0].IndParam.NumParam[1].Caption = "Level";
            strategy.OpenFilter[0].IndParam.NumParam[1].Value = robot.Indicator0Param1;

            strategy.OpenFilter[0].IndParam.NumParam[2].Enabled = true;
            strategy.OpenFilter[0].IndParam.NumParam[2].Caption = "Multiplier";
            strategy.OpenFilter[0].IndParam.NumParam[2].Value = robot.Indicator0Param2;

            strategy.OpenFilter[0].IndParam.CheckParam[0].Enabled = true;
            strategy.OpenFilter[0].IndParam.CheckParam[0].Caption = "Use previous bar value";
            strategy.OpenFilter[0].IndParam.CheckParam[0].Checked = true;

            strategy.CloseFilter[0] = new BotStudio.Indicators.Store.MovingAveragesCrossover();
            strategy.CloseFilter[0].Initialize(SlotTypes.CloseFilter);

            strategy.CloseFilter[0].IndParam.ListParam[0].Enabled = true;
            strategy.CloseFilter[0].IndParam.ListParam[0].Caption = "Logic";
            strategy.CloseFilter[0].IndParam.ListParam[0].Index = 1;
            strategy.CloseFilter[0].IndParam.ListParam[0].Text = "Fast MA crosses Slow MA downward";

            strategy.CloseFilter[0].IndParam.ListParam[1].Enabled = true;
            strategy.CloseFilter[0].IndParam.ListParam[1].Caption = "Base price";
            strategy.CloseFilter[0].IndParam.ListParam[1].Index = 3;
            strategy.CloseFilter[0].IndParam.ListParam[1].Text = "Close";

            strategy.CloseFilter[0].IndParam.ListParam[3].Enabled = true;
            strategy.CloseFilter[0].IndParam.ListParam[3].Caption = "Fast MA method";
            strategy.CloseFilter[0].IndParam.ListParam[3].Index = 0;
            strategy.CloseFilter[0].IndParam.ListParam[3].Text = "Simple";

            strategy.CloseFilter[0].IndParam.ListParam[4].Enabled = true;
            strategy.CloseFilter[0].IndParam.ListParam[4].Caption = "Slow MA method";
            strategy.CloseFilter[0].IndParam.ListParam[4].Index = 0;
            strategy.CloseFilter[0].IndParam.ListParam[4].Text = "Simple";

            strategy.CloseFilter[0].IndParam.NumParam[0].Enabled = true;
            strategy.CloseFilter[0].IndParam.NumParam[0].Caption = "Fast MA period";
            strategy.CloseFilter[0].IndParam.NumParam[0].Value = robot.Indicator1Param0;

            strategy.CloseFilter[0].IndParam.NumParam[1].Enabled = true;
            strategy.CloseFilter[0].IndParam.NumParam[1].Caption = "Slow MA period";
            strategy.CloseFilter[0].IndParam.NumParam[1].Value = robot.Indicator1Param1;

            strategy.CloseFilter[0].IndParam.NumParam[2].Enabled = true;
            strategy.CloseFilter[0].IndParam.NumParam[2].Caption = "Fast MA shift";
            strategy.CloseFilter[0].IndParam.NumParam[2].Value = robot.Indicator1Param2;

            strategy.CloseFilter[0].IndParam.NumParam[3].Enabled = true;
            strategy.CloseFilter[0].IndParam.NumParam[3].Caption = "Slow MA shift";
            strategy.CloseFilter[0].IndParam.NumParam[3].Value = robot.Indicator1Param3;

            strategy.CloseFilter[0].IndParam.CheckParam[0].Enabled = true;
            strategy.CloseFilter[0].IndParam.CheckParam[0].Caption = "Use previous bar value";
            strategy.CloseFilter[0].IndParam.CheckParam[0].Checked = true;

            return strategy;
        }
    }

    public class Trader : ITrader
    {
        readonly IStrategy strategy;

        public Trader(IStrategy strategy)
        {
            this.strategy = strategy;
        }

        public TradeDirection CalculateEntrySignals(IDataSet dataSet)
        {
            bool canOpenLong = true;
            bool canOpenShort = true;
            foreach (IBfIndicator indicator in strategy.OpenFilter)
            {
                indicator.Calculate(dataSet);
                int bar = dataSet.Bars - 1;
                foreach (IndicatorComp component in indicator.Component)
                {
                    if (component.DataType == IndComponentType.AllowOpenLong && component.Value[bar] < 0.5)
                        canOpenLong = false;

                    if (component.DataType == IndComponentType.AllowOpenShort && component.Value[bar] < 0.5)
                        canOpenShort = false;
                }
            }

            TradeDirection tradeDirection = TradeDirection.None;
            if (canOpenLong && canOpenShort)
                tradeDirection = TradeDirection.Both;
            else if (canOpenLong)
                tradeDirection = TradeDirection.Long;
            else if (canOpenShort)
                tradeDirection = TradeDirection.Short;

            return tradeDirection;
        }

        public TradeDirection CalculateExitSignals(IDataSet dataSet)
        {
            bool canCloseLong = strategy.CloseFilter.Length == 0;
            bool canCloseShort = strategy.CloseFilter.Length == 0;

            foreach (IBfIndicator indicator in strategy.CloseFilter)
            {
                indicator.Calculate(dataSet);
                int bar = dataSet.Bars - 1;
                foreach (var component in indicator.Component)
                {
                    if (component.DataType == IndComponentType.ForceCloseLong && component.Value[bar] > 0.5)
                        canCloseLong = true;

                    if (component.DataType == IndComponentType.ForceCloseShort && component.Value[bar] > 0.5)
                        canCloseShort = true;
                }
            }

            var tradeDirection = TradeDirection.None;
            if (canCloseLong && canCloseShort)
                tradeDirection = TradeDirection.Both;
            else if (canCloseLong)
                tradeDirection = TradeDirection.Short;
            else if (canCloseShort)
                tradeDirection = TradeDirection.Long;

            return tradeDirection;
        }
    }
}

namespace BotStudio.Indicators.Store
{
    public class CommodityChannelIndex : Indicator
    {
        public CommodityChannelIndex()
        {
            IndicatorName = "Commodity Channel Index";
            SeparatedChart = true;
            IndicatorAuthor = "Forex Software Ltd.";
            IndicatorVersion = "2.1";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] 
            {
                "CCI rises",
                "CCI falls",
                "CCI is higher than the Level line",
                "CCI is lower than the Level line",
                "CCI crosses the Level line upward",
                "CCI crosses the Level line downward",
                "CCI changes its direction upward",
                "CCI changes its direction downward"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[1].Index = (int)MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing CCI value.";

            IndParam.ListParam[2].Caption = "Base price";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[2].Index = (int)BasePrice.Typical;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The price the indicator is based on.";

            IndParam.NumParam[0].Caption = "Smoothing period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of smoothing used in the calculations.";

            IndParam.NumParam[1].Caption = "Level";
            IndParam.NumParam[1].Value = 100;
            IndParam.NumParam[1].Min = -1000;
            IndParam.NumParam[1].Max = 1000;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "A critical level (for the appropriate logic).";

            IndParam.NumParam[2].Caption = "Multiplier";
            IndParam.NumParam[2].Value = 0.015;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 1;
            IndParam.NumParam[2].Point = 3;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The multiplier value.";

            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            MAMethod maMethod = (MAMethod)IndParam.ListParam[1].Index;
            BasePrice basePrice = (BasePrice)IndParam.ListParam[2].Index;
            int period = (int)IndParam.NumParam[0].Value;
            double level = IndParam.NumParam[1].Value;
            double multiplier = IndParam.NumParam[2].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = period + 3;
            double[] price = Price(basePrice);
            double[] movingAverage = MovingAverage(period, 0, maMethod, price);

            double[] meanDev = new double[Bars];
            for (int bar = period; bar < Bars; bar++)
            {
                double sum = 0;
                for (int i = 0; i < period; i++)
                    sum += Math.Abs(price[bar - i] - movingAverage[bar]);
                meanDev[bar] = multiplier * sum / period;
            }

            double[] cci = new double[Bars];

            for (int bar = firstBar; bar < Bars; bar++)
            {
                if (Math.Abs(meanDev[bar] - 0) > Sigma)
                    cci[bar] = (price[bar] - movingAverage[bar]) / meanDev[bar];
                else
                    cci[bar] = 0;
            }

            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp 
            {
                CompName = "CCI",
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                ChartColor = Colors.RoyalBlue,
                FirstBar = firstBar,
                Value = cci
            };

            Component[1] = new IndicatorComp 
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[2] = new IndicatorComp 
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "CCI rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[] 
                    {
                        -100,
                        0,
                        100
                    };
                    break;
                case "CCI falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[] 
                    {
                        -100,
                        0,
                        100
                    };
                    break;
                case "CCI is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new[] 
                    {
                        level,
                        0,
                        -level
                    };
                    break;
                case "CCI is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new[] 
                    {
                        level,
                        0,
                        -level
                    };
                    break;
                case "CCI crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new[] 
                    {
                        level,
                        0,
                        -level
                    };
                    break;
                case "CCI crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new[] 
                    {
                        level,
                        0,
                        -level
                    };
                    break;
                case "CCI changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[] 
                    {
                        -100,
                        0,
                        100
                    };
                    break;
                case "CCI changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[] 
                    {
                        -100,
                        0,
                        100
                    };
                    break;
            }

            OscillatorLogic(firstBar, previous, cci, level, -level, ref Component[1], ref Component[2], indLogic);
        }
    }


    public class MovingAveragesCrossover : Indicator
    {
        public MovingAveragesCrossover()
        {
            IndicatorName = "Moving Averages Crossover";
            IndicatorAuthor = "Forex Software Ltd.";
            IndicatorVersion = "2.0";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[] 
            {
                "Fast MA crosses Slow MA upward",
                "Fast MA crosses Slow MA downward",
                "Fast MA is higher than Slow MA",
                "Fast MA is lower than Slow MA"
            };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index = (int)BasePrice.Close;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The price both Moving Averages are based on.";

            IndParam.ListParam[3].Caption = "Fast MA method";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[3].Index = (int)MAMethod.Simple;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "The method used for smoothing Fast Moving Averages.";

            IndParam.ListParam[4].Caption = "Slow MA method";
            IndParam.ListParam[4].ItemList = Enum.GetNames(typeof(MAMethod));
            IndParam.ListParam[4].Index = (int)MAMethod.Simple;
            IndParam.ListParam[4].Text = IndParam.ListParam[4].ItemList[IndParam.ListParam[4].Index];
            IndParam.ListParam[4].Enabled = true;
            IndParam.ListParam[4].ToolTip = "The method used for smoothing Slow Moving Averages.";

            IndParam.NumParam[0].Caption = "Fast MA period";
            IndParam.NumParam[0].Value = 13;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Fast MA.";

            IndParam.NumParam[1].Caption = "Slow MA period";
            IndParam.NumParam[1].Value = 21;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of Slow MA.";

            IndParam.NumParam[2].Caption = "Fast MA shift";
            IndParam.NumParam[2].Value = 0;
            IndParam.NumParam[2].Min = 0;
            IndParam.NumParam[2].Max = 100;
            IndParam.NumParam[2].Point = 0;
            IndParam.NumParam[2].Enabled = true;
            IndParam.NumParam[2].ToolTip = "The shifting value of Fast MA.";

            IndParam.NumParam[3].Caption = "Slow MA shift";
            IndParam.NumParam[3].Value = 0;
            IndParam.NumParam[3].Min = 0;
            IndParam.NumParam[3].Max = 100;
            IndParam.NumParam[3].Point = 0;
            IndParam.NumParam[3].Enabled = true;
            IndParam.NumParam[3].ToolTip = "The shifting value of Slow MA.";

            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            BasePrice basePrice = (BasePrice)IndParam.ListParam[1].Index;
            MAMethod fastMAMethod = (MAMethod)IndParam.ListParam[3].Index;
            MAMethod slowMAMethod = (MAMethod)IndParam.ListParam[4].Index;
            int periodFast = (int)IndParam.NumParam[0].Value;
            int periodSlow = (int)IndParam.NumParam[1].Value;
            int shiftFast = (int)IndParam.NumParam[2].Value;
            int shiftSlow = (int)IndParam.NumParam[3].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            int firstBar = Math.Max(periodFast + shiftFast, periodSlow + shiftSlow) + 2;
            double[] maFast = MovingAverage(periodFast, shiftFast, fastMAMethod, Price(basePrice));
            double[] maSlow = MovingAverage(periodSlow, shiftSlow, slowMAMethod, Price(basePrice));
            double[] maOscillator = new double[Bars];

            for (int bar = firstBar; bar < Bars; bar++)
                maOscillator[bar] = maFast[bar] - maSlow[bar];

            Component = new IndicatorComp[4];

            Component[0] = new IndicatorComp 
            {
                CompName = "Fast Moving Average",
                ChartColor = Colors.Goldenrod,
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                FirstBar = firstBar,
                Value = maFast
            };

            Component[1] = new IndicatorComp 
            {
                CompName = "Slow Moving Average",
                ChartColor = Colors.IndianRed,
                DataType = IndComponentType.IndicatorValue,
                ChartType = IndChartType.Line,
                FirstBar = firstBar,
                Value = maSlow
            };

            Component[2] = new IndicatorComp 
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            Component[3] = new IndicatorComp 
            {
                ChartType = IndChartType.NoChart,
                FirstBar = firstBar,
                Value = new double[Bars]
            };

            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[2].DataType = IndComponentType.AllowOpenLong;
                Component[2].CompName = "Is long entry allowed";
                Component[3].DataType = IndComponentType.AllowOpenShort;
                Component[3].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[2].DataType = IndComponentType.ForceCloseLong;
                Component[2].CompName = "Close out long position";
                Component[3].DataType = IndComponentType.ForceCloseShort;
                Component[3].CompName = "Close out short position";
            }

            var indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "Fast MA crosses Slow MA upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;
                case "Fast MA crosses Slow MA downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;
                case "Fast MA is higher than Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;
                case "Fast MA is lower than Slow MA":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;
            }

            OscillatorLogic(firstBar, previous, maOscillator, 0, 0, ref Component[2], ref Component[3], indLogic);
        }
    }


}
