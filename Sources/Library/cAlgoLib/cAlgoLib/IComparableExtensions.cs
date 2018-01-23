using System;

namespace cAlgo.Lib
{
	public static class IComparableExtensions
	{
		public static bool between<T>(this T value, T from, T to) where T : IComparable<T>
		{
			return value.CompareTo(from) >= 0 && value.CompareTo(to) <= 0;
		}
	}
}
