using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using cAlgo.API;
using CSVLib;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SaveCSV : Robot
    {
        private string _datadir;
        private string _filename;
        private System.Timers.Timer timer1;

        protected override void OnStart()
        {
            _datadir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _filename = _datadir + "\\" + "cBotSet.csv";
            Print("fiName=" + _filename);
            InitTimer1();
            timer1.Start();
        }

        private void InitTimer1()
        {
            //设置定时间隔(毫秒为单位)
            int interval = 60000;
            timer1 = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            timer1.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer1.Enabled = true;
            //绑定Elapsed事件
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer1);
        }

        private void OnTimer1(object sender, System.Timers.ElapsedEventArgs e)
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            try
            {
                con.Open();
                DataSet dataset_time = new DataSet();
                string strsql_time = "select * from Frx_Ecs";
                SqlDataAdapter objdataadpater_time = new SqlDataAdapter(strsql_time, con);
                SqlCommandBuilder sql_time = new SqlCommandBuilder(objdataadpater_time);
                objdataadpater_time.SelectCommand.CommandTimeout = 1000;
                objdataadpater_time.Fill(dataset_time, "time");
                var utctime = DateTime.UtcNow;
                foreach (DataRow r in dataset_time.Tables["time"].Rows)
                {
                    if (r["EcsName"].ToString() == "LeeInfo")
                    {
                        r["EcsTime"] = utctime;
                        objdataadpater_time.Update(dataset_time.Tables["time"]);
                        Print("It's Successful to update " + utctime.ToString());

                    }
                }

                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 1000;
                objdataadpater.Fill(dataset, "cBotSet");
                CsvParsingHelper.SaveCsv(dataset.Tables["cBotSet"], _datadir);
                Print("It's Successful to save CSV.");
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

        protected override void OnStop()
        {
            timer1.Stop();
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
}
