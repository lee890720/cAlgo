using cAlgo.API;
using System;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class SQL_Local_to_server : Robot
    {
        private System.Timers.Timer _timer1;

        protected override void OnStart()
        {
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
                string strcon_local = "Data Source=.;Initial Catalog=LeeInfoDb;User ID=lee890720;Password=Lee37355175;Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                string strcon = "Server=tcp:leeinfodb.database.windows.net,1433;Initial Catalog=LeeInfoDb;Persist Security Info=False;User ID=lee890720;Password=Lee37355175;Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                SqlConnection sqlCon_local = new SqlConnection();
                sqlCon_local.ConnectionString = strcon_local;
                sqlCon_local.Open();
                SqlConnection sqlCon = new SqlConnection();
                sqlCon.ConnectionString = strcon;
                sqlCon.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset";
                SqlDataAdapter sqlData_local = new SqlDataAdapter(strsql, sqlCon_local);
                SqlDataAdapter sqlData = new SqlDataAdapter(strsql, sqlCon);
                SqlCommandBuilder sqlCom = new SqlCommandBuilder(sqlData);
                sqlData_local.Fill(dataset, "cbotset_local");
                DataTable dt_local = dataset.Tables["cbotset_local"];
                sqlData.Fill(dataset, "cbotset");
                DataTable dt = dataset.Tables["cbotset"];
                dt_local.PrimaryKey = new DataColumn[] 
                {
                    dt_local.Columns["Id"]
                };
                dt.PrimaryKey = new DataColumn[] 
                {
                    dt.Columns["Id"]
                };
                foreach (DataRow dr in dt.Rows)
                {
                    DataRow dr_local = dt_local.Rows.Find(Convert.ToInt32(dr["Id"]));
                    if (dr_local != null)
                    {
                        dr["Cr"] = dr_local["Cr"];
                        dr["Ca"] = dr_local["Ca"];
                        dr["Sr"] = dr_local["Sr"];
                        dr["Sa"] = dr_local["Sa"];
                        dr["Signal"] = dr_local["Signal"];
                        dr["Signal2"] = dr_local["Signal2"];
                    }
                }
                var result = sqlData.Update(dataset, "cbotset");
                Print(result.ToString() + " has been changed.");
                dataset.Dispose();
                sqlData_local.Dispose();
                sqlCon_local.Close();
                sqlCon_local.Dispose();
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
