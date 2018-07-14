using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using JsonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class VMAS : Indicator
    {
        [Output("Result")]
        public IndicatorDataSeries Result { get; set; }

        [Output("Average")]
        public IndicatorDataSeries Average { get; set; }

        [Output("SigOne_A", Color = Colors.DeepSkyBlue, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_A { get; set; }

        [Output("SigOne_B", Color = Colors.OrangeRed, PlotType = PlotType.Histogram, LineStyle = LineStyle.LinesDots, Thickness = 1)]
        public IndicatorDataSeries SigOne_B { get; set; }

        [Output("SigTwo", Color = Colors.Yellow, PlotType = PlotType.Histogram, LineStyle = LineStyle.Dots, Thickness = 1)]
        public IndicatorDataSeries SigTwo { get; set; }

        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private double _brk;
        private string _filePath;
        private string _fileName;

        private MaCross _mac;
        private MaSub _mas;

        public string SignalOne;
        public string SignalTwo;
        public int BarsAgo;
        public string Mark;

        //PCorel=Colors.Lime;NCorel=Colors.OrangeRed;NoCorel=Colors.Gray;
        private Colors _nocorel;

        protected override void Initialize()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            SetParams();
            _mac = Indicators.GetIndicator<MaCross>(_resultperiods, _averageperiods);
            _mas = Indicators.GetIndicator<MaSub>(_resultperiods, _averageperiods);
            _nocorel = Colors.Gray;
        }

        public override void Calculate(int index)
        {
            Result[index] = _mas.Result[index];
            Average[index] = _mas.Average[index];

            SignalTwo = "";
            if (Result[index] > 0)
            {
                if (Average[index] > 0)
                {
                    if (Result[index] > Average[index])
                    {
                        SigTwo[index] = Result[index] - Average[index];
                        if (SigTwo[index] > _brk)
                            SignalTwo = "aboveBreak";
                    }
                    else
                        SigTwo[index] = 0;
                }
                else
                    SigTwo[index] = -(Result[index] - Average[index]);
            }
            else
            {
                if (Average[index] < 0)
                {
                    if (Result[index] < Average[index])
                    {
                        SigTwo[index] = Math.Abs(Result[index] - Average[index]);
                        if (SigTwo[index] > _brk)
                            SignalTwo = "belowBreak";
                    }
                    else
                        SigTwo[index] = 0;
                }
                else
                    SigTwo[index] = Result[index] - Average[index];
            }

            SignalOne = GetSigOne(index);
            if (SignalOne == "below")
                SigOne_B[index] = _mas.Result[index];
            else
                SigOne_B[index] = 0;
            if (SignalOne == "above")
                SigOne_A[index] = _mas.Result[index];
            else
                SigOne_A[index] = 0;

            #region Chart
            BarsAgo = _mas.BarsAgo;
            Mark = GetMark(index).ToString("yyyy-MM-dd") + "-" + GetMark(index).ToString("HH");
            if (string.IsNullOrEmpty(SignalOne))
                ChartObjects.DrawText("SignalOne", "NoSignal-1", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("SignalOne", "Signal1_(" + SignalOne + ")", StaticPosition.TopLeft, _nocorel);
            if (string.IsNullOrEmpty(SignalTwo))
                ChartObjects.DrawText("SignalTwo", "\nNoSignal-2", StaticPosition.TopLeft, _nocorel);
            else
                ChartObjects.DrawText("SignalTwo", "\nSignal2_(" + SignalTwo + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("barsago", "\n\nCross_(" + BarsAgo.ToString() + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("mark", "\n\n\nMark_(" + Mark + ")", StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawText("break", "\n\n\n\n" + GetBreak(index), StaticPosition.TopLeft, _nocorel);
            ChartObjects.DrawHorizontalLine("breakLine", _brk, Colors.DarkCyan);
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
        private void SetParams()
        {
            string data = Json.ReadJsonFile(_fileName);
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var d in list_data)
            {
                if (d.Symbol == Symbol.Code)
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
                    if (_brk != d.Brk)
                    {
                        _brk = d.Brk;
                        Print("Brk: " + _brk.ToString() + "-" + _brk.GetType().ToString());
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
