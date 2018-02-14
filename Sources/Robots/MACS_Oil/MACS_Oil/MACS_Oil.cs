using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class MACS_Oil : Robot
    {
        #region Parameter
        private double _initvolume;
        private int _timer;
        private double _break;
        private double _distance;
        private bool _istrade;
        private bool _isbreak;
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        #endregion

        private Oil_MAC _mac;
        private Oil_MAS _mas;
        private Symbol _xbrsymbol;
        private Symbol _xtisymbol;
        private bool _abovecross;
        private bool _belowcross;
        private bool _risk;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _initbuy, _initsell;

        protected override void OnStart()
        {
            #region Set Paramters
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "Data Source=bds121909490.my3w.com;Initial Catalog=bds121909490_db;User ID=bds121909490;Password=lee37355175";
            con.Open();
            DataSet dataset = new DataSet();
            string strsql = "select * from CBotSet where symbol='" + Symbol.Code + "'";
            SqlDataAdapter objdataadpater = new SqlDataAdapter(strsql, con);
            SqlCommandBuilder sql = new SqlCommandBuilder(objdataadpater);
            objdataadpater.SelectCommand.CommandTimeout = 300;
            objdataadpater.Fill(dataset, "cbotset");
            DataTable dt = dataset.Tables["cbotset"];
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == Symbol.Code)
                {
                    _initvolume = Convert.ToDouble(dr["initvolume"]);
                    _timer = Convert.ToInt32(dr["tmr"]);
                    _break = Convert.ToDouble(dr["brk"]);
                    _distance = Convert.ToDouble(dr["distance"]);
                    _istrade = Convert.ToBoolean(dr["istrade"]);
                    _isbreak = Convert.ToBoolean(dr["isbreak"]);
                    _resultperiods = Convert.ToInt32(dr["resultperiods"]);
                    _averageperiods = Convert.ToInt32(dr["averageperiods"]);
                    _magnify = Convert.ToDouble(dr["magnify"]);
                    _sub = Convert.ToDouble(dr["sub"]);
                    Print("Init_Volume: " + _initvolume.ToString() + "-" + _initvolume.GetType().ToString());
                    Print("Timer: " + _timer.ToString() + "-" + _timer.GetType().ToString());
                    Print("Break: " + _break.ToString() + "-" + _break.GetType().ToString());
                    Print("Distance: " + _distance.ToString() + "-" + _distance.GetType().ToString());
                    Print("IsTrade: " + _istrade.ToString() + "-" + _istrade.GetType().ToString());
                    Print("IsBreak: " + _isbreak.ToString() + "-" + _isbreak.GetType().ToString());
                    Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                    Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                    Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                    break;
                }
            }
            con.Close();
            con.Dispose();
            #endregion

            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionsClosed;
            _mac = Indicators.GetIndicator<Oil_MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<Oil_MAS>(_resultperiods, _averageperiods, _sub);
            _xbrsymbol = MarketData.GetSymbol("XBRUSD");
            _xtisymbol = MarketData.GetSymbol("XTIUSD");
            _abovecross = false;
            _belowcross = false;
            _risk = false;

            _abovelabel = "Above" + "-" + "XBRXTI" + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + "XBRXTI" + "-" + MarketSeries.TimeFrame.ToString();

            _initbuy = new OrderParams(TradeType.Buy, null, null, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            _initsell = new OrderParams(TradeType.Sell, null, null, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            #region Get Mark
            Position[] Pos_above = this.GetPositions(_abovelabel);
            Position[] Pos_below = this.GetPositions(_belowlabel);
            var Poss = Pos_above.Length == 0 ? Pos_below : Pos_above;
            if (Poss.Length != 0)
                foreach (var p in Poss)
                {
                    var idx = p.Comment.IndexOf("M_") + 2;
                    if (!_marklist.Contains(p.Comment.Substring(idx, 13)))
                        _marklist.Add(p.Comment.Substring(idx, 13));
                }
            if (_marklist.Count != 0)
            {
                foreach (var mar in _marklist)
                    Print(mar);
            }
            Print("The cbot is ok.");
            #endregion
        }

        private void OnPositionsOpened(PositionOpenedEventArgs obj)
        {
            Position pos = obj.Position;
            var idx = pos.Comment.IndexOf("M_") + 2;
            _marklist.Add(pos.Comment.Substring(idx, 13));
            Print("It's successful to add a mark.");
        }

        private void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            Position pos = obj.Position;
            var idx = pos.Comment.IndexOf("M_") + 2;
            if (_marklist.Remove(pos.Comment.Substring(idx, 13)))
                Print("It's successful to remove a mark.");
        }

        protected override void OnTick()
        {
            #region Parameter
            var CR = _mac.Result.LastValue;
            var CA = _mac.Average.LastValue;
            var SR = _mas.Result.LastValue;
            var SA = _mas.Average.LastValue;
            Position[] Pos_above = this.GetPositions(_abovelabel);
            Position[] Pos_below = this.GetPositions(_belowlabel);
            var Poss = Pos_above.Length == 0 ? Pos_below : Pos_above;
            List<Position> Poss_xbr = new List<Position>();
            List<Position> Poss_xti = new List<Position>();
            if (Poss.Length != 0)
            {
                foreach (var p in Poss)
                {
                    if (p.SymbolCode == _xbrsymbol.Code)
                        Poss_xbr.Add(p);
                    if (p.SymbolCode == _xtisymbol.Code)
                        Poss_xti.Add(p);
                }
                Poss_xbr.OrderBy(p => p.EntryTime);
                Poss_xti.OrderBy(p => p.EntryTime);
            }
            #endregion

            #region Cross
            if (Pos_above.Length == 0)
                _abovecross = true;
            else
            {
                if (SR > SA)
                    _abovecross = true;
            }
            if (Pos_below.Length == 0)
                _belowcross = true;
            else
            {
                if (SR < SA)
                    _belowcross = true;
            }
            #endregion

            #region Close
            //Risk
            if (_risk)
            {
                Print("There is a risk for the current symbol.");
                if (Poss_xbr.Count >= 2 && Poss_xti.Count >= 2)
                {
                    var first_xbr = Poss_xbr[0];
                    var second_xbr = Poss_xbr[1];
                    var first_xti = Poss_xti[0];
                    var second_xti = Poss_xti[1];
                    Poss_xbr.OrderByDescending(p => p.EntryTime);
                    Poss_xti.OrderByDescending(p => p.EntryTime);
                    var last0_xbr = Poss[0];
                    var last1_xbr = Poss[1];
                    var last0_xti = Poss[0];
                    var last1_xti = Poss[1];
                    var first_net = first_xbr.NetProfit + first_xti.NetProfit;
                    var second_net = second_xbr.NetProfit + second_xti.NetProfit;
                    var last0_net = last0_xbr.NetProfit + last0_xti.NetProfit;
                    var last1_net = last1_xbr.NetProfit + last1_xti.NetProfit;
                    if (last1_net < 0 && first_net + last0_net > 0)
                    {
                        this.ClosePosition(last0_xbr);
                        this.ClosePosition(last0_xti);
                        this.ClosePosition(first_xbr);
                        this.ClosePosition(first_xti);
                        _risk = false;
                        return;
                    }
                    else if (last1_net > 0)
                    {
                        this.ClosePosition(last0_xbr);
                        this.ClosePosition(last0_xti);
                        _risk = false;
                        return;
                    }
                }
            }
            if (Pos_above.Length != 0)
            {
                if (GetClose(_abovelabel))
                {
                    if (SR <= _sub / 5)
                    {
                        this.closeAllLabel(_abovelabel);
                        _risk = false;
                    }
                }
                else
                {
                    if (SR <= 0)
                    {
                        this.closeAllLabel(_abovelabel);
                        _risk = false;
                    }
                }
            }
            if (Pos_below.Length != 0)
            {
                if (GetClose(_belowlabel))
                {
                    if (SR >= -_sub / 5)
                    {
                        this.closeAllLabel(_belowlabel);
                        _risk = false;
                    }
                }
                else
                {
                    if (SR >= 0)
                    {
                        this.closeAllLabel(_belowlabel);
                        _risk = false;
                    }
                }
            }
            #endregion

            if (_istrade)
            {
                #region Open
                #region Above
                if (GetOpen() == "above")
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _initsell.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += "BR_000" + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "P_" + string.Format("{0:000}", Poss_xbr.Count + 1) + "<";
                    _initsell.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initsell);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initbuy.Symbol = _xtisymbol;
                        _initbuy.Volume = _initsell.Volume;
                        _initbuy.Label = _initsell.Label;
                        _initbuy.Comment = _initsell.Comment;
                        this.executeOrder(_initbuy);
                    }
                    _abovecross = false;
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _initsell.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += "BR_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "P_" + string.Format("{0:000}", Poss_xbr.Count + 1) + "<";
                    _initsell.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initsell);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initbuy.Symbol = _xtisymbol;
                        _initbuy.Volume = _initsell.Volume;
                        _initbuy.Label = _initsell.Label;
                        _initbuy.Comment = _initsell.Comment;
                        this.executeOrder(_initbuy);
                    }
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _initbuy.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += "BR_000" + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "P_" + string.Format("{0:000}", Poss_xbr.Count + 1) + "<";
                    _initbuy.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initbuy);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initsell.Symbol = _xtisymbol;
                        _initsell.Volume = _initbuy.Volume;
                        _initsell.Label = _initbuy.Label;
                        _initsell.Comment = _initbuy.Comment;
                        this.executeOrder(_initsell);
                    }
                    _belowcross = false;
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _initbuy.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += "BR_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "P_" + string.Format("{0:000}", Poss_xbr.Count + 1) + "<";
                    _initbuy.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initbuy);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initsell.Symbol = _xtisymbol;
                        _initsell.Volume = _initbuy.Volume;
                        _initsell.Label = _initbuy.Label;
                        _initsell.Comment = _initbuy.Comment;
                        this.executeOrder(_initsell);
                    }
                }
                #endregion
                #endregion
            }
        }

        private string GetOpen()
        {
            #region Parameter
            string Signal = null;
            Position[] Pos_above = this.GetPositions(_abovelabel);
            Position[] Pos_below = this.GetPositions(_belowlabel);
            var Poss = Pos_above.Length == 0 ? Pos_below : Pos_above;
            var CR = _mac.Result.LastValue;
            var CA = _mac.Average.LastValue;
            var SR = _mas.Result.LastValue;
            var SA = _mas.Average.LastValue;
            var NowTime = DateTime.UtcNow;
            List<DateTime> LastPosTime = new List<DateTime>();
            if (Poss.Length != 0)
            {
                LastPosTime.Add(this.LastPosition(Poss).EntryTime.AddHours(_timer));
            }
            var Pos_LastTime = LastPosTime.Count == 0 ? DateTime.UtcNow.AddHours(-_timer) : LastPosTime.Max();
            #endregion

            if (DateTime.Compare(NowTime, Pos_LastTime) < 0)
                return null;
            if (_isbreak && Poss.Length != 0)
            {
                if (SR >= GetBreak(_abovelabel))
                    return Signal = "above_br";
                if (SR <= -GetBreak(_belowlabel))
                    return Signal = "below_br";
            }
            var Sig = _mas._Signal1;
            if (Sig == null)
            {
                return null;
            }

            if (!_marklist.Contains(_mas._Mark))
            {
                if (Sig == "above" && _abovecross)
                {
                    Signal = "above";
                    if (Pos_above.Length != 0)
                    {
                        var idx = this.LastPosition(Pos_above).Comment.IndexOf("CR_") + 3;
                        if (CR - GetDistance() < Convert.ToDouble(this.LastPosition(Pos_above).Comment.Substring(idx, 6)))
                            Signal = null;
                    }
                }
                if (Sig == "below" && _belowcross)
                {
                    Signal = "below";
                    if (Pos_below.Length != 0)
                    {
                        var idx = this.LastPosition(Pos_below).Comment.IndexOf("CR_") + 3;
                        if (CR + GetDistance() > Convert.ToDouble(this.LastPosition(Pos_below).Comment.Substring(idx, 6)))
                            Signal = null;
                    }
                }
            }
            return Signal;
        }

        private double GetOpenVolume(string opensignal)
        {
            double Volume = 0;
            if (opensignal == null)
                return _initvolume;
            string Label = opensignal.Substring(0, 5);
            var Poss = this.GetPositions(Label, _xbrsymbol);
            if (Poss.Length == 0)
                return _initvolume;
            List<Position> List_Poss = new List<Position>();
            var CR = _mac.Result.LastValue;
            var CA = _mac.Average.LastValue;
            var SR = _mas.Result.LastValue;
            var SA = _mas.Average.LastValue;
            foreach (var p in Poss)
            {
                var IDX = p.Comment.IndexOf("CR_") + 3;
                double PCR = Convert.ToDouble(p.Comment.Substring(IDX, 6));
                if (PCR < CA && SR > 0)
                    List_Poss.Add(p);
                if (PCR > CA & SR < 0)
                    List_Poss.Add(p);
            }
            if (List_Poss.Count > 0)
            {
                foreach (var p in List_Poss)
                {
                    Volume += p.Volume * 2;
                }
            }
            if (List_Poss.Count > 1)
            {
                _risk = true;
            }
            else
            {
                _risk = false;
            }

            if (this.LastPosition(Poss).Volume > Volume)
                Volume = this.LastPosition(Poss).Volume;
            return Volume;
        }

        private double GetDistance()
        {
            return _distance;
        }

        private bool GetClose(string label)
        {
            var Poss = this.GetPositions(label, Symbol);
            if (Poss.Length != 0)
            {
                int BarsAgo = MarketSeries.barsAgo(this.FirstPosition(Poss));
                if (BarsAgo > 24 || Poss.Length > 1)
                    return true;
            }
            return false;
        }

        private double GetBreak(string label)
        {
            var Poss = this.GetPositions(label, Symbol);
            var SR = Math.Abs(_mas.Result.LastValue);
            double BR = _break;
            if (BR < SR)
                BR = Math.Floor(SR);
            if (Poss.Length != 0)
            {
                foreach (var p in Poss)
                {
                    var idx = p.Comment.IndexOf("BR_") + 3;
                    if (BR < Convert.ToDouble(p.Comment.Substring(idx, 3)))
                        BR = Convert.ToDouble(p.Comment.Substring(idx, 3));
                }
            }
            return BR;
        }
    }
}
