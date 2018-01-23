using cAlgo.API.Internals;

namespace cAlgo.Lib
{

	public static class SymbolExtensions
	{
		public static double Mid(this Symbol symbol)
		{
			double midPrice = (symbol.Ask + symbol.Bid) / 2; 

			return midPrice;
		}

		public static double marginRequired(this Symbol symbol, double lots, int leverage)
		{
			double margin;
			double crossPrice = symbol.Ask;

			//USD / XXX:  
			margin = lots / leverage;

			//XXX / USD:
			margin = crossPrice * lots / leverage;

			//XXX / YYY:
			//a). (EUR/GBP, AUD/NZD ...)
			//margin = crossPrice  * currentPrice(XXX/USD) * lots / leverage;
			//b). (CAD/CHF, CHF/JPY ....)
			// margin = crossPrice / currentPrice(USD/XXX)  * lots / leverage;

			return margin;
		}
	}
}

