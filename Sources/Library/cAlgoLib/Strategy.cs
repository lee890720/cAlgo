using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Strategies
{
	public abstract class Strategy : IStrategy
	{
		protected Strategy(Robot robot)
		{
			Robot = robot;
		}
		public Robot Robot
		{
			get;
			private set;
		}
		public abstract TradeType? signal1();
        public abstract TradeType? signal2();
        public abstract TradeType? signal3();
		protected virtual void Initialize() {}
	}
}
