using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cAlgo.Lib
{
	/// <summary>
	/// Extensions methods of boolean
    /// 布尔类型扩展方法
	/// </summary>
	public static class BooleanExtensions
	{
		/// <summary>
		/// Converts true to 1 and false to 0
		/// </summary>
		/// <param name="b">Boolean to test</param>
		/// <returns>returns 1 if b = true 0 otherwise</returns>
		public static int toInt(this Boolean b)
		{
			return b ? 1 : 0;
		}

		/// <summary>
		/// Convert true to 1 and false to -1
		/// </summary>
		/// <param name="b">Boolean to test</param>
		/// <returns>returns 1 if b=true, -1 otherwise</returns>
		public static int factor(this Boolean b)
		{
			return b ? 1 : -1;
		}
	}
}
