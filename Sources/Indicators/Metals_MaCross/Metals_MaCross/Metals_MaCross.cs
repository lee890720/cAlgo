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

        private DateTime _symboltime;
        private double _ratio;
        private Symbol _xausymbol, _xagsymbol;
        private MarketSeries _xauseries, _xagseries;
        private int _xauindex, _xagindex;

        public int BarsAgo;

        private Colors _nocorel;

        protected override void Initialize()
        {
            _ratio = 80;
            _xausymbol = MarketData.GetSymbol("XAUUSD");
            _xagsymbol = MarketData.GetSymbol("XAGUSD");
            _xauseries = MarketData.GetSeries(_xausymbol, TimeFrame);
            _xagseries = MarketData.GetSeries(_xagsymbol, TimeFrame);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            _symboltime = MarketSeries.OpenTime[index];
            _xauindex = _xauseries.GetIndexByDate(_symboltime);
            _xagindex = _xagseries.GetIndexByDate(_symboltime);
            var xautime = _xauseries.OpenTime[_xauindex];
            var xagtime = _xagseries.OpenTime[_xagindex];
            List<DateTime> timelist = new List<DateTime>();
            timelist.Add(_symboltime);
            timelist.Add(xautime);
            timelist.Add(xagtime);
            _xauindex = _xauseries.GetIndexByDate(timelist.Min());
            _xagindex = _xagseries.GetIndexByDate(timelist.Min());
            double xauclose = _xauseries.Close[_xauindex] / _xausymbol.PipSize;
            double xagclose = _xagseries.Close[_xagindex] * _ratio / _xagsymbol.PipSize;
            double newclose = (xauclose / xagclose) / (_xausymbol.PipSize * _xagsymbol.PipSize);
            Result[index] = newclose;
            double sum = 0.0;
            for (int i = index - AveragePeriods + 1; i <= index; i++)
            {
                sum += Result[i];
            }
            Average[index] = sum / AveragePeriods;

            #region Chart
            BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private int GetBarsAgo(int index)
        {
            double cr = Result[index];
            double ca = Average[index];
            if (cr > ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (Result[i] <= Average[i])
                        return index - i;
                }
            if (cr < ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (Result[i] >= Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
