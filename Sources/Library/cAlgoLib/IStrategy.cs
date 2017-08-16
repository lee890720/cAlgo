using cAlgo.API;

namespace cAlgo.Strategies
{
	public interface IStrategy
	{
		TradeType? signal();
        string singnalS();
	}
}
