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
    public class Metals_MaCross : Indicator
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

        public double _Ratio;
        private MovingAverage _result;
        private MovingAverage _average;
        private Symbol _XAUSymbol, _XAGSymbol;
        private MarketSeries _XAUSeries, _XAGSeries;
        private DateTime _symboltime;
        private int _XAUIndex, _XAGIndex;

        protected override void Initialize()
        {
            _Ratio = 80;
            _XAUSymbol = MarketData.GetSymbol("XAUUSD");
            _XAGSymbol = MarketData.GetSymbol("XAGUSD");
            _XAUSeries = MarketData.GetSeries(_XAUSymbol, TimeFrame);
            _XAGSeries = MarketData.GetSeries(_XAGSymbol, TimeFrame);
            _result = Indicators.MovingAverage(SourceSeries, ResultPeriods, MAType);
            _average = Indicators.MovingAverage(SourceSeries, AveragePeriods, MAType);
        }

        public override void Calculate(int index)
        {
            _symboltime = MarketSeries.OpenTime[index];
            _XAUIndex = _XAUSeries.GetIndexByDate(_symboltime);
            _XAGIndex = _XAGSeries.GetIndexByDate(_symboltime);
            var XAUTime = _XAUSeries.OpenTime[_XAUIndex];
            var XAGTime = _XAGSeries.OpenTime[_XAGIndex];
            List<DateTime> TimeList = new List<DateTime>();
            TimeList.Add(_symboltime);
            TimeList.Add(XAUTime);
            TimeList.Add(XAGTime);
            _XAUIndex = _XAUSeries.GetIndexByDate(TimeList.Min());
            _XAGIndex = _XAGSeries.GetIndexByDate(TimeList.Min());
            double XAUClose = _XAUSeries.Close[_XAUIndex] / _XAUSymbol.PipSize;
            double XAGClose = _XAGSeries.Close[_XAGIndex] * _Ratio / _XAGSymbol.PipSize;
            double NEWClose = (XAUClose / XAGClose) / (_XAUSymbol.PipSize * _XAGSymbol.PipSize);
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
