using cAlgo.API;
using CSVLib;
using System;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SaveCSV : Robot
    {
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
            int interval = 10000;
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
            var utctime = DateTime.UtcNow;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            try
            {
                con.Open();
                DataSet dataset_time = new DataSet();
                string strsql_time = "select * from Person where PersonID=1";
                SqlDataAdapter objdataadpater_time = new SqlDataAdapter(strsql_time, con);
                SqlCommandBuilder sql_time = new SqlCommandBuilder(objdataadpater_time);
                objdataadpater_time.SelectCommand.CommandTimeout = 1000;
                objdataadpater_time.Fill(dataset_time, "time");
                dataset_time.Tables["time"].Rows[0][3] = utctime;
                objdataadpater_time.Update(dataset_time.Tables["time"]);
                Print(dataset_time.Tables["time"].Rows[0][3].ToString());
                objdataadpater_time.Update(dataset_time.Tables["time"]);

                DataSet dataset = new DataSet();
                string strsql = "select * from CBotSet";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 1000;
                objdataadpater.Fill(dataset, "cBotSet");
                CsvParsingHelper.SaveCsv(dataset.Tables["cBotSet"], DataDir);
                Print("It's Successful to save CSV.");
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }

        protected override void OnStop()
        {
            timer.Stop();
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
}
