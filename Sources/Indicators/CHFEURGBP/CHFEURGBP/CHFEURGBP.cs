using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class CHFEURGBP : Indicator
    {
        [Output("Result_One", Color = Colors.Red)]
        public IndicatorDataSeries Result_One { get; set; }

        [Output("Average_One", Color = Colors.Red)]
        public IndicatorDataSeries Average_One { get; set; }

        [Output("Result_Two", Color = Colors.Green)]
        public IndicatorDataSeries Result_Two { get; set; }

        [Output("Average_Two", Color = Colors.Green)]
        public IndicatorDataSeries Average_Two { get; set; }

        [Output("Result_Three", Color = Colors.Blue)]
        public IndicatorDataSeries Result_Three { get; set; }

        [Output("Average_Three", Color = Colors.Blue)]
        public IndicatorDataSeries Average_Three { get; set; }

        private int Period = 120;
        private string Big_OneSymbol = "EURUSD";
        private string Small_OneSymbol = "USDCHF";
        private MarketSeries _symbolbig_OneSeries, _symbolsmall_OneSeries;
        private string Big_TwoSymbol = "GBPUSD";
        private string Small_TwoSymbol = "USDCHF";
        private MarketSeries _symbolbig_TwoSeries, _symbolsmall_TwoSeries;
        private string Big_ThreeSymbol = "GBPUSD";
        private string Small_ThreeSymbol = "EURUSD";
        private MarketSeries _symbolbig_ThreeSeries, _symbolsmall_ThreeSeries;
        protected override void Initialize()
        {
            _symbolbig_OneSeries = MarketData.GetSeries(Big_OneSymbol, TimeFrame);
            _symbolsmall_OneSeries = MarketData.GetSeries(Small_OneSymbol, TimeFrame);
            _symbolbig_TwoSeries = MarketData.GetSeries(Big_TwoSymbol, TimeFrame);
            _symbolsmall_TwoSeries = MarketData.GetSeries(Small_TwoSymbol, TimeFrame);
            _symbolbig_ThreeSeries = MarketData.GetSeries(Big_ThreeSymbol, TimeFrame);
            _symbolsmall_ThreeSeries = MarketData.GetSeries(Small_ThreeSymbol, TimeFrame);

        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int Big_OneIndex = _symbolbig_OneSeries.GetIndexByDate(SymbolTime);
            int Small_OneIndex = _symbolsmall_OneSeries.GetIndexByDate(SymbolTime);
            Result_One[index] = (_symbolbig_OneSeries.Close[Big_OneIndex] - 1 / _symbolsmall_OneSeries.Close[Small_OneIndex]) / 0.0001 + 10000;
            double sum_One = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum_One += Result_One[i];
            }
            Average_One[index] = sum_One / Period;
            int Big_TwoIndex = _symbolbig_TwoSeries.GetIndexByDate(SymbolTime);
            int Small_TwoIndex = _symbolsmall_TwoSeries.GetIndexByDate(SymbolTime);
            Result_Two[index] = (_symbolbig_TwoSeries.Close[Big_TwoIndex] - 1 / _symbolsmall_TwoSeries.Close[Small_TwoIndex]) / 0.0001 + 10000;
            double sum_Two = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum_Two += Result_Two[i];
            }
            Average_Two[index] = sum_Two / Period;
            int Big_ThreeIndex = _symbolbig_ThreeSeries.GetIndexByDate(SymbolTime);
            int Small_ThreeIndex = _symbolsmall_ThreeSeries.GetIndexByDate(SymbolTime);
            Result_Three[index] = (_symbolbig_ThreeSeries.Close[Big_ThreeIndex] - _symbolsmall_ThreeSeries.Close[Small_ThreeIndex]) / 0.0001 + 10000;
            double sum_Three = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum_Three += Result_Three[i];
            }
            Average_Three[index] = sum_Three / Period;
        }
    }
}
