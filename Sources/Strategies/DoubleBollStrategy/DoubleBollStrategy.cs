using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.Strategies;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Lib;


namespace cAlgo.Strategies
{
    public class DoubleBollStrategy : Strategy
    {
        public DataSeries Source { get; set; }
        public int Periods { get; set; }
        public double Deviations { get; set; }
        public MovingAverageType MAType { get; set; }
        public string Condition { get; set; }
        BollingerBands boll;
        BollingerBands bollhalf;
        public double UpperMax, UpperMin, LowerMax, LowerMin;
        public DoubleBollStrategy(Robot robot, DataSeries source, int periods, double deviations, MovingAverageType ma, string condition = null)
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
            bollhalf = Robot.Indicators.BollingerBands(Source, Periods/2, Deviations, MAType);

        }
        public override TradeType? signal()
        {
            //throw new NotImplementedException();
            var upper = boll.Top.Last(1);
            var lower = boll.Bottom.Last(1);
            var midmin = boll.Main.Last(1) < bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var midmax = boll.Main.Last(1) > bollhalf.Main.Last(1) ? boll.Main.Last(1) : bollhalf.Main.Last(1);
            var uppermin = boll.Top.Last(1) < bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var uppermax = boll.Top.Last(1) > bollhalf.Top.Last(1) ? boll.Top.Last(1) : bollhalf.Top.Last(1);
            var lowermin = boll.Bottom.Last(1) < bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            var lowermax = boll.Bottom.Last(1) > bollhalf.Bottom.Last(1) ? boll.Bottom.Last(1) : bollhalf.Bottom.Last(1);
            UpperMax = uppermax;
            UpperMin = uppermin;
            LowerMax = lowermax;
            lowermin = lowermin;
            TradeType? tradeType = null;
            if (Robot.Symbol.Mid() > uppermax)
                tradeType = TradeType.Sell;
            if (Robot.Symbol.Mid() < lowermin)
                tradeType = TradeType.Buy;
            return tradeType;
        }
    }
}
