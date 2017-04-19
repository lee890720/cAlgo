using System;
using System.Text;
using System.Linq;

using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;


namespace cAlgo.Lib
{
	public static class PositionExtensions
	{
		public static bool isAlive(this Position position, Positions positions)
		{
			var request = from p in positions select p.Id==position.Id ;

			return request!=null;

		}

		public static bool isBuy(this Position position)
		{

			return position !=null && TradeType.Buy == position.TradeType;
		}

		public static bool isSell(this Position position)
		{
			return position !=null && TradeType.Sell == position.TradeType;
		}

		public static bool hasStop(this Position position)
		{
			return position.StopLoss.HasValue;
		}

		public static bool hasTakeProfit(this Position position)
		{
			return position.TakeProfit.HasValue;
		}

		public static int factor(this Position position)
		{
			TradeType tradeType = position.TradeType;

			return tradeType.factor();
		}

		public static double? pipsToStopLoss(this Position position, Symbol symbol, int SLPips)
		{
			return position.EntryPrice - position.factor() * symbol.PipSize * SLPips;
		}

		public static double? pipsToTakeProfit(this Position position, Symbol symbol, int TPPips)
		{

			return position.EntryPrice + position.factor() * symbol.PipSize * TPPips;

		}

		public static double? valueToPips(this Position position, Symbol symbol, double? number)
		{
			if (number.HasValue)
				return (number - position.EntryPrice) / (position.factor() * symbol.PipSize);
			else
				return null;

		}

		public static double? stopLossToPips(this Position position, Symbol symbol)
		{
			return valueToPips(position, symbol, position.StopLoss);
		}

		public static double? takeProfitToPips(this Position position, Symbol symbol)
		{
			return valueToPips(position, symbol, position.TakeProfit);
		}

		public static TradeType inverseTradeType(this Position position)
		{
			return (position.isBuy() ? TradeType.Sell : TradeType.Buy);
		}

		public static double? potentialProfit(this Position position)
		{
			if (position.TakeProfit.HasValue)
				return (position.isBuy() ? position.TakeProfit.Value - position.EntryPrice : position.EntryPrice - position.TakeProfit.Value) * position.Volume;
			else
				return null;

		}

		public static double? potentialLoss(this Position position)
		{
			if (position.StopLoss.HasValue)
				return (position.isBuy() ? position.EntryPrice - position.StopLoss.Value : position.StopLoss.Value - position.EntryPrice) * position.Volume;
			else
				return null;

		}

		public static double? percentProfit(this Position position)
		{
			double? potentialProfit = position.potentialProfit();

			if (potentialProfit.HasValue && potentialProfit.Value !=0)
				return position.GrossProfit / position.potentialProfit();
			else
				return null;
		}

		public static double? percentLoss(this Position position)
		{
			double? potentialLoss = position.potentialLoss();

			if (potentialLoss.HasValue && potentialLoss.Value != 0)
				return position.GrossProfit / position.potentialLoss();
			else
				return null;
		}

		public static string infos(this Position position, Symbol symbol)
		{
			StringBuilder logMessage = new StringBuilder();

			logMessage.AppendFormat("Symbol: {0}, {1}, Gain: {0} Pips", symbol.Code, position.TradeType, position.valueToPips(symbol,position.GrossProfit));

			return logMessage.ToString();
		}

		public static string log(this Position position, Robot robot, bool withLabels=false)
		{
			StringBuilder logMessage = new StringBuilder();
			string logFormat;

			if (withLabels)
				logFormat = "Symbol: {0}, Id: {1}, Year: {2}, Month: {3}, Day: {4}, DayOfWeek: {5}, EntryTime: {6}, Volume: {7}, TradeType: {8}, EntryPrice: {9}, StopLoss: {10}, TakeProfit: {11}, GrossProfit: {12}, NetProfit: {13}, Equity: {14}, Balance: {15}";
			else
				logFormat = "{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}";



			logMessage.AppendFormat(logFormat,	robot.Symbol.Code,
												position.Id,
												robot.Server.Time.Year,
												robot.Server.Time.Month,
												robot.Server.Time.Day,
												robot.Server.Time.DayOfWeek,
												position.EntryTime,
												position.Volume,
												position.TradeType,
												position.EntryPrice,
												position.StopLoss,
												position.TakeProfit,
												position.GrossProfit,
												position.NetProfit,
												robot.Account.Equity,
												robot.Account.Balance);
			if (position.isAlive(robot.Positions))
			{
				string format;
				if (withLabels)
					format = ",ExitTime: {0}, ClosePrice {1}";
				else
					format = ",{0}, {1}";

					logMessage.AppendFormat(format,robot.Server.Time,robot.Symbol.Ask);
			}

			return logMessage.ToString();
		}

		public static double? trailStop(this  Position position, Robot robot, double trailStart, double trailStop, bool isModifyPosition=true)
		{
			double? newStopLoss = position.StopLoss;	
			
			if (position.Pips > trailStart)
			{
				double actualPrice = position.isBuy() ? robot.Symbol.Bid : robot.Symbol.Ask;
				int factor = position.factor();

				if((actualPrice - newStopLoss) * factor > trailStop * robot.Symbol.PipSize)
				{
					newStopLoss += factor * trailStop * robot.Symbol.PipSize;

					if (isModifyPosition && newStopLoss != position.StopLoss)
						robot.ModifyPosition(position, newStopLoss, position.TakeProfit.Value);
				}
			}

			return newStopLoss;
		}
	}
}
