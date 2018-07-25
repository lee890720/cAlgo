using cAlgo.API;
using cAlgo.API.Internals;
using System.Collections.Generic;

namespace cAlgo.Lib
{
	public class OrderParams
	{
		public TradeType? TradeType { get; set; }
		public Symbol Symbol { get; set; }
		public double? Volume { get; set; }
		public string Label { get; set; }
		public double? StopLoss { get; set; }
		public double? TakeProfit { get; set; }
		public double? Slippage { get; set; }
		public string Comment { get; set; }
		public int? Id { get; set; }

		public List<double> Parties{ get; set; }

		public OrderParams()
		{ }

		public OrderParams(TradeType? tradeType, Symbol symbol, double? volume, string label, double? stopLoss, double? takeProfit, double? slippage, string comment, int? id, List<double> parties)
		{
			TradeType = tradeType;
			Symbol = symbol;
			Volume = volume;
			Label = label;
			StopLoss = stopLoss;
			TakeProfit = takeProfit;
			Slippage = slippage;
			Comment = comment;
			Id = id;

			Parties = parties;
		}


		public OrderParams(Position p)
		{
			//Robot = robot;
			TradeType = p.TradeType;
			Symbol = (new Robot()).MarketData.GetSymbol(p.SymbolCode);
            Volume = p.VolumeInUnits;
			Label = p.Label;
			StopLoss = p.StopLoss;
			TakeProfit = p.TakeProfit;
			Slippage = null;
			Comment = p.Comment;
			Id = p.Id;
		}


		public OrderParams(OrderParams op) : this(op.TradeType,op.Symbol,op.Volume,op.Label,op.StopLoss,op.TakeProfit, op.Slippage, op.Comment, op.Id,op.Parties){}


	}
}
