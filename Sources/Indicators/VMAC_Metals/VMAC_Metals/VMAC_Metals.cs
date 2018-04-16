using cAlgo.API;
using cAlgo.API.Internals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        private double _magnify;
        private double _sub;
        private string _filePath;
        private string _fileName;
        private bool _isChange;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        public string SignalOne;
        public int BarsAgo;
        private Metals_MaCross _mac;
        private Metals_MaSub _mas;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            SetParams();
            _mac = Indicators.GetIndicator<Metals_MaCross>(_resultperiods, _averageperiods);
            _mas = Indicators.GetIndicator<Metals_MaSub>(_resultperiods, _averageperiods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            if (_isChange)
            {
                _mac = Indicators.GetIndicator<Metals_MaCross>(_resultperiods, _averageperiods);
                _mas = Indicators.GetIndicator<Metals_MaSub>(_resultperiods, _averageperiods);
                _isChange = false;
            }
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

        private void SetParams()
        {
            string data = ReadFileData();
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var d in list_data)
            {
                if (d.Symbol == Symbol.Code)
                {
                    if (_resultperiods != d.Result)
                    {
                        _resultperiods = d.Result;
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                        _isChange = true;
                    }
                    if (_averageperiods != d.Average)
                    {
                        _averageperiods = d.Average;
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                        _isChange = true;
                    }
                    if (_magnify != d.Magnify)
                    {
                        _magnify = d.Magnify;
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                        _isChange = true;
                    }
                    if (_sub != d.Sub)
                    {
                        _sub = d.Sub;
                        Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                        _isChange = true;
                    }
                    break;
                }
            }
        }

        private string ReadFileData()
        {
            FileStream stream = null;
            StreamReader streamReader = null;
            //StreamWriter streamWriter = null;
            stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(stream);
            //streamWriter = new StreamWriter(stream,Encoding.Default);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            return data;
        }

        private void WriteFileData(string data)
        {
            FileStream stream = null;
            StreamWriter streamWriter = null;
            stream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            streamWriter = new StreamWriter(stream, Encoding.Default);
            streamWriter.Write(data);
            streamWriter.Close();
            stream.Close();
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
        public string Signal { get; set; }
        public string Alike { get; set; }
    }
}
