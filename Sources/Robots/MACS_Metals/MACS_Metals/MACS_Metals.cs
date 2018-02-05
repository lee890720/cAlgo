using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.Lib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MACS_Metals : Robot
    {
        #region Parameter
        [Parameter("INIT_Volume", DefaultValue = 1, MinValue = 1)]
        public double _initvolume { get; set; }

        [Parameter("Timer", DefaultValue = 1)]
        public int _timer { get; set; }

        [Parameter("Break", DefaultValue = 180)]
        public double _break { get; set; }

        [Parameter("Distance", DefaultValue = 30)]
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

        private Metals_MAC _mac;
        private Metals_MAS _mas;
        private double _ratio;
        private Symbol _xausymbol;
        private Symbol _xagsymbol;
        private bool _abovecross;
        private bool _belowcross;
        private string _abovelabel, _belowlabel;
        private List<string> _marklist = new List<string>();
        private OrderParams _initbuy, _initsell;

        protected override void OnStart()
        {
            _mac = Indicators.GetIndicator<Metals_MAC>(_matype, _sourceseries, _resultperiods, _averageperiods, _sub);
            _mas = Indicators.GetIndicator<Metals_MAS>(_matype, _sourceseries, _resultperiods, _averageperiods, _sub);
            _ratio = _mac._Ratio;
            _xausymbol = MarketData.GetSymbol("XAUUSD");
            _xagsymbol = MarketData.GetSymbol("XAGUSD");
            if (Symbol.Code != _xausymbol.Code)
            {
                Print("Please choose XAUUSD.");
                this.Stop();
            }

            _abovecross = false;
            _belowcross = false;

            _abovelabel = "Above" + "-" + "XAUXAG" + "-" + MarketSeries.TimeFrame.ToString();
            _belowlabel = "Below" + "-" + "XAUXAG" + "-" + MarketSeries.TimeFrame.ToString();

            _initbuy = new OrderParams(TradeType.Buy, null, null, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
            {
                            });
            _initsell = new OrderParams(TradeType.Sell, null, null, null, null, null, null, null, null, new System.Collections.Generic.List<double> 
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
            //var PossLabel = this.FirstPosition(Poss).Label;
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
            }
            #endregion

            #region Mark
            if (Poss.Length != 0)
                foreach (var p in Poss)
                {
                    var idx = p.Comment.IndexOf("M_") + 2;
                    if (!_marklist.Contains(p.Comment.Substring(idx, 13)))
                        _marklist.Add(p.Comment.Substring(idx, 13));
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
            if (Poss_xau.Count >= 2 && Poss_xag.Count >= 2)
            {
                var Pos_xau_Last = this.LastPosition(Poss_xau.ToArray());
                var Pos_xag_Last = this.LastPosition(Poss_xag.ToArray());
                var Pos_xau_Pre = Pos_xau_Last;
                var Pos_xag_Pre = Pos_xag_Last;
                var List_xau_Pre = Poss_xau;
                var List_xag_Pre = Poss_xag;
                List<Position> Temp_xau = new List<Position>();
                List<Position> Temp_xag = new List<Position>();
                if (List_xau_Pre.Remove(Pos_xau_Last))
                {
                    Temp_xau = List_xau_Pre;
                    Pos_xau_Pre = this.LastPosition(Temp_xau.ToArray());
                }
                if (List_xag_Pre.Remove(Pos_xag_Last))
                {
                    Temp_xag = List_xag_Pre;
                    Pos_xag_Pre = this.LastPosition(Temp_xag.ToArray());
                }
                if (Pos_xau_Pre.NetProfit + Pos_xag_Pre.NetProfit > 0)
                {
                    var idx = Pos_xau_Last.Comment.IndexOf("M_") + 2;
                    if (_marklist.Contains(Pos_xau_Last.Comment.Substring(idx, 13)))
                        if (_marklist.Remove(Pos_xau_Last.Comment.Substring(idx, 13)))
                            this.ClosePosition(Pos_xau_Last);
                    if (LastResult.IsSuccessful)
                        this.ClosePosition(Pos_xag_Last);
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
                        InitVolume = this.LastPosition(Poss_xau.ToArray()).Volume * 2;
                    }
                    _initsell.Symbol = _xausymbol;
                    _initsell.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += "BR_000" + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _initsell.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initsell);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initbuy.Symbol = _xagsymbol;
                        _initbuy.Volume = _initsell.Volume * _ratio;
                        _initbuy.Label = _initsell.Label;
                        _initbuy.Comment = _initsell.Comment;
                        this.executeOrder(_initbuy);
                    }
                    _abovecross = false;
                }
                if (GetOpen() == "above_br" && _isbreak)
                {
                    var InitVolume = _initvolume;
                    if (Pos_above.Length != 0)
                    {
                        InitVolume = this.LastPosition(Poss_xau.ToArray()).Volume;
                    }
                    _initsell.Symbol = _xausymbol;
                    _initsell.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initsell.Label = _abovelabel;
                    _initsell.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initsell.Comment += "BR_" + string.Format("{0:000}", GetBreak(_abovelabel) + _distance) + "<";
                    _initsell.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initsell.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initsell.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initsell.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _initsell.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initsell);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initbuy.Symbol = _xagsymbol;
                        _initbuy.Volume = _initsell.Volume * _ratio;
                        _initbuy.Label = _initsell.Label;
                        _initbuy.Comment = _initsell.Comment;
                        this.executeOrder(_initbuy);
                    }
                }
                #endregion
                #region Below
                if (GetOpen() == "below")
                {
                    var InitVolume = _initvolume;
                    if (Pos_below.Length != 0)
                    {
                        InitVolume = this.LastPosition(Poss_xau.ToArray()).Volume * 2;
                    }
                    _initbuy.Symbol = _xausymbol;
                    _initbuy.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += "BR_000" + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _initbuy.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initbuy);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initsell.Symbol = _xagsymbol;
                        _initsell.Volume = _initbuy.Volume * _ratio;
                        _initsell.Label = _initbuy.Label;
                        _initsell.Comment = _initbuy.Comment;
                        this.executeOrder(_initsell);
                    }
                    _belowcross = false;
                }
                if (GetOpen() == "below_br" && _isbreak)
                {
                    var InitVolume = _initvolume;
                    if (Pos_below.Length != 0)
                    {
                        InitVolume = this.LastPosition(Poss_xau.ToArray()).Volume;
                    }
                    _initbuy.Symbol = _xausymbol;
                    _initbuy.Volume = Symbol.NormalizeVolume(InitVolume, RoundingMode.ToNearest);
                    _initbuy.Label = _belowlabel;
                    _initbuy.Comment = "CR_" + string.Format("{0:000000}", Math.Round(CR)) + "<";
                    _initbuy.Comment += "BR_" + string.Format("{0:000}", GetBreak(_belowlabel) + _distance) + "<";
                    _initbuy.Comment += "D_" + string.Format("{0:000}", _distance) + "<";
                    _initbuy.Comment += "S_" + string.Format("{0:000}", _sub) + "<";
                    _initbuy.Comment += "B_" + string.Format("{0:000}", _break) + "<";
                    _initbuy.Comment += "P_" + string.Format("{0:000}", Poss_xau.Count + 1) + "<";
                    _initbuy.Comment += "M_" + _mas._Mark + "<";
                    this.executeOrder(_initbuy);
                    if (this.LastResult.IsSuccessful)
                    {
                        _initsell.Symbol = _xagsymbol;
                        _initsell.Volume = _initbuy.Volume * _ratio;
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
            //var PossLabel = this.FirstPosition(Poss).Label;
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
