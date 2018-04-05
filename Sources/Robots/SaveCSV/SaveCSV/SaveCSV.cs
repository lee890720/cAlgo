using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using cAlgo.API;
using CSVLib;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SaveCSV : Robot
    {
        private string _datadir;
        private string _filename;
        private System.Timers.Timer _timer1;

        protected override void OnStart()
        {
            _datadir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _filename = _datadir + "\\" + "cBotSet.csv";
            Print("fiName=" + _filename);
            InitTimer1();
            _timer1.Start();
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
            try
            {
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175;Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                sqlCon.Open();
                DataSet dataset_time = new DataSet();
                string strsql_time = "select * from Frx_Ecs";
                SqlDataAdapter sqlData_time = new SqlDataAdapter(strsql_time, sqlCon);
                SqlCommandBuilder sqlCom_time = new SqlCommandBuilder(sqlData_time);
                sqlData_time.Fill(dataset_time, "time");
                var utctime = DateTime.UtcNow;
                DataTable dt = dataset_time.Tables["time"];
                foreach (DataRow r in dt.Rows)
                {
                    if (r["EcsName"].ToString() == "LeeInfo")
                    {
                        r["EcsTime"] = utctime;
                    }
                }
                sqlData_time.Update(dataset_time, "time");
                Print("It's Successful to update " + utctime.ToString());
                dataset_time.Dispose();
                sqlCom_time.Dispose();
                sqlData_time.Dispose();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset";
                SqlDataAdapter sqlData = new SqlDataAdapter(strsql, sqlCon);
                SqlCommandBuilder sqlCom = new SqlCommandBuilder(sqlData);
                sqlData.Fill(dataset, "cBotSet");
                CsvParsingHelper.SaveCsv(dataset.Tables["cBotSet"], _datadir);
                Print("It's Successful to save CSV.");
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

        protected override void OnStop()
        {
            _timer1.Stop();
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
}
