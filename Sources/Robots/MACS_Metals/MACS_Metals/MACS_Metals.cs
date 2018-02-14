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
    public class MACS_Metals : Robot
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

        private Metals_MAC _mac;
        private Metals_MAS _mas;
        private double _ratio;
        private Symbol _xausymbol;
        private Symbol _xagsymbol;
        private bool _abovecross;
        private bool _belowcross;
        private bool _risk;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _init;

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
            _mac = Indicators.GetIndicator<Metals_MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<Metals_MAS>(_resultperiods, _averageperiods, _sub);
            _ratio = 80;
            _xausymbol = MarketData.GetSymbol("XAUUSD");
            _xagsymbol = MarketData.GetSymbol("XAGUSD");
            _abovecross = false;
            _belowcross = false;
            _risk = false;

            _abovelabel = "Above" + "-" + "XAUXAG" + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + "XAUXAG" + "-" + MarketSeries.TimeFrame.ToString();

            _init = new OrderParams(null, null, null, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
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
            if (pos.Label != _abovelabel && pos.Label != _belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            _marklist.Add(pos.Comment.Substring(idx, 13));
            Print("It's successful to add a mark for XAUXAG.");
        }

        private void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            Position pos = obj.Position;
            if (pos.Label != _abovelabel && pos.Label != _belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            if (_marklist.Remove(pos.Comment.Substring(idx, 13)))
                Print("It's successful to remove a mark for XAUXAG.");
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
            List<Position> Poss_xau = new List<Position>();
            List<Position> Poss_xag = new List<Position>();
            if (Poss.Length != 0)
            {
                foreach (var p in Poss)
                {
                    if (p.SymbolCode == _xausymbol.Code)
                        Poss_xau.Add(p);
                    if (p.SymbolCode == _xagsymbol.Code)
                        Poss_xag.Add(p);
                }
                Poss_xau.OrderBy(p => p.EntryTime);
                Poss_xag.OrderBy(p => p.EntryTime);
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
                if (Poss_xau.Count >= 2 && Poss_xag.Count >= 2)
                {
                    var first_xau = Poss_xau[0];
                    var second_xau = Poss_xau[1];
                    var first_xag = Poss_xag[0];
                    var second_xag = Poss_xag[1];
                    Poss_xau.OrderByDescending(p => p.EntryTime);
                    Poss_xag.OrderByDescending(p => p.EntryTime);
                    var last0_xau = Poss_xau[0];
                    var last1_xau = Poss_xau[1];
                    var last0_xag = Poss_xag[0];
                    var last1_xag = Poss_xag[1];
                    var first_net = first_xau.NetProfit + first_xag.NetProfit;
                    var second_net = second_xau.NetProfit + second_xag.NetProfit;
                    var last0_net = last0_xau.NetProfit + last0_xag.NetProfit;
                    var last1_net = last1_xau.NetProfit + last1_xag.NetProfit;
                    if (last1_net < 0 && first_net + last0_net > 0)
                    {
                        this.ClosePosition(last0_xau);
                        this.ClosePosition(last0_xag);
                        this.ClosePosition(first_xau);
                        this.ClosePosition(first_xag);
                        _risk = false;
                        return;
                    }
                    else if (last1_net > 0)
                    {
                        this.ClosePosition(last0_xau);
                        this.ClosePosition(last0_xag);
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
                    _init.TradeType = TradeType.Sell;
                    _init.Symbol = _xausymbol;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    if (this.LastResult.IsSuccessful)
                    {
                        _init.TradeType = TradeType.Buy;
                        _init.Symbol = _xagsymbol;
                        _init.Volume = _init.Volume * _ratio;
                        this.executeOrder(_init);
                    }
                    _abovecross = false;
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Symbol = _xausymbol;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    if (this.LastResult.IsSuccessful)
                    {
                        _init.TradeType = TradeType.Buy;
                        _init.Symbol = _xagsymbol;
                        _init.Volume = _init.Volume * _ratio;
                        this.executeOrder(_init);
                    }
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Symbol = _xausymbol;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    if (this.LastResult.IsSuccessful)
                    {
                        _init.TradeType = TradeType.Sell;
                        _init.Symbol = _xagsymbol;
                        _init.Volume = _init.Volume * _ratio;
                        this.executeOrder(_init);
                    }
                    _belowcross = false;
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Symbol = _xausymbol;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    if (this.LastResult.IsSuccessful)
                    {
                        _init.TradeType = TradeType.Sell;
                        _init.Symbol = _xagsymbol;
                        _init.Volume = _init.Volume * _ratio;
                        this.executeOrder(_init);
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
            string Label = opensignal.Substring(0, 1).ToUpper();
            Label += opensignal.Substring(1, 4);
            Label += "-" + "XAUXAG" + "-" + MarketSeries.TimeFrame.ToString();
            var Poss = this.GetPositions(Label, Symbol);
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
