using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class Metals_MaCross : Indicator
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
        private double _Ratio;
        private Symbol _XAUSymbol, _XAGSymbol;
        private MarketSeries _XAUSeries, _XAGSeries;
        private int _XAUIndex, _XAGIndex;

        protected override void Initialize()
        {
            _Ratio = 80;
            _XAUSymbol = MarketData.GetSymbol("XAUUSD");
            _XAGSymbol = MarketData.GetSymbol("XAGUSD");
            _XAUSeries = MarketData.GetSeries(_XAUSymbol, TimeFrame);
            _XAGSeries = MarketData.GetSeries(_XAGSymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            _Symboltime = MarketSeries.OpenTime[index];
            _XAUIndex = _XAUSeries.GetIndexByDate(_Symboltime);
            _XAGIndex = _XAGSeries.GetIndexByDate(_Symboltime);
            var XAUTime = _XAUSeries.OpenTime[_XAUIndex];
            var XAGTime = _XAGSeries.OpenTime[_XAGIndex];
            List<DateTime> TimeList = new List<DateTime>();
            TimeList.Add(_Symboltime);
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
