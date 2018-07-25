using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using FormLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Management : Robot
    {
        private FormLib.ThreadHandler _threadhandler;
        private ThreadStart _threadstart;
        private Thread _thread;
        protected override void OnStart()
        {
            _threadhandler = new ThreadHandler(this.Account.Number.ToString());
            _threadstart = new ThreadStart(_threadhandler.PositionsFormWork);
            _thread = new Thread(_threadstart);
            _thread.Start();
            _threadhandler.open_Click += OnOpen_Click;
            _threadhandler.close_Click += OnClose_Click;
            System.Threading.Thread.Sleep(1500);
        }

        protected override void OnTick()
        {
            List<string> list_str = new List<string>();
            string str;
            foreach (var p in Positions)
            {
                if (!string.IsNullOrEmpty(p.Label))
                {
                    str = p.SymbolCode + "-(" + p.Label + ")-[" + (this.TotalLots(p.Label, MarketData.GetSymbol(p.SymbolCode))).ToString() + "]-<" + this.LastPosition(this.GetPositions(p.Label)).Comment + ">";
                    if (!list_str.Contains(str))
                        list_str.Add(str);
                }
            }
            var b = Math.Round(this.Account.Balance, 2).ToString();
            var e = Math.Round(this.Account.Equity, 2).ToString();
            var m = Math.Round(this.Account.Margin, 2).ToString();
            var n = Math.Round(this.Account.UnrealizedNetProfit, 2).ToString();
            var i = 0;
            b = string.IsNullOrEmpty(b) ? i.ToString() : b;
            e = string.IsNullOrEmpty(e) ? i.ToString() : e;
            m = string.IsNullOrEmpty(m) ? i.ToString() : m;
            n = string.IsNullOrEmpty(n) ? i.ToString() : n;
            _threadhandler.setPos(list_str);
            _threadhandler.setAccountInfo(b, e, m, n);
        }

        private void OnOpen_Click()
        {
            var list_p = _threadhandler.positionParam();
            Symbol p_symbol = MarketData.GetSymbol(list_p[0]);
            string cr_comment = null;
            #region GetComment
            SqlConnection con = new SqlConnection();
            //con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            con.ConnectionString = "Data Source=leeinfodb.database.windows.net;Initial Catalog=LeeInfoDb;User ID=Lee890720;Password=Lee37355175";
            try
            {
                con.Open();
                DataSet dataset = new DataSet();
                string strsql = "select * from Frx_Cbotset where symbol='" + p_symbol.Code + "'";
                SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
                SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
                objdataadpater.SelectCommand.CommandTimeout = 300;
                objdataadpater.Fill(dataset, "cBot");
                foreach (DataRow dr in dataset.Tables["cBot"].Rows)
                {
                    if (Convert.ToString(dr["symbol"]) == p_symbol.Code)
                        cr_comment = "CR_" + string.Format("{0:000000}", dr["cr"]);
                }
                objdataadpater.Update(dataset.Tables["cBot"]);
            } catch (System.Data.SqlClient.SqlException ex)
            {
                Print(ex.ToString());
                throw new Exception(ex.Message);
            } finally
            {
                con.Close();
                con.Dispose();
            }
            #endregion
            var p_label = list_p[1];
            var p_volume = p_symbol.NormalizeVolumeInUnits(Convert.ToDouble(list_p[2]), RoundingMode.ToNearest);
            var p_comment = list_p[3].Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
            p_comment = p_comment.Remove(0, 9);
            p_comment = cr_comment + p_comment;
            TradeType p_trade = new TradeType();
            if (p_label.Contains("Above"))
                p_trade = TradeType.Sell;
            if (p_label.Contains("Below"))
                p_trade = TradeType.Buy;
            //if (p_symbol.Code != p_label.Substring(6, 6))
            if(!p_label.Contains(p_symbol.Code))
                if (p_symbol.Code.Contains("USD"))
                {
                    Symbol _firstsymbol = MarketData.GetSymbol(p_label.Substring(6, 3) + "USD");
                    Symbol _secondsymbol = MarketData.GetSymbol(p_label.Substring(9, 3) + "USD");
                    var _firstvolume = p_volume;
                    var _secondvolume = p_volume;
                    if (_firstsymbol.Code == "XAUUSD")
                    {
                        if (p_symbol.Code == "XAUUSD")
                            _secondvolume = _firstvolume * 80;
                        if (p_symbol.Code == "XAGUSD")
                            _firstvolume = _secondvolume / 80;
                    }
                    if (p_label.Contains("Above"))
                    {
                        ExecuteMarketOrder(p_trade, _firstsymbol, _firstvolume, p_label, null, null, 2, p_comment);
                        ExecuteMarketOrder(p_trade.inverseTradeType(), _secondsymbol, _secondvolume, p_label, null, null, null, p_comment);

                    }
                    if (p_label.Contains("Below"))
                    {
                        ExecuteMarketOrder(p_trade, _firstsymbol, _firstvolume, p_label, null, null, 3, p_comment);
                        ExecuteMarketOrder(p_trade.inverseTradeType(), _secondsymbol, _secondvolume, p_label, null, null, null, p_comment);
                    }
                    return;
                }
                else
                    return;
            ExecuteMarketOrder(p_trade, p_symbol, p_volume, p_label, null, null, 4, p_comment);
        }

        protected override void OnStop()
        {
            _thread.Abort();
        }

        private void OnClose_Click()
        {
            var list_p = _threadhandler.positionParam();
            var p_symbol = MarketData.GetSymbol(list_p[0]);
            var p_label = list_p[1];
            var p_volume = p_symbol.NormalizeVolumeInUnits(Convert.ToDouble(list_p[2]), RoundingMode.ToNearest);
            var p_comment = list_p[3];
            //if (string.IsNullOrEmpty(p_label))
            //{
            //    this.closeAllPositions();
            //}
            if (!string.IsNullOrEmpty(p_label))
                this.closeAllLabel(p_label);
        }
    }
}
