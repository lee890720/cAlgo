using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace cAlgo.Lib
{
	public static class MarketSeriesExtensions
	{
		public static int Bars(this MarketSeries marketSeries)
		{
			return marketSeries.Open.Count;
		}

		public static bool? isBullCandle(this MarketSeries marketSeries, int index)
		{
			int count = marketSeries.Bars();

			if (index >= 0 && index < count)
			{
				double open = marketSeries.Open[index];
				double close = marketSeries.Close[index];
				double median = marketSeries.Median[index];
				double variation;

				if (double.IsNaN(close))
					variation = median-open;
				else
					variation = close - open;

				if (variation > 0)
					return true;
				else
					if (variation < 0)
						return false;
					else
						return null;
			}
			else
				throw new ArgumentException(string.Format("Valeur de l'indice {0} en dehors des valeurs permises", index));
		}

		public static bool? isBearCandle(this MarketSeries marketSeries, int index)
		{
			bool? returnValue = marketSeries.isBullCandle(index);
			
			return (returnValue!=null) ? !returnValue : null;

		}

		public static bool isCandleAbove(this MarketSeries marketSeries, int index, double limit)
		{
			int count = marketSeries.Bars();

			if (index >= 0 && index < count)
				return marketSeries.Low[count - 1 - index] >= limit;
			else 
				throw new ArgumentException(string.Format("Valeur de l'indice {0} en dehors des valeurs permises", index));


		}

		public static bool isCandleBelow(this MarketSeries marketSeries, int index, double limit)
		{
			int count = marketSeries.Bars();

			if (index >= 0 && index < count)
				return marketSeries.High[count - 1 - index] <= limit;
			else
				throw new ArgumentException(string.Format("Valeur de l'indice {0} en dehors des valeurs permises", index));


		}
		
		public static bool isCandleOver(this MarketSeries marketSeries, int index, double frontier)
		{
			int count = marketSeries.Bars();

			if (index >= 0 && index < count)
			{
				return frontier.between(marketSeries.Low[count - 1 - index], marketSeries.High[count - 1 - index]);

			}
			else
				throw new ArgumentException(string.Format("Valeur de l'indice {0} en dehors des valeurs permises", index));

		}

		public static bool isCandleBetween(this MarketSeries marketSeries, int index, double dn, double up)
		{
			return isCandleAbove(marketSeries, index, dn) && isCandleBelow(marketSeries, index, up);

		}

		public static double volatility(this MarketSeries marketSeries, int period)
		{
			double maximum = marketSeries.High.Maximum(period);
			double minimum = marketSeries.Low.Minimum(period);

			return (maximum - minimum);
		}

		public static double volatilityInPips(this MarketSeries marketSeries, int period, Symbol symbol)
		{
			return marketSeries.volatility(period) / symbol.PipSize;
		}

		public static int barsAgo(this MarketSeries marketSeries, Position position)
		{
			for(var i = marketSeries.OpenTime.Count - 1; i >= 0; i--)
			{
				if(position.EntryTime > marketSeries.OpenTime[i])
					return marketSeries.OpenTime.Count - 1 - i;
			}
			return -1;
		}

		public static int GetIndexByDate(this MarketSeries series, DateTime time)
		{
			for(int i = series.Open.Count - 1; i > 0; i--)
			{
				if(time >= series.OpenTime[i])
					return i;
			}
			return -1;
		}
	}
}
