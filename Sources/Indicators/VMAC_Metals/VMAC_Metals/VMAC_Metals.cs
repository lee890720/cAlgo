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

        [Output("Sig1_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries Sig1_A { get; set; }

        [Output("Sig1_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries Sig1_B { get; set; }

        private int ResultPeriods;
        private int AveragePeriods;
        private double Sub;
        private string DataDir;
        private string fiName;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        public string _Signal1;
        public int _BarsAgo;
        private Metals_MaCross _mac;
        private Metals_MaSub _mas;
        private Colors _nocorel;

        private void SetParams()
        {
            DataTable dt = CSVLib.CsvParsingHelper.CsvToDataTable(fiName, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == "XAUUSD" || dr["symbol"].ToString() == "XAGUSD")
                {
                    if (ResultPeriods != Convert.ToInt32(dr["resultperiods"]))
                    {
                        ResultPeriods = Convert.ToInt32(dr["resultperiods"]);
                    }
                    if (AveragePeriods != Convert.ToInt32(dr["averageperiods"]))
                    {
                        AveragePeriods = Convert.ToInt32(dr["averageperiods"]);
                    }
                    if (Sub != Convert.ToDouble(dr["sub"]))
                    {
                        Sub = Convert.ToDouble(dr["sub"]);
                    }
                    break;
                }
            }
            if (Sub == 0)
            {
                ResultPeriods = 1;
                AveragePeriods = 120;
                Sub = 30;
            }
        }

        protected override void Initialize()
        {
            Sub = 0;
            DataDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            fiName = DataDir + "\\" + "cBotSet.csv";
            SetParams();
            _mac = Indicators.GetIndicator<Metals_MaCross>(ResultPeriods, AveragePeriods);
            _mas = Indicators.GetIndicator<Metals_MaSub>(ResultPeriods, AveragePeriods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index];
            Average[index] = _mac.Average[index];
            string Sig1 = GetSig1(index);
            if (Sig1 == "above")
                Sig1_A[index] = _mac.Result[index];
            if (Sig1 == "below")
                Sig1_B[index] = _mac.Result[index];

            #region Chart
            _Signal1 = Sig1;
            _BarsAgo = GetBarsAgo(index);
            ChartObjects.DrawText("barsago", "Cross_(" + _BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            #endregion
        }

        private string GetSig1(int index)
        {
            double CR = _mac.Result[index];
            double CA = _mac.Average[index];
            double SR = _mas.Result[index];
            double SA = _mas.Average[index];
            if (-Sub > SR && SR > SA && CR < CA)
                return "below";
            if (Sub < SR && SR < SA && CR > CA)
                return "above";
            return null;
        }

        private int GetBarsAgo(int index)
        {
            double CR = _mac.Result[index];
            double CA = _mac.Average[index];
            if (CR > CA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] <= _mac.Average[i])
                        return index - i;
                }
            if (CR < CA)
                for (int i = index - 1; i > 0; i--)
                {
                    if (_mac.Result[i] >= _mac.Average[i])
                        return index - i;
                }
            return -1;
        }
    }
}
