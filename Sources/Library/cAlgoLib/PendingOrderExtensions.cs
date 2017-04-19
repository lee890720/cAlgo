using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Lib
{
	public static class PendingOrderExtensions
	{
		public static PendingOrder Find(this PendingOrders pendingOrders, string label, Symbol symbol)
		{
			foreach(PendingOrder po in pendingOrders)
			{
				if(po.SymbolCode == symbol.Code && po.Label == label)
					return po;
			}

			return null;
		}
		
	}
}

	