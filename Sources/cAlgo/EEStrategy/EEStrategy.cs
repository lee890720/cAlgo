using cAlgo.API;
using System;

namespace cAlgo.Strategies
{
    public class EEStrategy : Strategy
    {
        public int Period { get; set; }
        public string Symbol2 { get; set; }
        public int Distance { get; set; }

        private _EE ee;

        public EEStrategy(Robot robot, int period, string symbol2, int distance)
            : base(robot)
        {
            this.Period = period;
            this.Symbol2 = symbol2;
            this.Distance = distance;
            Initialize();
        }
        protected override void Initialize()
        {
            ee = Robot.Indicators.GetIndicator<_EE>(Period, Symbol2);
        }
        public override TradeType? signal()
        {
            var result = ee.Result.LastValue;
            var average = ee.Average.LastValue;
            TradeType? tt = null;
            if (Robot.Positions.Count == 0)
            {
                if (result > average + Distance)
                    tt = TradeType.Buy;
                if (result < average - Distance)
                    tt = TradeType.Sell;
            }
            else
            {
                var now = DateTime.UtcNow;
                if (DateTime.Compare(Robot.Positions[Robot.Positions.Count - 1].EntryTime.AddHours(1), now) < 0)
                {
                    if (result > average + Distance)
                        tt = TradeType.Buy;
                    if (result < average - Distance)
                        tt = TradeType.Sell;
                }
                else
                {
                    var eb = Math.Abs(Robot.Positions[Robot.Positions.Count - 1].EntryPrice - Robot.Positions[Robot.Positions.Count - 2].EntryPrice) / Robot.Symbol.PipSize;
                    if (result > average + eb + 10)
                        tt = TradeType.Buy;
                    if (result < average - eb - 10)
                        tt = TradeType.Sell;
                }
            }
            return tt;
        }
        public override string singnalS()
        {
            var result = ee.Result.LastValue;
            var average = ee.Average.LastValue;
            string ss = null;
            if (result >= average)
                ss = "closesell";
            if (result <= average)
                ss = "closebuy";
            return ss;
        }
    }
}