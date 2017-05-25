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
        public double upper, lower, uppermax, uppermin, lowermax, lowermin, midmax, midmin;
        public int bars;
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
            bollhalf = Robot.Indicators.BollingerBands(Source, Periods / 2, Deviations, MAType);
            bars = Robot.MarketSeries.Bars();
            upper = boll.Top[bars - 2];
            lower = boll.Bottom[bars - 2];
            midmin = boll.Main[bars - 2] < bollhalf.Main[bars - 2] ? boll.Main[bars - 2] : bollhalf.Main[bars - 2];
            midmax = boll.Main[bars - 2] > bollhalf.Main[bars - 2] ? boll.Main[bars - 2] : bollhalf.Main[bars - 2];
            uppermin = boll.Top[bars - 2] < bollhalf.Top[bars - 2] ? boll.Top[bars - 2] : bollhalf.Top[bars - 2];
            uppermax = boll.Top[bars - 2] > bollhalf.Top[bars - 2] ? boll.Top[bars - 2] : bollhalf.Top[bars - 2];
            lowermin = boll.Bottom[bars - 2] < bollhalf.Bottom[bars - 2] ? boll.Bottom[bars - 2] : bollhalf.Bottom[bars - 2];
            lowermax = boll.Bottom[bars - 2] > bollhalf.Bottom[bars - 2] ? boll.Bottom[bars - 2] : bollhalf.Bottom[bars - 2];
        }
        public override TradeType? signal1()
        {
            //throw new NotImplementedException();
            Initialize();
            TradeType? tradeType = null;
            if (Robot.Symbol.Mid() > uppermax)
                tradeType = TradeType.Sell;
            if (Robot.Symbol.Mid() < lowermin)
                tradeType = TradeType.Buy;
            return tradeType;
        }
        public override TradeType? signal2()
        {
            //throw new NotImplementedException();
            Initialize();
            TradeType? tradeType = null;
            if (Robot.MarketSeries.isBullCandle(1) == true && Robot.MarketSeries.isCandleOver(1, midmax))
                tradeType = TradeType.Buy;
            if (Robot.MarketSeries.isBearCandle(1) == true && Robot.MarketSeries.isCandleOver(1, midmin))
                tradeType = TradeType.Sell;
            return tradeType;
        }
        public override TradeType? signal3()
        {
            //throw new NotImplementedException();
            Initialize();
            TradeType? tradeType = null;
            if (Robot.MarketSeries.isBearCandle(1) == true && Robot.MarketSeries.isCandleOver(1, midmax))
                tradeType = TradeType.Buy;
            if (Robot.MarketSeries.isBullCandle(1) == true && Robot.MarketSeries.isCandleOver(1, midmin))
                tradeType = TradeType.Sell;
            return tradeType;
        }
    }
}
