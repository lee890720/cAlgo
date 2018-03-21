using cAlgo.API;
using cAlgo.API.Indicators;
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

        private DateTime _symboltime;
        private Symbol _xbrsymbol, _xtisymbol;
        private MarketSeries _xbrseries, _xtiseries;
        private int _xbrindex, _xtiindex;

        protected override void Initialize()
        {
            _xbrsymbol = MarketData.GetSymbol("XBRUSD");
            _xtisymbol = MarketData.GetSymbol("XTIUSD");
            _xbrseries = MarketData.GetSeries(_xbrsymbol, TimeFrame);
            _xtiseries = MarketData.GetSeries(_xtisymbol, TimeFrame);
        }

        public override void Calculate(int index)
        {
            _symboltime = MarketSeries.OpenTime[index];
            _xbrindex = _xbrseries.GetIndexByDate(_symboltime);
            _xtiindex = _xtiseries.GetIndexByDate(_symboltime);
            var xbrtime = _xbrseries.OpenTime[_xbrindex];
            var xtitime = _xtiseries.OpenTime[_xtiindex];
            List<DateTime> timelist = new List<DateTime>();
            timelist.Add(_symboltime);
            timelist.Add(xbrtime);
            timelist.Add(xtitime);
            _xbrindex = _xbrseries.GetIndexByDate(timelist.Min());
            _xtiindex = _xtiseries.GetIndexByDate(timelist.Min());
            double xbrclose = _xbrseries.Close[_xbrindex] / _xbrsymbol.PipSize;
            double xticlose = _xtiseries.Close[_xtiindex] / _xtisymbol.PipSize;
            double newclose = (xbrclose / xticlose) / (_xbrsymbol.PipSize * _xtisymbol.PipSize);
            Result[index] = newclose;
            double sum = 0.0;
            for (int i = index - AveragePeriods + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / AveragePeriods;
        }
    }
}
