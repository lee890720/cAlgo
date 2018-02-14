using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Oil_MaCross : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int ResultPeriods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int AveragePeriods { get; set; }

        private DateTime _Symboltime;
        private Symbol _XBRSymbol, _XTISymbol;
        private MarketSeries _XBRSeries, _XTISeries;
        private int _XBRIndex, _XTIIndex;

        protected override void Initialize()
        {
            _XBRSymbol = MarketData.GetSymbol("XBRUSD");
            _XTISymbol = MarketData.GetSymbol("XTIUSD");
            _XBRSeries = MarketData.GetSeries(_XBRSymbol, TimeFrame);
            _XTISeries = MarketData.GetSeries(_XTISymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            _Symboltime = MarketSeries.OpenTime[index];
            _XBRIndex = _XBRSeries.GetIndexByDate(_Symboltime);
            _XTIIndex = _XTISeries.GetIndexByDate(_Symboltime);
            var XBRTime = _XBRSeries.OpenTime[_XBRIndex];
            var XTITime = _XTISeries.OpenTime[_XTIIndex];
            List<DateTime> TimeList = new List<DateTime>();
            TimeList.Add(_Symboltime);
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
