using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ExportToSQL : Robot
    {
        protected override void OnStart()
        {
            Timer.Start(60);
        }

        protected override void OnTimer()
        {
            var utctime = DateTime.UtcNow;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            con.Open();
            DataSet dataset = new DataSet();
            string strsql = "select * from Person where PersonID=1";
            SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
            SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
            objdataadpater.Fill(dataset, "cBot");
            dataset.Tables["cBot"].Rows[0][3] = utctime;
            objdataadpater.Update(dataset.Tables["cBot"]);
            Print(dataset.Tables["cBot"].Rows[0][3].ToString());
            con.Close();
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
