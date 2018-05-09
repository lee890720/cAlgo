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
    public class MACS_Magnify : Robot
    {
        [Parameter("FirstCross", DefaultValue = false)]
        public bool _firstCross { get; set; }
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
        private _Magnify_MAC _mac;
        private _Magnify_MAS _mas;
        private bool _abovecross;
        private bool _belowcross;
        private bool _risk;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _init;

        protected override void OnStart()
        {
            _filePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cAlgo\\cbotset\\";
            _fileName = _filePath + "cbotset.json";
            Print("fiName=" + _fileName);
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

            if (_firstCross)
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

            _abovelabel = "Above" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _init = new OrderParams(null, Symbol, _initvolume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });

            #region Get Mark
            Position[] pos_above = this.GetPositions(_abovelabel);
            Position[] pos_below = this.GetPositions(_belowlabel);
            var poss = pos_above.Length == 0 ? pos_below : pos_above;
            if (poss.Length != 0)
                foreach (var p in poss)
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
            #endregion
            Print("Done OnStart()");
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
            GetRisk();
            SetParams();
            if (_isChange)
            {
                _mac = Indicators.GetIndicator<_Magnify_MAC>(_resultperiods, _averageperiods, _magnify, _sub);
                _mas = Indicators.GetIndicator<_Magnify_MAS>(_resultperiods, _averageperiods, _magnify, _sub);
            }
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            Position[] pos_above = this.GetPositions(_abovelabel);
            Position[] pos_below = this.GetPositions(_belowlabel);
            var poss = pos_above.Length == 0 ? pos_below : pos_above;
            #endregion

            #region Cross
            if (pos_above.Length == 0)
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
            if (pos_below.Length == 0)
            {
                if (_belowcross == false)
                {
                    _belowcross = true;
                    Print("_belowcross: " + _belowcross.ToString());
                }
            }
            else
            {
                if (sr < sa)
                {
                    if (_belowcross == false)
                    {
                        _belowcross = true;
                        Print("_belowcross: " + _belowcross.ToString());
                    }
                }
            }
            #endregion

            #region Close
            //Risk
            if (_risk)
            {
                //Print("There is a risk for the current symbol.");
                if (poss.Length >= 2)
                {
                    var first = poss[0];
                    var second = poss[1];
                    var last0 = poss.OrderByDescending(p => p.EntryTime).ToArray()[0];
                    var last1 = poss.OrderByDescending(p => p.EntryTime).ToArray()[1];
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
            if (pos_above.Length != 0)
            {
                if (GetClose(_abovelabel))
                {
                    if (sr <= _sub / 5)
                    {
                        this.closeAllLabel(_abovelabel);
                        _risk = false;
                        Print("risk: " + _risk.ToString());
                    }
                }
                else
                {
                    if (sr <= 0)
                    {
                        this.closeAllLabel(_abovelabel);
                        _risk = false;
                        Print("risk: " + _risk.ToString());
                    }
                }
            }
            if (pos_below.Length != 0)
            {
                if (GetClose(_belowlabel))
                {
                    if (sr >= -_sub / 5)
                    {
                        this.closeAllLabel(_belowlabel);
                        _risk = false;
                        Print("risk: " + _risk.ToString());
                    }
                }
                else
                {
                    if (sr >= 0)
                    {
                        this.closeAllLabel(_belowlabel);
                        _risk = false;
                        Print("risk: " + _risk.ToString());
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
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolume(volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                    _abovecross = false;
                    Print("abovecross: " + _abovecross.ToString());
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Sell;
                    _init.Volume = Symbol.NormalizeVolume(volume, RoundingMode.ToNearest);
                    _init.Label = _abovelabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", pos_above.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolume(volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_000" + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
                    this.executeOrder(_init);
                    _belowcross = false;
                    Print("belowcross: " + _belowcross.ToString());
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var volume = GetOpenVolume(GetOpen());
                    _init.TradeType = TradeType.Buy;
                    _init.Volume = Symbol.NormalizeVolume(volume, RoundingMode.ToNearest);
                    _init.Label = _belowlabel;
                    _init.Comment = "CR_" + string.Format("{0:000000}", Math.Round(cr)) + "<";
                    _init.Comment += "BR_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _init.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _init.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _init.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _init.Comment += "P_" + string.Format("{0:000}", pos_below.Length + 1) + "<";
                    _init.Comment += "M_" + _mas.Mark + "<";
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
            string signal = null;
            Position[] pos_above = this.GetPositions(_abovelabel);
            Position[] pos_below = this.GetPositions(_belowlabel);
            var poss = pos_above.Length == 0 ? pos_below : pos_above;
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            var nowtime = DateTime.UtcNow;
            List<DateTime> lastpostime = new List<DateTime>();
            if (poss.Length != 0)
            {
                lastpostime.Add(this.LastPosition(poss).EntryTime.AddHours(_timer));
            }
            var pos_lasttime = lastpostime.Count == 0 ? DateTime.UtcNow.AddHours(-_timer) : lastpostime.Max();
            #endregion

            if (DateTime.Compare(nowtime, pos_lasttime) < 0)
                return null;
            if ((_isbreak && poss.Length != 0) || (_isbreak && _isbrkfirst))
            {
                if (sr >= GetBreak(_abovelabel))
                    return signal = "above_br";
                if (sr <= -GetBreak(_belowlabel))
                    return signal = "below_br";
            }
            var sig = _mas.SignalOne;
            if (sig == null)
            {
                return null;
            }

            if (!_marklist.Contains(_mas.Mark))
            {
                if (sig == "above" && _abovecross)
                {
                    signal = "above";
                    if (pos_above.Length != 0)
                    {
                        var idx = this.LastPosition(pos_above).Comment.IndexOf("CR_") + 3;
                        if (cr - GetDistance() < Convert.ToDouble(this.LastPosition(pos_above).Comment.Substring(idx, 6)))
                            signal = null;
                    }
                }
                if (sig == "below" && _belowcross)
                {
                    signal = "below";
                    if (pos_below.Length != 0)
                    {
                        var idx = this.LastPosition(pos_below).Comment.IndexOf("CR_") + 3;
                        if (cr + GetDistance() > Convert.ToDouble(this.LastPosition(pos_below).Comment.Substring(idx, 6)))
                            signal = null;
                    }
                }
            }
            return signal;
        }

        private double GetOpenVolume(string opensignal)
        {
            double volume = 0;
            if (opensignal == null)
                return _initvolume;
            string label = opensignal.Substring(0, 1).ToUpper() + opensignal.Substring(1, 4);
            label = label + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            var poss = this.GetPositions(label);
            if (poss.Length == 0)
                return _initvolume;
            List<Position> list_poss = new List<Position>();
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            foreach (var p in poss)
            {
                var idx = p.Comment.IndexOf("CR_") + 3;
                double pcr = Convert.ToDouble(p.Comment.Substring(idx, 6));
                if (pcr < ca && sr > 0)
                    list_poss.Add(p);
                if (pcr > ca & sr < 0)
                    list_poss.Add(p);
            }
            if (list_poss.Count > 0)
            {
                foreach (var p in list_poss)
                {
                    volume += p.Volume * 2;
                }
            }

            if (this.LastPosition(poss).Volume > volume)
                volume = this.LastPosition(poss).Volume;
            if (_initvolume > volume)
                volume = _initvolume;
            return volume;
        }

        private double GetDistance()
        {
            return _distance;
        }

        private bool GetClose(string label)
        {
            var poss = this.GetPositions(label, Symbol);
            if (poss.Length != 0)
            {
                int barsago = MarketSeries.barsAgo(this.FirstPosition(poss));
                if (barsago > 24 || poss.Length > 1)
                    return true;
            }
            return false;
        }

        private double GetBreak(string label)
        {
            var poss = this.GetPositions(label);
            var sr = Math.Abs(_mas.Result.LastValue);
            double br = _break;
            if (br < sr)
                br = Math.Floor(sr);
            if (poss.Length != 0)
            {
                foreach (var p in poss)
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
            Position[] pos_above = this.GetPositions(_abovelabel);
            Position[] pos_below = this.GetPositions(_belowlabel);
            var poss = pos_above.Length == 0 ? pos_below : pos_above;
            if (poss.Length == 0)
            {
                _risk = false;
                return;
            }

            List<Position> list_poss = new List<Position>();
            var cr = _mac.Result.LastValue;
            var ca = _mac.Average.LastValue;
            var sr = _mas.Result.LastValue;
            var sa = _mas.Average.LastValue;
            foreach (var p in poss)
            {
                var idx = p.Comment.IndexOf("CR_") + 3;
                double pcr = Convert.ToDouble(p.Comment.Substring(idx, 6));
                if (pcr < ca && sr > 0)
                    list_poss.Add(p);
                if (pcr > ca & sr < 0)
                    list_poss.Add(p);
            }
            if (list_poss.Count > 1)
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
                    }
                    if (_distance != d.Distance)
                    {
                        _distance = d.Distance;
                        Print("Distance: " + _distance.ToString() + "-" + _distance.GetType().ToString());
                    }
                    if (_istrade != d.IsTrade)
                    {
                        _istrade = d.IsTrade;
                        Print("IsTrade: " + _istrade.ToString() + "-" + _istrade.GetType().ToString());
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
        public string Signal { get; set; }
        public string Alike { get; set; }
    }
}
