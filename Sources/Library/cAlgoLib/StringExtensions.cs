using System;
using cAlgo.API;

namespace cAlgo.Lib
{
	public static class StringExtensions
	{
		public static Colors? parseColor(this string colorString)
		{
			Colors color;

			if (!Enum.TryParse<Colors>(colorString, out color))
				return color;
			else
				return null;
		}
	}
}
