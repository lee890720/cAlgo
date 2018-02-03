using System;
using System.Collections.Generic;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using cAlgo.Lib;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_MaCross : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("MA Type")]
        public MovingAverageType MAType { get; set; }

        [Parameter("SourceSeries")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        private MovingAverage _result;
        private MovingAverage _average;
        private Symbol _XBRSymbol, _XTISymbol;
        private MarketSeries _XBRSeries, _XTISeries;
        private DateTime _symboltime;
        private int _XBRIndex, _XTIIndex;

        protected override void Initialize()
        {
            _XBRSymbol = MarketData.GetSymbol("XBRUSD");
            _XTISymbol = MarketData.GetSymbol("XTIUSD");
            _XBRSeries = MarketData.GetSeries(_XBRSymbol, TimeFrame);
            _XTISeries = MarketData.GetSeries(_XTISymbol, TimeFrame);
            _result = Indicators.MovingAverage(SourceSeries, ResultPeriods, MAType);
            _average = Indicators.MovingAverage(SourceSeries, AveragePeriods, MAType);
        }

        public override void Calculate(int index)
        {
            _symboltime = MarketSeries.OpenTime[index];
            _XBRIndex = _XBRSeries.GetIndexByDate(_symboltime);
            _XTIIndex = _XTISeries.GetIndexByDate(_symboltime);
            var XBRTime = _XBRSeries.OpenTime[_XBRIndex];
            var XTITime = _XTISeries.OpenTime[_XTIIndex];
            List<DateTime> TimeList = new List<DateTime>();
            TimeList.Add(_symboltime);
            TimeList.Add(XBRTime);
            TimeList.Add(XTITime);
            _XBRIndex = _XBRSeries.GetIndexByDate(TimeList.Min());
            _XTIIndex = _XTISeries.GetIndexByDate(TimeList.Min());
            double XBRClose = _XBRSeries.Close[_XBRIndex] / _XBRSymbol.PipSize;
            double XTIClose = _XTISeries.Close[_XTIIndex] / _XTISymbol.PipSize;
            double NEWClose = (XBRClose / XTIClose) / (_XBRSymbol.PipSize * _XTISymbol.PipSize);
            Result[index] = NEWClose;
            double Sum = 0.0;
            for (int i = index - AveragePeriods + 1; i <= index; i++)
            {
                Sum += Result[i];
            }
            Average[index] = Sum / AveragePeriods;
        }
    }
}
