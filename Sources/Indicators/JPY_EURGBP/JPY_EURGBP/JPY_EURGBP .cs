﻿using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class JPY_EURGBP : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        private int Period = 120;
        private string BigSymbol = "EURJPY";
        private string SmallSymbol = "GBPJPY";
        private MarketSeries _symbolbigSeries, _symbolsmallSeries;
        protected override void Initialize()
        {
            _symbolbigSeries = MarketData.GetSeries(BigSymbol, TimeFrame);
            _symbolsmallSeries = MarketData.GetSeries(SmallSymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            DateTime SymbolTime = MarketSeries.OpenTime[index];
            int BigIndex = _symbolbigSeries.GetIndexByDate(SymbolTime);
            int SmallIndex = _symbolsmallSeries.GetIndexByDate(SymbolTime);
            Result[index] = (100 / _symbolbigSeries.Close[BigIndex] - 100 / _symbolsmallSeries.Close[SmallIndex]) / 0.0001 + 10000;
            double sum = 0.0;
            for (int i = index - Period + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / Period;
        }
    }
}
