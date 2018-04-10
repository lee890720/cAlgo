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
    public class ToSql_Cbotset : Robot
    {
        protected override void OnStart()
        {
            Timer.Start(60);
        }

        protected override void OnTimer()
        {
            try
            {
                string strcon_local = "Data Source=.;Initial Catalog=LeeInfoDb;User ID=sa;Password=Lee37355175;Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
                string strcon = "Server=tcp:leeinfo.database.windows.net,1433;Initial Catalog=LeeInfoDb;Persist Security Info=False;User ID=lee890720;Password=Lee37355175;Integrated Security=False;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;MultipleActiveResultSets=True";
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
                sqlData_local.Fill(dataset, "cBotSet_local");
                DataTable dt_local = dataset.Tables["cBotSet_local"];
                sqlData.Fill(dataset, "cBotSet");
                DataTable dt = dataset.Tables["cBotSet"];
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
                    }
                }
                var result = sqlData.Update(dataset, "cBotSet");
                Print(result.ToString() + " has been changed.");
                dataset.Dispose();
                sqlData_local.Dispose();
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
            Timer.Stop();
        }
    }
}
