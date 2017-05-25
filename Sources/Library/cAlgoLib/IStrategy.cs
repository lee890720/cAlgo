using cAlgo.API;

namespace cAlgo.Strategies
{
	public interface IStrategy
	{
		TradeType? signal1();
        TradeType? signal2();
        TradeType? signal3();
	}
}
