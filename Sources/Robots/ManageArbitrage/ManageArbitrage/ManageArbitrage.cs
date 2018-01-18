using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class ClosePosition : Robot
    {
        [Parameter("INIT_Volume", DefaultValue = 1, MinValue = 1)]
        public double Init_Volume { get; set; }

        [Parameter(DefaultValue = "EURCHF")]
        public string FirstSymbol { get; set; }

        [Parameter(DefaultValue = "EURCHF")]
        public string SecondSymbol { get; set; }

        [Parameter("label", DefaultValue = "Above-EURCHF-Hour")]
        public string label { get; set; }

        [Parameter("comment", DefaultValue = "000001-001-001-2000-01-01-01")]
        public string comment { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsOpen { get; set; }

        [Parameter(DefaultValue = false)]
        public bool IsClose { get; set; }

        private Symbol _symbol, _firstsymbol, _secondsymbol;

        protected override void OnStart()
        {
            if (FirstSymbol == SecondSymbol)
                _symbol = MarketData.GetSymbol(FirstSymbol);
            else
            {
                _firstsymbol = MarketData.GetSymbol(FirstSymbol);
                _secondsymbol = MarketData.GetSymbol(SecondSymbol);
            }
        }

        protected override void OnTick()
        {
            if (IsOpen)
            {
                if (FirstSymbol == SecondSymbol)
                {
                    if (label.Substring(0, 5) == "Above")
                        ExecuteMarketOrder(TradeType.Sell, _symbol, _symbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                    else if (label.Substring(0, 5) == "Below")
                        ExecuteMarketOrder(TradeType.Buy, _symbol, _symbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                }
                else
                {
                    if (label.Substring(0, 5) == "Above")
                    {
                        ExecuteMarketOrder(TradeType.Sell, _firstsymbol, _firstsymbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                        ExecuteMarketOrder(TradeType.Buy, _secondsymbol, _secondsymbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                    }
                    else if (label.Substring(0, 5) == "Below")
                    {
                        ExecuteMarketOrder(TradeType.Buy, _firstsymbol, _firstsymbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                        ExecuteMarketOrder(TradeType.Sell, _secondsymbol, _secondsymbol.NormalizeVolume(Init_Volume, RoundingMode.ToNearest), label, null, null, null, comment);
                    }
                }
            }
            if (IsClose)
                foreach (var pos in Positions)
                {
                    if (pos.Label == label)
                        ClosePosition(pos);
                }
            Stop();
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
