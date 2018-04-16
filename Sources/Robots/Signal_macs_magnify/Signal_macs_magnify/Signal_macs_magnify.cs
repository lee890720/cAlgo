using cAlgo.API;
using cAlgo.API.Internals;
using JsonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Signal_macs_magnify : Robot
    {
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private string _filePath;
        private string _fileName;
        private _Magnify_MAC _mac;
        private _Magnify_MAS _mas;
        private bool _isChange;

        protected override void OnStart()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
            SetParams();
            if (_magnify == 1)
            {
                Print("Please choose the MACS.");
                this.Stop();
            }
            _mac = Indicators.GetIndicator<_Magnify_MAC>(_resultperiods, _averageperiods, _magnify, _sub);
            _mas = Indicators.GetIndicator<_Magnify_MAS>(_resultperiods, _averageperiods, _magnify, _sub);
            _isChange = false;
            Timer.Start(60);
            Print("Done OnStart()");
        }

        protected override void OnTimer()
        {
            SetParams();
            if (_isChange)
            {
                _mac = Indicators.GetIndicator<_Magnify_MAC>(_resultperiods, _averageperiods, _magnify, _sub);
                _mas = Indicators.GetIndicator<_Magnify_MAS>(_resultperiods, _averageperiods, _magnify, _sub);
                _isChange = false;
            }
            #region Parameter
            var cr = Math.Round(_mac.Result.LastValue);
            var ca = Math.Round(_mac.Average.LastValue);
            var sr = Math.Round(_mas.Result.LastValue);
            var sa = Math.Round(_mas.Average.LastValue);
            var sig = _mas.SignalOne;
            #endregion
            string data = ReadFileData();
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var l in list_data)
            {
                if (l.Symbol == Symbol.Code)
                {
                    l.Cr = cr;
                    l.Ca = ca;
                    l.Sr = sr;
                    l.Sa = sa;
                    l.Signal = sig;
                    break;
                }
            }
            var temp = Json.ListToJson(list_data);
            WriteFileData(temp);
            Print(Symbol.Code + " is changed.");
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
            if (!File.Exists(_fileName))
                return null;
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
            if (!File.Exists(_fileName))
                return;
            FileStream stream = null;
            StreamWriter streamWriter = null;
            stream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            streamWriter = new StreamWriter(stream, Encoding.Default);
            streamWriter.Write(data);
            streamWriter.Close();
            stream.Close();
        }

        protected override void OnStop()
        {
            Timer.Stop();
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
