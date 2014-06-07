
using System.Text;

namespace RgenLib.Extensions
{
	public static class String
	{

#region StringBuilder

	    private const char Tab = '\t';
	    private const char DoubleQuote = '"';
	    public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] values)
		{
			return sb.AppendFormat(format, values).AppendLine();
		}

		public static StringBuilder AppendIndentFormat(this StringBuilder sb, int tabCount, string format, params object[] values)
		{

            return sb.AppendFormat("{0}{1}", new string(Tab, tabCount), string.Format(format, values));
		}

		public static StringBuilder AppendIndent(this StringBuilder sb, int tabCount, string text)
		{

            return sb.AppendFormat("{0}{1}", new string(Tab, tabCount), text);
		}
		/// <summary>
		/// Join two strings , only if both are not empty strings
		/// </summary>
		/// <param name="leftSide"></param>
		/// <param name="conjunction"></param>
		/// <param name="rightSide"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string Conjoin(this string leftSide, string conjunction, string rightSide)
		{
			return leftSide + ((!string.IsNullOrEmpty(leftSide) && !string.IsNullOrEmpty(rightSide)) ? conjunction : "") + rightSide;
		}
#endregion
		public static string Quote(this string s)
		{
            return DoubleQuote + s + DoubleQuote;
		}
	}

}