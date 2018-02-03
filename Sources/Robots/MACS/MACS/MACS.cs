using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACS : Robot
    {
        #region Parameter
        [Parameter("INIT_Volume", DefaultValue = 1000, MinValue = 1000)]
        public double _initvolume { get; set; }

        [Parameter("Timer", DefaultValue = 1)]
        public int _timer { get; set; }

        [Parameter("Break", DefaultValue = 100)]
        public double _break { get; set; }

        [Parameter("Distance", DefaultValue = 20)]
        public double _distance { get; set; }

        [Parameter(DefaultValue = false)]
        public bool _istrade { get; set; }

        [Parameter(DefaultValue = false)]
        public bool _isbreak { get; set; }
        #endregion

        #region Indicators
        [Parameter("MA Type")]
        public MovingAverageType _matype { get; set; }

        [Parameter("SourceSeries")]
        public DataSeries _sourceseries { get; set; }

        [Parameter("Result Periods", DefaultValue = 1)]
        public int _resultperiods { get; set; }

        [Parameter("Average Periods", DefaultValue = 120)]
        public int _averageperiods { get; set; }

        [Parameter("Sub", DefaultValue = 30)]
        public double _sub { get; set; }
        #endregion

        private MAC _mac;
        private MAS _mas;
        private bool _abovecross;
        private bool _belowcross;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _initbuy, _initsell;

        protected override void OnStart()
        {
            _mac = Indicators.GetIndicator<MAC>(_matype, _sourceseries, _resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<MAS>(_matype, _sourceseries, _resultperiods, _averageperiods, _sub);

            _abovecross = false;
            _belowcross = false;

            _abovelabel = "Above" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + Symbol.Code + "-" + MarketSeries.TimeFrame.ToString();

            _initbuy = new OrderParams(TradeType.Buy, Symbol, _initvolume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            _initsell = new OrderParams(TradeType.Sell, Symbol, _initvolume, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });
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
            var PossLabel = this.FirstPosition(Poss).Label;
            #endregion

            #region Mark
            if (Poss.Length != 0)
                foreach (var p in Poss)
                {
                    if (!_marklist.Contains(p.Comment.Substring(15, 13)))
                        _marklist.Add(p.Comment.Substring(15, 13));
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
            if (this.Risk())
            {
                Print("There is a risk for the current symbol.");
                if (Poss.Length >= 2)
                {
                    if (this.FirstPosition(Poss).NetProfit + this.LastPosition(Poss).NetProfit > 0)
                    {
                        this.ClosePosition(this.FirstPosition(Poss));
                        this.ClosePosition(this.LastPosition(Poss));
                    }
                }
            }
            else if (this.PreRisk())
            {
                Print("There is a pre_risk for thi current symbol.");
                if (Poss.Length >= 2)
                {
                    var Pos_Last = this.LastPosition(Poss);
                    var List_Pre = Poss.ToList();
                    if (List_Pre.Remove(Pos_Last))
                    {
                        var Poss_Pre = List_Pre.ToArray();
                        var Pos_Pre = this.LastPosition(Poss_Pre);
                        if (Pos_Pre.NetProfit > 0)
                            this.ClosePosition(Pos_Last);
                    }
                }
            }
            if (Pos_above.Length != 0)
            {
                if (GetClose(_abovelabel))
                {
                    if (SR <= _sub / 5)
                        this.closeAllLabel(_abovelabel);
                }
                else
                {
                    if (SR <= 0)
                        this.closeAllLabel(_abovelabel);
                }
            }
            if (Pos_below.Length != 0)
            {
                if (GetClose(_belowlabel))
                {
                    if (SR >= -_sub / 5)
                        this.closeAllLabel(_belowlabel);
                }
                else
                {
                    if (SR >= 0)
                        this.closeAllLabel(_belowlabel);
                }
            }
            #endregion

            if (_istrade)
            {
                #region Open
                #region Above
                if (GetOpen() == "above")
                {
                    var InitVolume = _initvolume;
                    if (Pos_above.Length != 0)
                    {
                        InitVolume = this.LastPosition(Pos_above).Volume * 2;
                    }
                    _initsell.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += string.Format("{0:000}", GetDistance()) + "<";
                    _initsell.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    _initsell.Comment += _mas._Mark + "<";
                    _initsell.Comment += "nul000" + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    this.executeOrder(_initsell);
                    _abovecross = false;
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var InitVolume = _initvolume;
                    if (Pos_above.Length != 0)
                    {
                        InitVolume = this.LastPosition(Pos_above).Volume;
                    }
                    _initsell.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += string.Format("{0:000}", GetDistance()) + "<";
                    _initsell.Comment += string.Format("{0:000}", Pos_above.Length + 1) + "<";
                    _initsell.Comment += _mas._Mark + "<";
                    _initsell.Comment += "br_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    this.executeOrder(_initsell);
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var InitVolume = _initvolume;
                    if (Pos_below.Length != 0)
                    {
                        InitVolume = this.LastPosition(Pos_below).Volume * 2;
                    }
                    _initbuy.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += string.Format("{0:000}", GetDistance()) + "<";
                    _initbuy.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    _initbuy.Comment += _mas._Mark + "<";
                    _initbuy.Comment += "nul000" + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    this.executeOrder(_initbuy);
                    _belowcross = false;
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var InitVolume = _initvolume;
                    if (Pos_below.Length != 0)
                    {
                        InitVolume = this.LastPosition(Pos_below).Volume;
                    }
                    _initbuy.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += string.Format("{0:000}", GetDistance()) + "<";
                    _initbuy.Comment += string.Format("{0:000}", Pos_below.Length + 1) + "<";
                    _initbuy.Comment += _mas._Mark + "<";
                    _initbuy.Comment += "br_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    this.executeOrder(_initbuy);
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
            var PossLabel = this.FirstPosition(Poss).Label;
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
            if (_isbreak)
            {
                if (SR >= GetBreak(_abovelabel))
                    return Signal = "above_br";
                if (SR <= -GetBreak(_belowlabel))
                    return Signal = "below_br";
            }
            var Sig = _mas._Signal;
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
                        if (CR - GetDistance() < Convert.ToDouble(this.LastPosition(Pos_above).Comment.Substring(0, 6)))
                            Signal = null;
                    }
                }
                if (Sig == "below" && _belowcross)
                {
                    Signal = "below";
                    if (Pos_below.Length != 0)
                    {
                        if (CR + GetDistance() > Convert.ToDouble(this.LastPosition(Pos_below).Comment.Substring(0, 6)))
                            Signal = null;
                    }
                }
            }
            return Signal;
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
                    if (p.Comment.Substring(29, 3) == "br_")
                    {
                        if (BR < Convert.ToDouble(p.Comment.Substring(32, 3)))
                            BR = Convert.ToDouble(p.Comment.Substring(32, 3));
                    }
                }
            }
            return BR;
        }
    }
}
