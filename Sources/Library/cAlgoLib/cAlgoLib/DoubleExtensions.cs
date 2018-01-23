using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;

namespace cAlgo.Lib
{
	/// <summary>
	/// Extensions methods of double
	/// </summary>	public static class DoubleExtensions
	public static class DoubleExtensions
	{
		public static double round(this double value, Robot robot)
		{
			return (double)Math.Round((decimal)value, robot.Symbol.Digits, MidpointRounding.AwayFromZero);
		}

	}
}
