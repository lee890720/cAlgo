using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class VMAC_Metals : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_B { get; set; }

        private int _resultperiods;
        private int _averageperiods;
        private double _sub;
        private string _datadir;
        private string _filename;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        public string SignalOne;
        public int BarsAgo;
        private Metals_MaCross _mac;
        private Metals_MaSub _mas;
        private Colors _nocorel;

        private void SetParams()
        {
            DataTable dt = CSVLib.CsvParsingHelper.CsvToDataTable(_filename, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == "XAUXAG")
                {
                    if (_resultperiods != Convert.ToInt32(dr["result"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["result"]);
                    }
                    if (_averageperiods != Convert.ToInt32(dr["average"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["average"]);
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
            _mac = Indicators.GetIndicator<Metals_MaCross>(_resultperiods, _averageperiods);
            _mas = Indicators.GetIndicator<Metals_MaSub>(_resultperiods, _averageperiods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index];
            Average[index] = _mac.Average[index];
            string sigone = GetSigOne(index);
            if (sigone == "above")
                SigOne_A[index] = _mac.Result[index];
            if (sigone == "below")
                SigOne_B[index] = _mac.Result[index];

            #region Chart
            SignalOne = sigone;
            BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
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
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            if (cr > ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] <= _mac.Average[i])
                        return index - i;
                }
            if (cr < ca)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] >= _mac.Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
