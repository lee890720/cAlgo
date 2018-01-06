using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Data;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Currency : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter(DefaultValue = "EURUSD")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "USDCHF")]
        public string SecondSymbol { get; set; }

        [Parameter(DefaultValue = 120)]
        public int Period { get; set; }
        public DateTime SymbolTime;
        public int FirstIndex, SecondIndex;

        private MarketSeries _symbolFirstSeries, _symbolSecondSeries;
        protected override void Initialize()
        {
            _symbolFirstSeries = MarketData.GetSeries(FirstSymbol, TimeFrame);
            _symbolSecondSeries = MarketData.GetSeries(SecondSymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            SymbolTime = MarketSeries.OpenTime[index];
            FirstIndex = _symbolFirstSeries.GetIndexByDate(SymbolTime);
            SecondIndex = _symbolSecondSeries.GetIndexByDate(SymbolTime);

            //FirstClose
            double FirstClose = 0;
            if (FirstSymbol.Substring(3, 3) == "USD")
                FirstClose = _symbolFirstSeries.Close[FirstIndex];
            if (FirstSymbol.Substring(0, 3) == "USD")
                FirstClose = 1 / _symbolFirstSeries.Close[FirstIndex];
            if (FirstSymbol.Substring(3, 3) == "JPY")
                FirstClose = 100 / _symbolFirstSeries.Close[FirstIndex];
            if (FirstSymbol.Substring(0, 3) == "XBR")
                FirstClose = _symbolFirstSeries.Close[FirstIndex] / 100;
            if (FirstSymbol.Substring(0, 3) == "XTI")
                FirstClose = _symbolFirstSeries.Close[FirstIndex] / 100;
            if (FirstSymbol.Substring(0, 3) == "XAU")
                FirstClose = _symbolFirstSeries.Close[FirstIndex] / 100 / 10;
            if (FirstSymbol.Substring(0, 3) == "XAG")
                FirstClose = _symbolFirstSeries.Close[FirstIndex] / 100 * 4;

            //SecondClose
            double SecondClose = 0;
            if (SecondSymbol.Substring(3, 3) == "USD")
                SecondClose = _symbolSecondSeries.Close[SecondIndex];
            if (SecondSymbol.Substring(0, 3) == "USD")
                SecondClose = 1 / _symbolSecondSeries.Close[SecondIndex];
            if (SecondSymbol.Substring(3, 3) == "JPY")
                SecondClose = 100 / _symbolSecondSeries.Close[SecondIndex];
            if (SecondSymbol.Substring(0, 3) == "XBR")
                SecondClose = _symbolSecondSeries.Close[SecondIndex] / 100;
            if (SecondSymbol.Substring(0, 3) == "XTI")
                SecondClose = _symbolSecondSeries.Close[SecondIndex] / 100;
            if (SecondSymbol.Substring(0, 3) == "XAU")
                SecondClose = _symbolSecondSeries.Close[SecondIndex] / 100 / 10;
            if (SecondSymbol.Substring(0, 3) == "XAG")
                SecondClose = _symbolSecondSeries.Close[SecondIndex] / 100 * 4;

            Result[index] = (FirstClose - SecondClose) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
            //var result = from item in R
            //    group item by item into gro
            //    orderby gro.Count() descending
            //    select new 
            //    {
            //        num = gro.Key,
            //        nums = gro.Count()
            //    };
            double firstwave = AverageWave(FirstSymbol);
            double secondwave = AverageWave(SecondSymbol);
            ChartObjects.DrawText("ratio", Math.Round((firstwave / secondwave), 2).ToString(), StaticPosition.TopRight, Colors.White);
        }

        private double AverageWave(string symbol)
        {
            var symbolseries = MarketData.GetSeries(symbol, TimeFrame);
            var symbolindex = symbolseries.GetIndexByDate(SymbolTime);
            var symbolC = MarketData.GetSymbol(symbol);
            double total = 0;
            for (int i = symbolindex - Period + 1; i <= symbolindex; i++)
            {
                total += Math.Abs(symbolseries.Open[i] - symbolseries.Close[i]) / symbolC.PipSize;
            }
            return Math.Round(total / Period);
        }
    }
}
