using cAlgo.API;
using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class ToSql_TradeData : Robot
    {
        [Parameter("Data Source", DefaultValue = ".")]
        public string DataSource { get; set; }

        [Parameter("Initial Catalog", DefaultValue = "LeeInfoDb")]
        public string InitialCatalog { get; set; }

        [Parameter("User ID", DefaultValue = "sa")]
        public string UserID { get; set; }

        [Parameter("Password", DefaultValue = "Lee37355175")]
        public string Password { get; set; }

        protected override void OnStart()
        {
            Timer.Start(600);
        }

        protected override void OnTimer()
        {
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
                string strsql_account = "select * from Frx_Account";
                SqlDataAdapter sqlData_account = new SqlDataAdapter(strsql_account, sqlCon);
                sqlData_account.Fill(dataset, "Frx_Account");
                DataTable dt_account = dataset.Tables["Frx_Account"];
                int accountid = 0;
                foreach (DataRow r in dt_account.Rows)
                {
                    if (Convert.ToString(r["AccountNumber"]) == this.Account.Number.ToString())
                    {
                        accountid = Convert.ToInt32(r["Id"]);
                        Print("It's success to find the account id.");
                    }
                }

                using (var cmd = sqlCon.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM Frx_Position";
                    cmd.ExecuteNonQuery();
                }
                string strsql_pos = "select * from Frx_Position";
                SqlDataAdapter sqlData_pos = new SqlDataAdapter(strsql_pos, sqlCon);
                SqlCommandBuilder sqlCom_pos = new SqlCommandBuilder(sqlData_pos);
                sqlData_pos.Fill(dataset, "Frx_Position");
                DataTable dt_pos = dataset.Tables["Frx_Position"];
                foreach (var p in Positions)
                {
                    DataRow dr_pos = dt_pos.NewRow();
                    dr_pos["Id"] = p.Id;
                    dr_pos["Comment"] = p.Comment;
                    dr_pos["Commissions"] = p.Commissions;
                    dr_pos["EntryPrice"] = p.EntryPrice;
                    dr_pos["EntryTime"] = p.EntryTime;
                    dr_pos["GrossProfit"] = p.GrossProfit;
                    dr_pos["Label"] = p.Label;
                    dr_pos["NetProfit"] = p.NetProfit;
                    dr_pos["Pips"] = p.Pips;
                    dr_pos["Quantity"] = p.Quantity;
                    if (p.StopLoss == null)
                        dr_pos["StopLoss"] = DBNull.Value;
                    else
                        dr_pos["StopLoss"] = p.StopLoss;
                    dr_pos["Swap"] = p.Swap;
                    dr_pos["SymbolCode"] = p.SymbolCode;
                    if (p.TakeProfit == null)
                        dr_pos["TakeProfit"] = DBNull.Value;
                    else
                        dr_pos["TakeProfit"] = p.TakeProfit;
                    dr_pos["TradeType"] = p.TradeType;
                    dr_pos["Volume"] = p.Volume;
                    dr_pos["FrxAccountId"] = accountid;
                    dt_pos.Rows.Add(dr_pos);
                }
                sqlData_pos.Update(dataset, "Frx_Position");
                Print("It's success to update Frx_Position.");

                string strsql_his = "select * from Frx_History";
                SqlDataAdapter sqlData_his = new SqlDataAdapter(strsql_his, sqlCon);
                SqlCommandBuilder sqlCom_his = new SqlCommandBuilder(sqlData_his);
                sqlData_his.Fill(dataset, "Frx_History");
                DataTable dt_his = dataset.Tables["Frx_History"];
                dt_his.PrimaryKey = new DataColumn[] 
                {
                    dt_his.Columns["ClosingDealId"]
                };
                foreach (var h in History)
                {
                    DataRow dr_his = dt_his.NewRow();
                    dr_his["ClosingDealId"] = h.ClosingDealId;
                    dr_his["Balance"] = h.Balance;
                    dr_his["ClosingPrice"] = h.ClosingPrice;
                    dr_his["ClosingTime"] = h.ClosingTime;
                    dr_his["Comment"] = h.Comment;
                    dr_his["Commissions"] = h.Commissions;
                    dr_his["EntryPrice"] = h.EntryPrice;
                    dr_his["EntryTime"] = h.EntryTime;
                    dr_his["GrossProfit"] = h.GrossProfit;
                    dr_his["Label"] = h.Label;
                    dr_his["NetProfit"] = h.NetProfit;
                    dr_his["Pips"] = h.Pips;
                    dr_his["PositionId"] = h.PositionId;
                    dr_his["Quantity"] = h.Quantity;
                    dr_his["Swap"] = h.Swap;
                    dr_his["SymbolCode"] = h.SymbolCode;
                    dr_his["TradeType"] = h.TradeType;
                    dr_his["Volume"] = h.Volume;
                    dr_his["FrxAccountId"] = accountid;
                    DataRow dr = dt_his.Rows.Find(h.ClosingDealId);
                    if (dr == null)
                        dt_his.Rows.Add(dr_his);
                }
                sqlData_his.Update(dataset, "Frx_History");
                Print("It's success to update Frx_History.");
                dataset.Dispose();
                sqlData_account.Dispose();
                sqlCom_pos.Dispose();
                sqlData_pos.Dispose();
                sqlCom_his.Dispose();
                sqlData_his.Dispose();
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
            Print("OnStop()");
            // Put your deinitialization logic here
        }
    }
}
