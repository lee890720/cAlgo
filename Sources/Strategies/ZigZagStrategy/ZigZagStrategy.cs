using cAlgo.API;
using cAlgo.Indicators;
using System;

namespace cAlgo.Strategies
{
    public class ZigZagStrategy : Strategy
	{
		double zigZagPrevValue; 

		public object ZzDepth { get; set; }
		public object ZzDeviation { get; set; }
		public object ZzBackStep { get; set; }

		ZigZagIndicator zigZag;
        

		public ZigZagStrategy(Robot robot, int ZzDepth, int ZzDeviation, int ZzBackStep) : base(robot)
		{
			this.ZzDepth = ZzDepth;
			this.ZzDeviation = ZzDeviation;
			this.ZzBackStep = ZzBackStep;

			Initialize();
		}

		protected override void Initialize()
		{
			zigZag = Robot.Indicators.GetIndicator<ZigZagIndicator>(ZzDepth, ZzDeviation, ZzBackStep);

		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public override TradeType? signal1()
		{
			double lastValue = zigZag.Result.LastValue;

			TradeType? tradeType = null;

			if (!double.IsNaN(lastValue))
			{
				if (lastValue < zigZagPrevValue)
					tradeType = TradeType.Buy;
				else if (lastValue > zigZagPrevValue && zigZagPrevValue > 0.0)
					tradeType = TradeType.Sell;

				zigZagPrevValue = lastValue;
			}

			return tradeType;
		}
        public override TradeType? signal2()
        {
            throw new NotImplementedException();
        }
        public override TradeType? signal3()
        {
            throw new NotImplementedException();
        }
	}
}
