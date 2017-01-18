
//+------------------------------------------------------+
//| The ZigZag indicator based on swing high low points. |
//+------------------------------------------------------+

#region Using declarations
using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
#endregion

namespace cAlgo.Indicators
{
    [Indicator(IsOverlay = true, AutoRescale = true, AccessRights = AccessRights.None)]
    public class SwingZigZag : Indicator
    {

        #region Properties
        [Parameter()]
        public DataSeries High { get; set; }

        [Parameter()]
        public DataSeries Low { get; set; }

        [Parameter("Period", DefaultValue = 20, MinValue = 1)]
        public int Period { get; set; }

        [Output("ZigZag", Color = Colors.Blue, Thickness = 2, PlotType = PlotType.Line)]
        public IndicatorDataSeries ZigZag { get; set; }

        [Output("Swing High", Color = Colors.Gray, Thickness = 1, PlotType = PlotType.Points, LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries SwingHigh { get; set; }

        [Output("Swing Low", Color = Colors.Gray, Thickness = 1, PlotType = PlotType.Points, LineStyle = LineStyle.Lines)]
        public IndicatorDataSeries SwingLow { get; set; }
        #endregion

        #region Variables
        private double currentZigZagHigh = 0;
        private double currentZigZagLow = 0;
        private int lastSwingIndex = -1;
        private double lastSwingPrice = 0.0;
        private int trendDir = 0;
        private int CurrentBar;
        #endregion

        protected override void Initialize()
        {

        }

        public override void Calculate(int index)
        {
            CurrentBar = High.Count;

            if (CurrentBar < 2)
                return;

            if (lastSwingPrice == 0.0)
                lastSwingPrice = Low[index] + (High[index] - Low[index]) / 2;

            bool isSwingHigh = High[index] == Functions.Maximum(High, Period);
            bool isSwingLow = Low[index] == Functions.Minimum(Low, Period);
            double saveValue = 0.0;
            bool addHigh = false;
            bool addLow = false;
            bool updateHigh = false;
            bool updateLow = false;

            if (!isSwingHigh && !isSwingLow)
            {
                return;
            }

            if (trendDir == 1 && isSwingHigh && High[index] >= lastSwingPrice)
            {
                saveValue = High[index];
                updateHigh = true;
            }
            else if (trendDir == -1 && isSwingLow && Low[index] <= lastSwingPrice)
            {
                saveValue = Low[index];
                updateLow = true;
            }
            else if (trendDir <= 0 && isSwingHigh)
            {
                saveValue = High[index];
                addHigh = true;
                trendDir = 1;
            }
            else if (trendDir >= 0 && isSwingLow)
            {
                saveValue = Low[index];
                addLow = true;
                trendDir = -1;
            }

            if (addHigh || addLow || updateHigh || updateLow)
            {
                if (updateHigh && lastSwingIndex >= 0)
                {
                    SwingHigh[lastSwingIndex] = double.NaN;
                    ZigZag[lastSwingIndex] = double.NaN;
                }
                else if (updateLow && lastSwingIndex >= 0)
                {
                    SwingLow[lastSwingIndex] = double.NaN;
                    ZigZag[lastSwingIndex] = double.NaN;
                }

                if (addHigh || updateHigh)
                {
                    currentZigZagHigh = saveValue;
                    SwingHigh[index] = currentZigZagHigh;
                    ZigZag[index] = currentZigZagHigh;
                }
                else if (addLow || updateLow)
                {
                    currentZigZagLow = saveValue;
                    SwingLow[index] = currentZigZagLow;
                    ZigZag[index] = currentZigZagLow;
                }

                lastSwingIndex = CurrentBar - 1;
                lastSwingPrice = saveValue;
            }
        }
    }
}

