using cAlgo.API;
using cAlgo.API.Internals;
using JsonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Signal_macs : Robot
    {
        private int _id;
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private double _break;
        private string _filePath;
        private string _fileName;
        private MAC _mac;
        private MAS _mas;
        private bool _isChange;
        private System.Timers.Timer _timer1;

        protected override void OnStart()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
            SetParams();
            if (_magnify != 1)
            {
                Print("Please choose the MACS_Magnify.");
                this.Stop();
            }
            _mac = Indicators.GetIndicator<MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<MAS>(_resultperiods, _averageperiods, _sub, _break);
            _isChange = false;
            InitTimer1();
            _timer1.Start();
            Print("Done OnStart()");
        }

        private void InitTimer1()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 60000;
            _timer1 = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            _timer1.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            _timer1.Enabled = true;
            //绑定Elapsed事件
            _timer1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer1);
        }

        private void OnTimer1(object sender, System.Timers.ElapsedEventArgs e)
        {
            SetParams();
            if (_isChange)
            {
                _mac = Indicators.GetIndicator<MAC>(_resultperiods, _averageperiods, _sub);
                _mas = Indicators.GetIndicator<MAS>(_resultperiods, _averageperiods, _sub, _break);
                _isChange = false;
            }
            #region Parameter
            //var cr = Math.Round(_mac.Result.LastValue);
            //var ca = Math.Round(_mac.Average.LastValue);
            //var sr = Math.Round(_mas.Result.LastValue);
            //var sa = Math.Round(_mas.Average.LastValue);
            double cr = 0;
            if (!double.IsNaN(Math.Round(_mac.Result.LastValue)) && !double.IsInfinity(Math.Round(_mac.Result.LastValue)))
                cr = Math.Round(_mac.Result.LastValue);
            double ca = 0;
            if (!double.IsNaN(Math.Round(_mac.Average.LastValue)) && !double.IsInfinity(Math.Round(_mac.Average.LastValue)))
                ca = Math.Round(_mac.Average.LastValue);
            double sr = 0;
            if (!double.IsNaN(Math.Round(_mas.Result.LastValue)) && !double.IsInfinity(Math.Round(_mas.Result.LastValue)))
                sr = Math.Round(_mas.Result.LastValue);
            double sa = 0;
            if (!double.IsNaN(Math.Round(_mas.Average.LastValue)) && !double.IsInfinity(Math.Round(_mas.Average.LastValue)))
                sa = Math.Round(_mas.Average.LastValue);
            double srsa = 0;
            if (!double.IsNaN(Math.Round(_mas.SigTwo.LastValue)) && !double.IsInfinity(Math.Round(_mas.SigTwo.LastValue)))
                srsa = Math.Round(_mas.SigTwo.LastValue);
            var sig = _mas.SignalOne;
            var sig2 = _mas.SignalTwo;
            #endregion
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
                    dr["Signal"] = sig;
                    dr["Signal2"] = sig2;
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
            string data = Json.ReadJsonFile(_fileName);
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
                    if (_break != d.Brk)
                    {
                        _break = d.Brk;
                        Print("Break: " + _break.ToString() + "-" + _break.GetType().ToString());
                    }
                    break;
                }
            }
        }

        protected override void OnStop()
        {
            _timer1.Stop();
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
        public string Alike { get; set; }
    }
}
