using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Signal2_magnify : Robot
    {
        private int _id;
        private int _resultPeriods;
        private int _averagePeriods;
        private double _magnify;
        private double _sub;
        private double _break;
        private string _filePath;
        private string _fileName;
        private Colors _nocorel;

        private SimpleMovingAverage _result;
        private SimpleMovingAverage _average;

        protected override void OnStart()
        {
            _nocorel = Colors.Gray;

            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
            SetParams();
            if (_magnify == 1)
            {
                Print("Please choose the MACS.");
                this.Stop();
            }

            _result = Indicators.SimpleMovingAverage(MarketSeries.Close, _resultPeriods);
            _average = Indicators.SimpleMovingAverage(MarketSeries.Close, _averagePeriods);

            Timer.Start(60);
            Print("Done OnStart()");
        }

        protected override void OnTimer()
        {
            if (double.IsNaN(_result.Result.LastValue) || double.IsInfinity(_result.Result.LastValue) || double.IsNaN(_average.Result.LastValue) || double.IsInfinity(_average.Result.LastValue))
                return;
            var cr = _result.Result.LastValue / Symbol.PipSize / _magnify;
            var ca = _average.Result.LastValue / Symbol.PipSize / _magnify;
            var sr = cr - ca;
            double sum = 0.0;
            for (int i = _averagePeriods; i > 0; i--)
            {
                sum += (_result.Result.Last(i) - _average.Result.Last(i)) / Symbol.PipSize / _magnify;
            }
            var sa = sum / _averagePeriods;

            string signal1 = null;
            if (-_sub > sr && sr > sa && cr < ca)
                signal1 = "below";
            if (_sub < sr && sr < sa && cr > ca)
                signal1 = "above";

            string signal2 = null;
            var srsa = 0.0;
            if (sr > 0)
            {
                if (sa > 0)
                {
                    if (sr > sa)
                    {
                        srsa = sr - sa;
                        if (srsa > _break)
                            signal2 = "aboveBreak";
                    }
                    else
                        srsa = 0;
                }
                else
                    srsa = -(sr - sa);
            }
            else
            {
                if (sa < 0)
                {
                    if (sr < sa)
                    {
                        srsa = Math.Abs(sr - sa);
                        if (srsa > _break)
                            signal2 = "belowBreak";
                    }
                    else
                        srsa = 0;
                }
                else
                    srsa = sr - sa;
            }

            try
            {
                string strcon = "Data Source=.;";
                strcon += "Initial Catalog=LeeInfoDb;";
                strcon += "User ID=lee890720;";
                strcon += "Password=Lee37355175;";
                strcon += "Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = strcon;
                sqlCon.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset";
                SqlDataAdapter sqlData = new SqlDataAdapter(strsql, sqlCon);
                SqlCommandBuilder sqlCom = new SqlCommandBuilder(sqlData);
                sqlData.Fill(dataset, "cbotset");
                DataTable dt = dataset.Tables["cbotset"];
                dt.PrimaryKey = new DataColumn[] 
                {
                    dt.Columns["Id"]
                };
                DataRow dr = dt.Rows.Find(_id);
                bool serverChanged = false;
                if (dr != null)
                {
                    dr["Cr"] = Math.Round(cr);
                    dr["Ca"] = Math.Round(ca);
                    dr["Sr"] = Math.Round(sr);
                    dr["Sa"] = Math.Round(sa);
                    dr["SrSa"] = Math.Round(srsa);
                    dr["Signal"] = signal1;
                    dr["Signal2"] = signal2;
                    ChartObjects.DrawText("data", Math.Round(cr).ToString() + "-" + Math.Round(ca).ToString() + "-" + Math.Round(sr).ToString() + "-" + Math.Round(sa).ToString() + "-" + Math.Round(srsa).ToString() + "-" + signal1 + "-" + signal2, StaticPosition.TopLeft, _nocorel);
                    serverChanged = true;
                }
                if (serverChanged)
                {
                    var result = sqlData.Update(dataset, "cbotset");
                    Print(Symbol.Code + result.ToString() + " has been changed.");
                }
                dataset.Dispose();
                sqlCom.Dispose();
                sqlData.Dispose();
                sqlCon.Close();
                sqlCon.Dispose();
            } catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            }
        }

        private void SetParams()
        {
            string data = ReadJsonFile(_fileName);
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var d in list_data)
            {
                if (d.Symbol == Symbol.Code)
                {
                    if (_id != d.Id)
                    {
                        _id = d.Id;
                        Print("Id: " + _id.ToString() + "-" + _id.GetType().ToString());
                    }
                    if (_resultPeriods != d.Result)
                    {
                        _resultPeriods = d.Result;
                        Print("ResultPeriods: " + _resultPeriods.ToString() + "-" + _resultPeriods.GetType().ToString());
                    }
                    if (_averagePeriods != d.Average)
                    {
                        _averagePeriods = d.Average;
                        Print("AveragePeriods: " + _averagePeriods.ToString() + "-" + _averagePeriods.GetType().ToString());
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
                    if (_break != d.Brk)
                    {
                        _break = d.Brk;
                        Print("Break: " + _break.ToString() + "-" + _break.GetType().ToString());
                    }
                    break;
                }
            }
        }

        private string ReadJsonFile(string _fileName)
        {
            if (!File.Exists(_fileName))
                return null;
            FileStream stream = null;
            StreamReader streamReader = null;
            stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streamReader = new StreamReader(stream);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            return data;
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
        public double? SrSa { get; set; }
        public string Signal { get; set; }
        public string Signal2 { get; set; }
    }
}
