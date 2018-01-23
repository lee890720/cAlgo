using cAlgo.API;

namespace cAlgo.Lib
{
	public static class TradeTypeExtensions
	{
		public static bool isBuy(this TradeType? tradeType)
		{
			if (tradeType.HasValue)
				return TradeType.Buy == tradeType;

			return false;
		}

		public static bool isBuy(this TradeType tradeType)
		{
			return TradeType.Buy == tradeType;
		}

		public static bool isSell(this TradeType? tradeType)
		{
			if (tradeType.HasValue)
				return TradeType.Sell == tradeType;

			return false;
		}

		public static bool isSell(this TradeType tradeType)
		{
			return TradeType.Sell == tradeType;
		}

		public static bool isNothing(this TradeType? tradeType)
		{
			return !(tradeType.HasValue);
		}

		public static int factor(this TradeType? tradeType)
		{
			if (tradeType.HasValue)
				return tradeType.Value.factor();
			else
				return 0;
		}

		public static int factor(this TradeType tradeType)
		{
			return (tradeType.isBuy()).factor();
		}

		public static TradeType inverseTradeType(this TradeType tradeType)
		{
			return (tradeType.isBuy() ? TradeType.Sell : TradeType.Buy);
		}

		public static TradeType? inverseTradeType(this TradeType? tradeType)
		{
			if (tradeType.HasValue)
				return tradeType.Value.inverseTradeType();
			else
				return null;
		}

	}
}
