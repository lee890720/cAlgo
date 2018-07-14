using cAlgo.API;
using cAlgo.API.Internals;
using JsonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class VMAC_Oil : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigOne_B { get; set; }

        [Output("SigTwo", Color = Colors.Yellow, PlotType = PlotType.Points, Thickness = 2)]
        public IndicatorDataSeries SigTwo { get; set; }

        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private string _filePath;
        private string _fileName;

        private Oil_MaCross _mac;
        private Oil_MaSub _mas;

        public string SignalOne;
        public string SignalTwo;
        public int BarsAgo;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            SetParams();
            _mac = Indicators.GetIndicator<Oil_MaCross>(_resultperiods, _averageperiods);
            _mas = Indicators.GetIndicator<Oil_MaSub>(_resultperiods, _averageperiods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mac.Result[index];
            Average[index] = _mac.Average[index];
            SignalOne = GetSigOne(index);
            if (SignalOne == "above")
                SigOne_A[index] = _mac.Result[index];
            if (SignalOne == "below")
                SigOne_B[index] = _mac.Result[index];
            SignalTwo = GetSigTwo(index);
            if (SignalTwo != null)
                SigTwo[index] = _mac.Result[index];
            #region Chart
            BarsAgo = _mac.BarsAgo;
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

        private string GetSigTwo(int index)
        {
            double cr = _mac.Result[index];
            double ca = _mac.Average[index];
            double sr = _mas.Result[index];
            double sa = _mas.Average[index];
            double sr1 = _mas.Result[index - 1];
            double cBarsAgo = _mac.BarsAgo;
            if (sa > 0)
            {
                if (sr <= -_sub && sr1 > -_sub)
                {
                    for (int i = index - (int)cBarsAgo - 1; i < index; i++)
                    {
                        if (sr > _mas.Result[i])
                            return null;
                    }
                    return "belowTrend";
                }
            }
            if (sa < 0)
            {
                if (sr >= _sub && sr1 < _sub)
                {
                    for (int i = index - (int)cBarsAgo - 1; i < index; i++)
                    {
                        if (sr < _mas.Result[i])
                            return null;
                    }
                    return "aboveTrend";
                }
            }
            return null;
        }

        private void SetParams()
        {
            string data = Json.ReadJsonFile(_fileName);
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var d in list_data)
            {
                if (d.Symbol == "XBRXTI")
                {
                    if (_resultperiods != d.Result)
                    {
                        _resultperiods = d.Result;
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    }
                    if (_averageperiods != d.Average)
                    {
                        _averageperiods = d.Average;
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                    }
                    if (_magnify != d.Magnify)
                    {
                        _magnify = d.Magnify;
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                    }
                    if (_sub != d.Sub)
                    {
                        _sub = d.Sub;
                        Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                    }
                    break;
                }
            }
        }
    }

    public class FrxCbotset
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public int InitVolume { get; set; }
        public int Tmr { get; set; }
        public double Brk { get; set; }
        public double Distance { get; set; }
        public bool IsTrade { get; set; }
        public bool IsBreak { get; set; }
        public bool IsBrkFirst { get; set; }
        public int Result { get; set; }
        public int Average { get; set; }
        public double Magnify { get; set; }
        public double Sub { get; set; }
        public double? Cr { get; set; }
        public double? Ca { get; set; }
        public double? Sr { get; set; }
        public double? Sa { get; set; }
        public double? SrSa { get; set; }
        public string Signal { get; set; }
        public string Signal2 { get; set; }
    }
}
