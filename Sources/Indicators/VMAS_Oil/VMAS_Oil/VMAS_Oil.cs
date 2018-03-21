using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class VMAS_Oil : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_B { get; set; }

        private int _resultperiods;
        private int _averageperiods;
        private double _sub;
        private string _datadir;
        private string _filename;

        public string SignalOne;
        public int BarsAgo;
        public string Mark;
        private Oil_MaCross _mac;
        private Oil_MaSub _mas;
        private Colors _nocorel;

        private void SetParams()
        {
            DataTable dt = CSVLib.CsvParsingHelper.CsvToDataTable(_filename, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == "XBRXTI")
                {
                    if (_resultperiods != Convert.ToInt32(dr["resultperiods"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["resultperiods"]);
                    }
                    if (_averageperiods != Convert.ToInt32(dr["averageperiods"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["averageperiods"]);
                    }
                    if (_sub != Convert.ToDouble(dr["sub"]))
                    {
                        _sub = Convert.ToDouble(dr["sub"]);
                    }
                    break;
                }
            }
            if (_sub == 0)
            {
                _resultperiods = 1;
                _averageperiods = 120;
                _sub = 30;
            }
        }

        protected override void Initialize()
        {
            _sub = 0;
            _datadir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _filename = _datadir + "\\" + "cBotSet.csv";
            SetParams();
            _mac = Indicators.GetIndicator<Oil_MaCross>(_resultperiods, _averageperiods);
            _mas = Indicators.GetIndicator<Oil_MaSub>(_resultperiods, _averageperiods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mas.Result[index];
            Average[index] = _mas.Average[index];
            string sigone = GetSigOne(index);
            if (sigone == "below")
                SigOne_B[index] = _mas.Result[index];
            else
                SigOne_B[index] = 0;
            if (sigone == "above")
                SigOne_A[index] = _mas.Result[index];
            else
                SigOne_A[index] = 0;

            #region Chart
            SignalOne = sigone;
            BarsAgo = GetBarsAgo(index);
            Mark = GetMark(index).ToString("yyyy-MM-dd") + "-" + GetMark(index).ToString("HH");
            if (SignalOne == null)
                ChartObjects.DrawText("sigone", "NoSignal", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("sigone", "Signal_(" + SignalOne + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("barsago", "\nCross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("mark", "\n\nMark_(" + Mark + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("break", "\n\n\n" + GetBreak(index), StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private string GetSigOne(int index)
        {
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            if (-_sub > sr && sr > sa && cr < ca)
                return "below";
            if (_sub < sr && sr < sa && cr > ca)
                return "above";
            return null;
        }

        private int GetBarsAgo(int index)
        {
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            if (sr > sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] <= _mas.Average[i])
                        return index - i;
                }
            if (sr < sa)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mas.Result[i] >= _mas.Average[i])
                        return index - i;
                }
            return -1;
        }

        private DateTime GetMark(int index)
        {
            int idx = index - BarsAgo;
            DateTime dt = MarketSeries.OpenTime[idx];
            return dt;
        }

        private string GetBreak(int index)
        {
            string br = null;
            int sub = 0;
            int initmax = 0;
            int t1 = 0;
            int t2 = 0;
            int t3 = 0;
            int t4 = 0;
            int t5 = 0;
            int bars = MarketSeries.Bars();
            int per = _averageperiods * 10;
            if (bars < _averageperiods * 10)
                per = bars;
            double maxbar = _mas.Result.Maximum(per);
            double minbar = _mas.Result.Minimum(per);
            double getmax = Math.Round(maxbar > Math.Abs(minbar) ? maxbar : Math.Abs(minbar));
            if (getmax < 100)
            {
                sub = 10;
                initmax = 100;
            }
            else if (getmax < 150)
            {
                sub = 15;
                initmax = 150;
            }
            else if (getmax < 200)
            {
                sub = 20;
                initmax = 200;
            }
            else if (getmax < 250)
            {
                sub = 25;
                initmax = 250;
            }
            else if (getmax < 300)
            {
                sub = 30;
                initmax = 300;
            }
            else if (getmax < 350)
            {
                sub = 35;
                initmax = 350;
            }
            else if (getmax < 400)
            {
                sub = 40;
                initmax = 400;
            }
            else
            {
                sub = 50;
                initmax = 500;
            }

            for (int i = ((bars > _averageperiods * 10) ? (bars - _averageperiods * 10) : 0); i < bars; i++)
            {
                var sr = Math.Abs(_mas.Result[i]);
                if (sr > initmax - sub * 0)
                {
                    t5++;
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 1)
                {
                    t4++;
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 2)
                {
                    t3++;
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 3)
                {
                    t2++;
                    t1++;
                    continue;
                }
                if (sr > initmax - sub * 4)
                {
                    t1++;
                    continue;
                }
            }
            br = "(" + Math.Round(150 / getmax, 3).ToString() + "-" + per.ToString() + "-" + getmax.ToString() + ")_(" + initmax.ToString() + "-" + sub.ToString() + ")_(" + (initmax - sub * 4).ToString() + "-" + t1.ToString() + ")_(" + (initmax - sub * 3).ToString() + "-" + t2.ToString() + ")_(" + (initmax - sub * 2).ToString() + "-" + t3.ToString() + ")_(" + (initmax - sub * 1).ToString() + "-" + t4.ToString() + ")_(" + (initmax - sub * 0).ToString() + "-" + t5.ToString() + ")";
            return br;
        }
    }
}
