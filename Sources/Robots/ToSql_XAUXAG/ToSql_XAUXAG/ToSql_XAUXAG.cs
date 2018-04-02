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
    public class ToSql_XAUXAG : Robot
    {
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        private string _datadir;
        private string _filename;
        private Metals_MAC _mac;
        private Metals_MAS _mas;

        protected override void OnStart()
        {
            _datadir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _filename = _datadir + "\\" + "cBotSet.csv";
            Print("fiName=" + _filename);
            SetParams();
            _mac = Indicators.GetIndicator<Metals_MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<Metals_MAS>(_resultperiods, _averageperiods, _sub);
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
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            try
            {
                con.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset where symbol='";
                strsql += "XAUXAG" + "'";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 1000;
                objdataadpater.Fill(dataset, "cBotSet");
                foreach (DataRow dr in dataset.Tables["cBotSet"].Rows)
                {
                    var symbol = Convert.ToString(dr["symbol"]);
                    if (symbol == "XAUXAG")
                    {
                        dr["cr"] = cr;
                        dr["ca"] = ca;
                        dr["sr"] = sr;
                        dr["sa"] = sa;
                        dr["signal"] = sig;
                    }
                }
                var _result = objdataadpater.Update(dataset.Tables["cBotSet"]);
                Print("XAUXAG" + _result.ToString() + " has been changed.");
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
            if (!File.Exists(_filename))
                Thread.Sleep(1000);
            if (File.Exists(_filename))
                dt = CSVLib.CsvParsingHelper.CsvToDataTable(_filename, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == "XAUXAG")
                {
                    if (_resultperiods != Convert.ToInt32(dr["result"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["result"]);
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    }
                    if (_averageperiods != Convert.ToInt32(dr["average"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["average"]);
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
