using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class MACS_Magnify : Robot
    {
        #region Parameter
        private double _initvolume;
        private int _timer;
        private double _break;
        private double _distance;
        private bool _istrade;
        private bool _isbreak;
        private bool _breakfirst;
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        #endregion
        private string DataDir;
        private string fiName;
        private _Magnify_MAC _mac;
        private _Magnify_MAS _mas;
        private bool _abovecross;
        private bool _belowcross;
        private bool _risk;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _init;

        private void SetParams()
        {
            DataTable dt = new DataTable();
            if (!File.Exists(fiName))
                Thread.Sleep(1000);
            if (File.Exists(fiName))
                dt = CSVLib.CsvParsingHelper.CsvToDataTable(fiName, true);
            foreach (DataRow dr in dt.Rows)
            {
                if (dr["symbol"].ToString() == Symbol.Code)
                {
                    if (_initvolume != Convert.ToDouble(dr["initvolume"]))
                    {
                        _initvolume = Convert.ToDouble(dr["initvolume"]);
                        Print("Init_Volume: " + _initvolume.ToString() + "-" + _initvolume.GetType().ToString());
                    }
                    if (_timer != Convert.ToInt32(dr["tmr"]))
                    {
                        _timer = Convert.ToInt32(dr["tmr"]);
                        Print("Timer: " + _timer.ToString() + "-" + _timer.GetType().ToString());
                    }
                    if (_break != Convert.ToDouble(dr["brk"]))
                    {
                        _break = Convert.ToDouble(dr["brk"]);
                        Print("Break: " + _break.ToString() + "-" + _break.GetType().ToString());
                    }
                    if (_distance != Convert.ToDouble(dr["distance"]))
                    {
                        _distance = Convert.ToDouble(dr["distance"]);
                        Print("Distance: " + _distance.ToString() + "-" + _distance.GetType().ToString());
                    }
                    if (_istrade != Convert.ToBoolean(dr["istrade"]))
                    {
                        _istrade = Convert.ToBoolean(dr["istrade"]);
                        Print("IsTrade: " + _istrade.ToString() + "-" + _istrade.GetType().ToString());
                    }
                    if (_isbreak != Convert.ToBoolean(dr["isbreak"]))
                    {
                        _isbreak = Convert.ToBoolean(dr["isbreak"]);
                        Print("IsBreak: " + _isbreak.ToString() + "-" + _isbreak.GetType().ToString());
                    }
                    if (_breakfirst != Convert.ToBoolean(dr["breakfirst"]))
                    {
                        _breakfirst = Convert.ToBoolean(dr["breakfirst"]);
                        Print("BreakFirst: " + _breakfirst.ToString() + "-" + _breakfirst.GetType().ToString());
                    }
                    if (_resultperiods != Convert.ToInt32(dr["resultperiods"]))
                    {
                        _resultperiods = Convert.ToInt32(dr["resultperiods"]);
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                    }
                    if (_averageperiods != Convert.ToInt32(dr["averageperiods"]))
                    {
                        _averageperiods = Convert.ToInt32(dr["averageperiods"]);
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                    }
                    if (_magnify != Convert.ToDouble(dr["magnify"]))
                    {
                        _magnify = Convert.ToDouble(dr["magnify"]);
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                    }
                    if (_sub != Convert.ToDouble(dr["sub"]))
                    {
                        _sub = Convert.ToDouble(dr["sub"]);
                        Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                    }
                    break;
                }
            }
        }

        protected override void OnStart()
        {
            DataDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            fiName = DataDir + "\\" + "cBotSet.csv";
            Print("fiName=" + fiName);
            SetParams();
            if (_magnify == 1)
            {
                Print("Please choose the MACS.");
                this.Stop();
            }
            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionsClosed;
            _mac = Indicators.GetIndicator<_Magnify_MAC>(_resultperiods, _averageperiods, _magnify, _sub);
            _mas = Indicators.GetIndicator<_Magnify_MAS>(_resultperiods, _averageperiods, _magnify, _sub);
            _abovecross = true;
            _belowcross = true;
            _risk = false;
            _abovelabel = "Above" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _init = new OrderParams(null, Symbol, _initvolume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
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
            Print("Done OnStart()");
            #endregion
        }

        private void OnPositionsOpened(PositionOpenedEventArgs obj)
        {
            Position pos = obj.Position;
            if (pos.Label != _abovelabel && pos.Label != _belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            _marklist.Add(pos.Comment.Substring(idx, 13));
            Print("It's successful to add a mark for " + Symbol.Code + ".");
        }

        private void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            Position pos = obj.Position;
            if (pos.Label != _abovelabel && pos.Label != _belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            if (_marklist.Remove(pos.Comment.Substring(idx, 13)))
                Print("It's successful to remove a mark for " + Symbol.Code + ".");
        }

        protected override void OnTick()
        {
            #region Parameter
            SetParams();
            var CR = _mac.Result.LastValue;
            var CA = _mac.Average.LastValue;
            var SR = _mas.Result.LastValue;
            var SA = _mas.Average.LastValue;
            Position[] Pos_above = this.GetPositions(_abovelabel);
            Position[] Pos_below = this.GetPositions(_belowlabel);
            var Poss = Pos_above.Length == 0 ? Pos_below : Pos_above;
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
                if (Poss.Length >= 2)
                {
                    var first = Poss[0];
                    var second = Poss[1];
                    Poss.OrderByDescending(p => p.EntryTime);
                    var last0 = Poss[0];
                    var last1 = Poss[1];
                    if (last1.NetProfit < 0 && first.NetProfit + last0.NetProfit > 0)
                    {
                        this.ClosePosition(last0);
                        this.ClosePosition(first);
                        _risk = false;
                        return;
                    }
                    else if (last1.NetProfit > 0)
                    {
                        this.ClosePosition(last0);
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
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    _abovecross = false;
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                    _belowcross = false;
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var Volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolume(Volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_init);
                }
                #endregion
                #endregion
            }
        }

        private string GetOpen()
        {
            if (!GetTradeTime())
                return null;
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
            if ((_isbreak && Poss.Length != 0) || (_isbreak && _breakfirst))
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
            string Label = opensignal.Substring(0, 1).ToUpper() + opensignal.Substring(1, 4);
            Label = Label + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            var Poss = this.GetPositions(Label);
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
            if (_initvolume > Volume)
                Volume = _initvolume;
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
            var Poss = this.GetPositions(label);
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

        private bool GetTradeTime()
        {
            var now = DateTime.UtcNow;
            var hour = now.Hour;
            if (hour >= 20)
                return false;
            return true;
        }
    }
}
