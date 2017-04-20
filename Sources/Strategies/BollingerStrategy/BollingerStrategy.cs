using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Lib;

namespace cAlgo.Strategies
{
    public class BollingerStrategy : Strategy
    {
        public DataSeries Source { get; set; }
        public int Periods { get; set; }
        public double Deviations { get; set; }
        public MovingAverageType MAType { get; set; }
        public string Condition { get; set; }

        public BollingerBands boll;
        public BollingerStrategy(Robot robot, DataSeries source, int periods, double deviations, MovingAverageType ma,string condition=null)
            : base(robot)
        {
            this.Source = source;
            this.Periods = periods;
            this.Deviations = deviations;
            this.MAType = ma;
            this.Condition = condition;
            Initialize();
        }
        protected override void Initialize()
        {
            boll = Robot.Indicators.BollingerBands(Source, Periods, Deviations, MAType);
        }
        public override TradeType? signal()
        {
            //throw new NotImplementedException();
            var upper = boll.Top.Last(1);
            var lower = boll.Bottom.Last(1);

            TradeType? tradeType = null;
            if (Condition == null || Condition == "Default")
            {
                if (Robot.Symbol.Mid() > upper)
                    tradeType = TradeType.Sell;
                if (Robot.Symbol.Mid() < lower)
                    tradeType = TradeType.Buy;
            }
            else if(Condition=="RisingAndFalling")
            {
                if (Robot.Symbol.Mid() > upper && !boll.Bottom.IsFalling())
                    tradeType = TradeType.Sell;
                if (Robot.Symbol.Mid() < lower && !boll.Top.IsRising())
                    tradeType = TradeType.Buy;
            }
            return tradeType;
        }
    }
}
