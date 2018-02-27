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
    public class ToSql_macs : Robot
    {
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private string DataDir;
        private string fiName;
        private MAC _mac;
        private MAS _mas;

        protected override void OnStart()
        {
            DataDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            fiName = DataDir + "\\" + "cBotSet.csv";
            Print("fiName=" + fiName);
            SetParams();
            if (_magnify != 1)
            {
                Print("Please choose the MACS_Magnify.");
                this.Stop();
            }
            _mac = Indicators.GetIndicator<MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<MAS>(_resultperiods, _averageperiods, _sub);
            Timer.Start(60);
            Print("Done OnStart()");
        }

        protected override void OnTimer()
        {
            #region Parameter
            var CR = _mac.Result.LastValue;
            var CA = _mac.Average.LastValue;
            var SR = _mas.Result.LastValue;
            var SA = _mas.Average.LastValue;
            var Sig = _mas._Signal1;
            #endregion
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            try
            {
                con.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from CBotSet where symbol='";
                strsql += Symbol.Code + "'";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 1000;
                objdataadpater.Fill(dataset, "cBotSet");
                foreach (DataRow dr in dataset.Tables["cBotSet"].Rows)
                {
                    var symbol = Convert.ToString(dr["symbol"]);
                    if (symbol == Symbol.Code)
                    {
                        dr["cr"] = CR;
                        dr["ca"] = CA;
                        dr["sr"] = SR;
                        dr["sa"] = SA;
                        dr["signal"] = Sig;
                    }
                }
                var _result = objdataadpater.Update(dataset.Tables["cBotSet"]);
                Print(Symbol.Code + _result.ToString() + " has been changed.");
            } catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            } finally
            {
                con.Close();
                con.Dispose();
            }
        }

        private void SetParams()
        {
            DataTable dt = new DataTable();
            if (!File.Exists(fiName))
                Thread.Sleep(1000);
            if (File.Exists(fiName))
                dt = CSVLib.CsvParsingHelper.CsvToDataTable(fiName, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == Symbol.Code)
                {
                    if (_resultperiods != Convert.ToInt32(dr["resultperiods"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["resultperiods"]);
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    }
                    if (_averageperiods != Convert.ToInt32(dr["averageperiods"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["averageperiods"]);
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                    }
                    if (_magnify != Convert.ToDouble(dr["magnify"]))
                    {
                        _magnify = Convert.ToDouble(dr["magnify"]);
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                    }
                    if (_sub != Convert.ToDouble(dr["sub"]))
                    {
                        _sub = Convert.ToDouble(dr["sub"]);
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
