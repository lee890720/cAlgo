using cAlgo.API;
using CSVLib;
using System;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ToSql : Robot
    {
        private SMAC _mac;
        private SMAS _mas;
        private Metals_MAC _mmac;
        private Metals_MAS _mmas;
        private Oil_MAC _omac;
        private Oil_MAS _omas;
        private string DataDir;
        private string fiName;
        private System.Timers.Timer timer;

        protected override void OnStart()
        {
            DataDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            fiName = DataDir + "\\" + "cBotSet.csv";
            Print("fiName=" + fiName);
            InitTimer();
            timer.Start();
        }

        private void InitTimer()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 60000;
            timer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer.Enabled = true;
            //绑定Elapsed事件
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
        }

        private void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Enabled = false;
            timer.Interval = 60000;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            try
            {
                con.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from CBotSet";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 1000;
                objdataadpater.Fill(dataset, "cBotSet");
                //CsvParsingHelper.SaveCsv(dataset.Tables["cBotSet"], DataDir);
                Print("The Count of Rows is " + dataset.Tables["cBotSet"].Rows.Count.ToString());
                var rowidx = 0;
                foreach (DataRow dr in dataset.Tables["cBotSet"].Rows)
                {
                    rowidx++;
                    var symbol = Convert.ToString(dr["symbol"]);
                    var result = Convert.ToInt32(dr["resultperiods"]);
                    var average = Convert.ToInt32(dr["averageperiods"]);
                    var magnify = Convert.ToDouble(dr["magnify"]);
                    var sub = Convert.ToDouble(dr["sub"]);
                    if (symbol == "XAUUSD")
                    {
                        _mmac = Indicators.GetIndicator<Metals_MAC>(result, average, sub);
                        _mmas = Indicators.GetIndicator<Metals_MAS>(result, average, sub);
                        dr["cr"] = _mmac.Result.LastValue;
                        dr["ca"] = _mmac.Average.LastValue;
                        dr["sr"] = _mmas.Result.LastValue;
                        dr["sa"] = _mmas.Average.LastValue;
                        dr["signal"] = _mmas._Signal1;
                        Print(symbol + rowidx.ToString() + " has been changed.");
                        continue;
                    }
                    if (symbol == "XBRUSD")
                    {
                        _omac = Indicators.GetIndicator<Oil_MAC>(result, average, sub);
                        _omas = Indicators.GetIndicator<Oil_MAS>(result, average, sub);
                        dr["cr"] = _omac.Result.LastValue;
                        dr["ca"] = _omac.Average.LastValue;
                        dr["sr"] = _omas.Result.LastValue;
                        dr["sa"] = _omas.Average.LastValue;
                        dr["signal"] = _omas._Signal1;
                        Print(symbol + rowidx.ToString() + " has been changed.");
                        continue;
                    }
                    _mac = Indicators.GetIndicator<SMAC>(symbol, result, average, magnify, sub);
                    _mas = Indicators.GetIndicator<SMAS>(symbol, result, average, magnify, sub);
                    dr["cr"] = _mac.Result.LastValue;
                    dr["ca"] = _mac.Average.LastValue;
                    dr["sr"] = _mas.Result.LastValue;
                    dr["sa"] = _mas.Average.LastValue;
                    dr["signal"] = _mas._Signal1;
                    Print(symbol + rowidx.ToString() + " has been changed.");
                    objdataadpater.Update(dataset.Tables["cBotSet"]);
                }
            } catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            } finally
            {
                con.Close();
                con.Dispose();
            }
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            timer.Stop();
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
}
