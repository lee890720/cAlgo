using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using JsonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class TMACS_t : Robot
    {
        [Parameter("OpenCross", DefaultValue = false)]
        public bool _openCross { get; set; }

        [Parameter("StopClose", DefaultValue = false)]
        public bool _stopClose { get; set; }

        [Parameter("StopTrade", DefaultValue = false)]
        public bool _stopTrade { get; set; }

        #region Parameter
        private double _initvolume;
        private int _timer;
        private double _break;
        private double _distance;
        private bool _istrade;
        private bool _isbreak;
        private bool _isbrkfirst;
        private int _resultperiods;
        private int _averageperiods;
        private double _magnify;
        private double _sub;
        #endregion

        private string _filePath;
        private string _fileName;
        private bool _isChange;
        private MAC _mac;
        private MAS _mas;
        private bool _abovecross;
        private bool _belowcross;
        private bool _risk;
        private string _h_abovelabel, _h_belowlabel;
        private string _t_abovelabel, _t_belowlabel;
        private List<string> _h_marklist = new List<string>();
        private List<string> _t_marklist = new List<string>();
        private List<string> _t_a_marklist = new List<string>();
        private List<string> _t_b_marklist = new List<string>();
        private OrderParams _init;

        protected override void OnStart()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
            SetParams();
            _istrade = !_stopTrade;
            Print("IsTrade: " + _istrade.ToString() + "-" + _istrade.GetType().ToString());

            if (_magnify != 1)
            {
                Print("Please choose the MACS_Magnify.");
                this.Stop();
            }
            Positions.Opened += OnPositionsOpened;
            Positions.Closed += OnPositionsClosed;
            _mac = Indicators.GetIndicator<MAC>(_resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<MAS>(_resultperiods, _averageperiods, _sub, _break);

            if (_openCross)
            {
                _abovecross = true;
                _belowcross = true;
                Print("abovecross: " + _abovecross.ToString());
                Print("belowcross: " + _belowcross.ToString());
            }
            else
            {
                _abovecross = false;
                _belowcross = false;
                Print("abovecross: " + _abovecross.ToString());
                Print("belowcross: " + _belowcross.ToString());
            }

            _risk = false;
            Print("risk: " + _risk.ToString());

            _h_abovelabel = "HAbove" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _h_belowlabel = "HBelow" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _t_abovelabel = "TAbove" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _t_belowlabel = "TBelow" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _init = new OrderParams(null, Symbol, _initvolume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });

            #region Get Mark
            Position[] h_pos_above = this.GetPositions(_h_abovelabel);
            Position[] h_pos_below = this.GetPositions(_h_belowlabel);
            var h_poss = h_pos_above.Length == 0 ? h_pos_below : h_pos_above;
            if (h_poss.Length != 0)
                foreach (var p in h_poss)
                {
                    var idx = p.Comment.IndexOf("M_") + 2;
                    if (!_h_marklist.Contains(p.Comment.Substring(idx, 13)))
                        _h_marklist.Add(p.Comment.Substring(idx, 13));
                }
            if (_h_marklist.Count != 0)
            {
                foreach (var mar in _h_marklist)
                    Print("h: " + mar);
            }

            Position[] t_pos_above = this.GetPositions(_t_abovelabel);
            Position[] t_pos_below = this.GetPositions(_t_belowlabel);
            if (t_pos_above.Length != 0)
                foreach (var p in t_pos_above)
                {
                    var idx = p.Comment.IndexOf("M_") + 2;
                    if (!_t_a_marklist.Contains(p.Comment.Substring(idx, 13)))
                        _t_a_marklist.Add(p.Comment.Substring(idx, 13));
                }
            if (t_pos_below.Length != 0)
                foreach (var p in t_pos_below)
                {
                    var idx = p.Comment.IndexOf("M_") + 2;
                    if (!_t_b_marklist.Contains(p.Comment.Substring(idx, 13)))
                        _t_b_marklist.Add(p.Comment.Substring(idx, 13));
                }
            if (_t_a_marklist.Count != 0)
            {
                foreach (var mar in _t_a_marklist)
                    Print("t_a: " + mar);
            }
            if (_t_b_marklist.Count != 0)
            {
                foreach (var mar in _t_b_marklist)
                    Print("t_b: " + mar);
            }
            #endregion
            Print("Done OnStart()");
        }

        private void OnPositionsOpened(PositionOpenedEventArgs obj)
        {
            Position pos = obj.Position;
            if (pos.Label == _t_abovelabel)
            {
                var t_idx = pos.Comment.IndexOf("M_") + 2;
                _t_a_marklist.Add(pos.Comment.Substring(t_idx, 13));
                Print("It's successful to add a mark for T-A-" + Symbol.Code + ".");
            }
            if (pos.Label == _t_belowlabel)
            {
                var t_idx = pos.Comment.IndexOf("M_") + 2;
                _t_b_marklist.Add(pos.Comment.Substring(t_idx, 13));
                Print("It's successful to add a mark for T-B-" + Symbol.Code + ".");
            }
            if (pos.Label != _h_abovelabel && pos.Label != _h_belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            _h_marklist.Add(pos.Comment.Substring(idx, 13));
            Print("It's successful to add a mark for " + Symbol.Code + ".");
        }

        private void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            Position pos = obj.Position;
            if (pos.Label == _t_abovelabel)
            {
                var t_idx = pos.Comment.IndexOf("M_") + 2;
                if (_t_a_marklist.Remove(pos.Comment.Substring(t_idx, 13)))
                    Print("It's successful to remove a mark for T-A-" + Symbol.Code + ".");
            }
            if (pos.Label == _t_belowlabel)
            {
                var t_idx = pos.Comment.IndexOf("M_") + 2;
                if (_t_b_marklist.Remove(pos.Comment.Substring(t_idx, 13)))
                    Print("It's successful to remove a mark for T-B-" + Symbol.Code + ".");
            }
            if (pos.Label != _h_abovelabel && pos.Label != _h_belowlabel)
                return;
            var idx = pos.Comment.IndexOf("M_") + 2;
            if (_h_marklist.Remove(pos.Comment.Substring(idx, 13)))
                Print("It's successful to remove a mark for " + Symbol.Code + ".");
        }

        protected override void OnTick()
        {
            #region Parameter
            GetRisk();
            SetParams();
            if (_isChange)
            {
                _mac = Indicators.GetIndicator<MAC>(_resultperiods, _averageperiods, _sub);
                _mas = Indicators.GetIndicator<MAS>(_resultperiods, _averageperiods, _sub, _break);
            }
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            Position[] h_pos_above = this.GetPositions(_h_abovelabel);
            Position[] h_pos_below = this.GetPositions(_h_belowlabel);
            Position[] t_pos_above = this.GetPositions(_t_abovelabel);
            Position[] t_pos_below = this.GetPositions(_t_belowlabel);
            var h_poss = h_pos_above.Length == 0 ? h_pos_below : h_pos_above;
            #endregion

            #region Cross
            if (h_pos_above.Length == 0)
            {
                if (_abovecross == false)
                {
                    _abovecross = true;
                    Print("abovecross: " + _abovecross.ToString());
                }
            }
            else
            {
                if (sr > sa)
                {
                    if (_abovecross == false)
                    {
                        _abovecross = true;
                        Print("abovecross: " + _abovecross.ToString());
                    }
                }
            }
            if (h_pos_below.Length == 0)
            {
                if (_belowcross == false)
                {
                    _belowcross = true;
                    Print("belowcross: " + _belowcross.ToString());
                }
            }
            else
            {
                if (sr < sa)
                {
                    if (_belowcross == false)
                    {
                        _belowcross = true;
                        Print("belowcross: " + _belowcross.ToString());
                    }
                }
            }
            #endregion

            #region Close
            if (!_stopClose)
            {
                #region Risk
                if (_risk)
                {
                    if (h_poss.Length >= 2)
                    {
                        var first = h_poss[0];
                        var second = h_poss[1];
                        var last0 = h_poss.OrderByDescending(p => p.EntryTime).ToArray()[0];
                        var last1 = h_poss.OrderByDescending(p => p.EntryTime).ToArray()[1];
                        if (last1.NetProfit < 0 && first.NetProfit + last0.NetProfit > 0)
                        {
                            this.ClosePosition(last0);
                            this.ClosePosition(first);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                            return;
                        }
                        else if (last1.NetProfit > 0)
                        {
                            this.ClosePosition(last0);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                            return;
                        }
                    }
                }
                #endregion
                //above
                if (t_pos_above.Length != 0)
                {
                    if (sr <= 0 && this.TotalProfits(_t_abovelabel) > 0)
                        this.closeAllLabel(_t_abovelabel);

                }
                if (h_pos_above.Length != 0)
                {
                    if (GetClose(_h_abovelabel))
                    {
                        if (sr <= _sub / 5)
                        {
                            this.closeAllLabel(_h_abovelabel);
                            this.closeAllLabel(_t_belowlabel);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                        }
                    }
                    else
                    {
                        if (sr <= 0)
                        {
                            this.closeAllLabel(_h_abovelabel);
                            this.closeAllLabel(_t_belowlabel);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                        }
                    }
                }
                //below
                if (t_pos_below.Length != 0)
                {
                    if (sr >= 0 && this.TotalProfits(_t_belowlabel) > 0)
                        this.closeAllLabel(_t_belowlabel);

                }
                if (h_pos_below.Length != 0)
                {
                    if (GetClose(_h_belowlabel))
                    {
                        if (sr >= -_sub / 5)
                        {
                            this.closeAllLabel(_h_belowlabel);
                            this.closeAllLabel(_t_abovelabel);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                        }
                    }
                    else
                    {
                        if (sr >= 0)
                        {
                            this.closeAllLabel(_h_belowlabel);
                            this.closeAllLabel(_t_abovelabel);
                            _risk = false;
                            Print("risk: " + _risk.ToString());
                        }
                    }
                }
            }
            #endregion

            #region Trade
            if (_istrade)
            {
                #region Open
                #region Above
                if (GetOpen() == "below_t")
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _t_belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", 1) + "<";
                    _init.Comment += "M_" + _mac.Mark + "<";
                    this.executeOrder(_init);
                }
                if (GetOpen() == "above")
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _h_abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", h_pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                    _abovecross = false;
                    Print("abovecross: " + _abovecross.ToString());
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _h_abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_h_abovelabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", h_pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                }
                #endregion
                #region Below
                if (GetOpen() == "above_t")
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _t_abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", 1) + "<";
                    _init.Comment += "M_" + _mac.Mark + "<";
                    this.executeOrder(_init);
                }
                if (GetOpen() == "below")
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _h_belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", h_pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                    _belowcross = false;
                    Print("belowcross: " + _belowcross.ToString());
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolumeInUnits(volume, RoundingMode.ToNearest);
                    _init.Label = _h_belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_h_belowlabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", h_pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                }
                #endregion
                #endregion
            }
            #endregion

            #region Draw
            ChartObjects.DrawText("stop1", "fCross: " + "\t\tsClose: " + "\t\tsTrade: ", StaticPosition.TopLeft);
            ChartObjects.DrawText("stop2", "\t" + _openCross.ToString() + "\t\t" + _stopClose.ToString() + "\t\t" + _stopTrade.ToString(), StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("Cross1", "\naCross: " + "\t\tbCross: " + "\t\trisk: ", StaticPosition.TopLeft);
            ChartObjects.DrawText("Cross2", "\n\t" + _abovecross.ToString() + "\t\t" + _belowcross.ToString() + "\t\t" + _risk.ToString(), StaticPosition.TopLeft, Colors.Red);
            ChartObjects.DrawText("Close1", "\n\naCount: " + "\t\taClose: " + "\t\tbCount: " + "\t\tbClose: ", StaticPosition.TopLeft);
            ChartObjects.DrawText("Close2", "\n\n\t" + h_pos_above.Length.ToString() + "\t\t" + GetClose(_h_abovelabel).ToString() + "\t\t" + h_pos_below.Length.ToString() + "\t\t" + GetClose(_h_belowlabel).ToString(), StaticPosition.TopLeft, Colors.Red);
            #endregion
        }

        private string GetOpen()
        {
            if (!GetTradeTime())
                return null;

            #region Parameter
            string signal = null;
            Position[] h_pos_above = this.GetPositions(_h_abovelabel);
            Position[] h_pos_below = this.GetPositions(_h_belowlabel);
            Position[] t_pos_above = this.GetPositions(_t_abovelabel);
            Position[] t_pos_below = this.GetPositions(_t_belowlabel);
            var h_poss = h_pos_above.Length == 0 ? h_pos_below : h_pos_above;

            if (_mac.SignalTwo == "aboveTrend" && t_pos_above.Length == 0)
                signal = "above_t";
            if (_mac.SignalTwo == "belowTrend" && t_pos_below.Length == 0)
                signal = "below_t";
            if (!string.IsNullOrEmpty(signal))
                return signal;
            if (t_pos_above.Length == 0 && t_pos_below.Length == 0)
                return null;
            //if (_t_marklist.Contains(_mac.Mark))
            //    return null;
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            var nowtime = DateTime.UtcNow;
            List<DateTime> lastpostime = new List<DateTime>();
            if (h_poss.Length != 0)
            {
                lastpostime.Add(this.LastPosition(h_poss).EntryTime.AddHours(_timer));
            }
            var pos_lasttime = lastpostime.Count == 0 ? DateTime.UtcNow.AddHours(-_timer) : lastpostime.Max();
            #endregion

            if (DateTime.Compare(nowtime, pos_lasttime) < 0)
                return null;
            //string sig2 = _mas.SignalTwo;
            //var mas_brk = _mas.SigTwo.LastValue;
            //if (!string.IsNullOrEmpty(sig2))
            //{
            //    if ((_isbreak && h_poss.Length != 0) || (_isbreak && _isbrkfirst))
            //    {
            //        if (!double.IsNaN(mas_brk))
            //        {
            //            if (mas_brk >= GetBreak(_h_abovelabel) && sig2 == "aboveBreak" && t_pos_below.Length != 0)
            //            {
            //                signal = "above_br";
            //                if (h_pos_above.Length != 0)
            //                {
            //                    var idx = this.LastPosition(h_pos_above).Comment.IndexOf("CR_") + 3;
            //                    if (cr - _distance < Convert.ToDouble(this.LastPosition(h_pos_above).Comment.Substring(idx, 6)))
            //                        signal = null;
            //                }
            //            }
            //            if (mas_brk >= GetBreak(_h_belowlabel) && sig2 == "belowBreak" && t_pos_above.Length != 0)
            //            {
            //                signal = "below_br";
            //                if (h_pos_below.Length != 0)
            //                {
            //                    var idx = this.LastPosition(h_pos_below).Comment.IndexOf("CR_") + 3;
            //                    if (cr + _distance > Convert.ToDouble(this.LastPosition(h_pos_below).Comment.Substring(idx, 6)))
            //                        signal = null;
            //                }
            //            }
            //            if (!string.IsNullOrEmpty(signal))
            //                return signal;
            //        }
            //    }
            //}
            string sig1 = _mas.SignalOne;
            if (string.IsNullOrEmpty(sig1))
            {
                return null;
            }

            if (!_h_marklist.Contains(_mas.Mark))
            {
                if (sig1 == "above" && _abovecross && t_pos_below.Length != 0 && !_t_b_marklist.Contains(_mac.Mark))
                {
                    signal = "above";
                    if (h_pos_above.Length != 0)
                    {
                        var idx = this.LastPosition(h_pos_above).Comment.IndexOf("CR_") + 3;
                        if (cr - _distance < Convert.ToDouble(this.LastPosition(h_pos_above).Comment.Substring(idx, 6)))
                            signal = null;
                    }
                }
                if (sig1 == "below" && _belowcross && t_pos_above.Length != 0 && !_t_a_marklist.Contains(_mac.Mark))
                {
                    signal = "below";
                    if (h_pos_below.Length != 0)
                    {
                        var idx = this.LastPosition(h_pos_below).Comment.IndexOf("CR_") + 3;
                        if (cr + _distance > Convert.ToDouble(this.LastPosition(h_pos_below).Comment.Substring(idx, 6)))
                            signal = null;
                    }
                }
            }
            return signal;
        }

        private double GetOpenVolume(string opensignal)
        {
            double volume = 0;
            if (opensignal == null || opensignal == "above_t" || opensignal == "below_t")
                return _initvolume;
            string label = "H" + opensignal.Substring(0, 1).ToUpper() + opensignal.Substring(1, 4);
            label = label + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            var h_poss = this.GetPositions(label);
            if (h_poss.Length == 0)
                return _initvolume * 2;
            List<Position> list_h_poss = new List<Position>();
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            foreach (var p in h_poss)
            {
                var idx = p.Comment.IndexOf("CR_") + 3;
                double pcr = Convert.ToDouble(p.Comment.Substring(idx, 6));
                if (pcr < ca && sr > 0)
                    list_h_poss.Add(p);
                if (pcr > ca & sr < 0)
                    list_h_poss.Add(p);
            }
            if (list_h_poss.Count > 0)
            {
                foreach (var p in list_h_poss)
                {
                    volume += p.VolumeInUnits * 2;
                }
            }
            volume += _initvolume * 2;
            if (this.LastPosition(h_poss).VolumeInUnits > volume)
                volume = this.LastPosition(h_poss).VolumeInUnits;
            if (_initvolume > volume)
                volume = _initvolume;
            return volume;
        }

        private bool GetClose(string label)
        {
            var h_poss = this.GetPositions(label, Symbol);
            if (h_poss.Length != 0)
            {
                int barsago = MarketSeries.barsAgo(this.FirstPosition(h_poss));
                if (barsago > 24 || h_poss.Length > 1)
                    return true;
            }
            return false;
        }

        private double GetBreak(string label)
        {
            var h_poss = this.GetPositions(label);
            double mas_brk = _break;
            if (!double.IsNaN(_mas.SigTwo.LastValue))
                mas_brk = _mas.SigTwo.LastValue;
            double br = _break;
            if (br < mas_brk)
                br = Math.Floor(mas_brk);
            if (!_h_marklist.Contains(_mas.Mark))
            {
                return br;
            }
            if (h_poss.Length != 0)
            {
                foreach (var p in h_poss)
                {
                    var idx = p.Comment.IndexOf("BR_") + 3;
                    if (br < Convert.ToDouble(p.Comment.Substring(idx, 3)))
                        br = Convert.ToDouble(p.Comment.Substring(idx, 3));
                }
            }
            return br;
        }

        private bool GetTradeTime()
        {
            if (Symbol.Spread / Symbol.PipSize <= 2)
                return true;
            var now = DateTime.UtcNow;
            var hour = now.Hour;
            if (hour >= 20)
                return false;
            return true;
        }

        private void GetRisk()
        {
            Position[] h_pos_above = this.GetPositions(_h_abovelabel);
            Position[] h_pos_below = this.GetPositions(_h_belowlabel);
            var h_poss = h_pos_above.Length == 0 ? h_pos_below : h_pos_above;
            if (h_poss.Length == 0)
            {
                _risk = false;
                return;
            }

            List<Position> list_h_poss = new List<Position>();
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            foreach (var p in h_poss)
            {
                var idx = p.Comment.IndexOf("CR_") + 3;
                double pcr = Convert.ToDouble(p.Comment.Substring(idx, 6));
                if (pcr < ca && sr > 0)
                    list_h_poss.Add(p);
                if (pcr > ca & sr < 0)
                    list_h_poss.Add(p);
            }
            if (list_h_poss.Count > 1)
            {
                if (_risk == false)
                {
                    _risk = true;
                    Print("risk: " + _risk.ToString());
                }
            }
            else
            {
                if (_risk == true)
                {
                    _risk = false;
                    Print("risk: " + _risk.ToString());
                }
            }
        }

        private void SetParams()
        {
            string data = Json.ReadJsonFile(_fileName);
            var list_data = JsonConvert.DeserializeObject<List<FrxCbotset>>(data);
            foreach (var d in list_data)
            {
                if (d.Symbol == Symbol.Code)
                {
                    if (_initvolume != d.InitVolume)
                    {
                        _initvolume = d.InitVolume;
                        Print("InitVolume: " + _initvolume.ToString() + "-" + _initvolume.GetType().ToString());
                    }
                    if (_timer != d.Tmr)
                    {
                        _timer = d.Tmr;
                        Print("Timer: " + _timer.ToString() + "-" + _timer.GetType().ToString());
                    }
                    if (_break != d.Brk)
                    {
                        _break = d.Brk;
                        Print("Break: " + _break.ToString() + "-" + _break.GetType().ToString());
                        _isChange = true;
                    }
                    if (_distance != d.Distance)
                    {
                        _distance = d.Distance;
                        Print("Distance: " + _distance.ToString() + "-" + _distance.GetType().ToString());
                    }
                    if (_isbreak != d.IsBreak)
                    {
                        _isbreak = d.IsBreak;
                        Print("IsBreak: " + _isbreak.ToString() + "-" + _isbreak.GetType().ToString());
                    }
                    if (_isbrkfirst != d.IsBrkFirst)
                    {
                        _isbrkfirst = d.IsBrkFirst;
                        Print("BreakFirst: " + _isbrkfirst.ToString() + "-" + _isbrkfirst.GetType().ToString());
                    }
                    if (_resultperiods != d.Result)
                    {
                        _resultperiods = d.Result;
                        Print("ResultPeriods: " + _resultperiods.ToString() + "-" + _resultperiods.GetType().ToString());
                        _isChange = true;
                    }
                    if (_averageperiods != d.Average)
                    {
                        _averageperiods = d.Average;
                        Print("AveragePeriods: " + _averageperiods.ToString() + "-" + _averageperiods.GetType().ToString());
                        _isChange = true;
                    }
                    if (_magnify != d.Magnify)
                    {
                        _magnify = d.Magnify;
                        Print("Magnify: " + _magnify.ToString() + "-" + _magnify.GetType().ToString());
                        _isChange = true;
                    }
                    if (_sub != d.Sub)
                    {
                        _sub = d.Sub;
                        Print("Sub: " + _sub.ToString() + "-" + _sub.GetType().ToString());
                        _isChange = true;
                    }
                    break;
                }
            }
        }
    }

    public class FrxCbotset
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public int InitVolume { get; set; }
        public int Tmr { get; set; }
        public double Brk { get; set; }
        public double Distance { get; set; }
        public bool IsTrade { get; set; }
        public bool IsBreak { get; set; }
        public bool IsBrkFirst { get; set; }
        public int Result { get; set; }
        public int Average { get; set; }
        public double Magnify { get; set; }
        public double Sub { get; set; }
        public double? Cr { get; set; }
        public double? Ca { get; set; }
        public double? Sr { get; set; }
        public double? Sa { get; set; }
        public double? SrSa { get; set; }
        public string Signal { get; set; }
        public string Signal2 { get; set; }
    }
}
