using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ToSql_macs_magnify : Robot
    {
        [Parameter("Data Source", DefaultValue = ".")]
        public string DataSource { get; set; }

        [Parameter("Initial Catalog", DefaultValue = "LeeInfoDb")]
        public string InitialCatalog { get; set; }

        [Parameter("User ID", DefaultValue = "sa")]
        public string UserID { get; set; }

        [Parameter("Password", DefaultValue = "Lee37355175")]
        public string Password { get; set; }

        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private string _datadir;
        private string _filename;
        private _Magnify_MAC _mac;
        private _Magnify_MAS _mas;

        protected override void OnStart()
        {
            _datadir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _filename = _datadir + "\\" + "cBotSet.csv";
            Print("fiName=" + _filename);
            SetParams();
            if (_magnify == 1)
            {
                Print("Please choose the MACS.");
                this.Stop();
            }
            _mac = Indicators.GetIndicator<_Magnify_MAC>(_resultperiods, _averageperiods, _magnify, _sub);
            _mas = Indicators.GetIndicator<_Magnify_MAS>(_resultperiods, _averageperiods, _magnify, _sub);
            Timer.Start(60);
            Print("Done OnStart()");
        }

        protected override void OnTimer()
        {
            #region Parameter
            var cr = Math.Round(_mac.Result.LastValue);
            var ca = Math.Round(_mac.Average.LastValue);
            var sr = Math.Round(_mas.Result.LastValue);
            var sa = Math.Round(_mas.Average.LastValue);
            var sig = _mas.SignalOne;
            #endregion
            try
            {
                string strcon = "Data Source=";
                strcon += DataSource + ";Initial Catalog=";
                strcon += InitialCatalog + ";User ID=";
                strcon += UserID + ";Password=";
                strcon += Password + ";Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = strcon;
                sqlCon.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset where symbol='";
                strsql += Symbol.Code + "'";
                SqlDataAdapter sqlData = new SqlDataAdapter(strsql, sqlCon);
                SqlCommandBuilder sqlCom = new SqlCommandBuilder(sqlData);
                sqlData.Fill(dataset, "cBotSet");
                DataTable dt = dataset.Tables["cBotSet"];
                foreach (DataRow dr in dt.Rows)
                {
                    var symbol = Convert.ToString(dr["Symbol"]);
                    if (symbol == Symbol.Code)
                    {
                        dr["Cr"] = cr;
                        dr["Ca"] = ca;
                        dr["Sr"] = sr;
                        dr["Sa"] = sa;
                        dr["Signal"] = sig;
                    }
                }
                var result = sqlData.Update(dataset, "cBotSet");
                Print(Symbol.Code + result.ToString() + " has been changed.");
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
            DataTable dt = new DataTable();
            if (!File.Exists(_filename))
                Thread.Sleep(1000);
            if (File.Exists(_filename))
                dt = CSVLib.CsvParsingHelper.CsvToDataTable(_filename, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["Symbol"].ToString() == Symbol.Code)
                {
                    if (_resultperiods != Convert.ToInt32(dr["Result"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["Result"]);
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    }
                    if (_averageperiods != Convert.ToInt32(dr["Average"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["Average"]);
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                    }
                    if (_magnify != Convert.ToDouble(dr["Magnify"]))
                    {
                        _magnify = Convert.ToDouble(dr["Magnify"]);
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                    }
                    if (_sub != Convert.ToDouble(dr["Sub"]))
                    {
                        _sub = Convert.ToDouble(dr["Sub"]);
                        Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                    }
                    break;
                }
            }
        }

        protected override void OnStop()
        {
            Timer.Stop();
        }
    }
}
