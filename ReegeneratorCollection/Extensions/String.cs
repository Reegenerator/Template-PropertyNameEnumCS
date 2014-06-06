//Formerly VB project-level imports:

using System.Text;

namespace ReegeneratorCollection.Extensions
{
	public static class String
	{

#region StringBuilder

//INSTANT C# NOTE: These were formerly VB static local variables:
	    private const char AppendIndentFormat_tab = '\t';
	    private const char AppendIndent_tab = '\t';

	    public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object[] values)
		{
			return sb.AppendFormat(format, values).AppendLine();
		}

		public static StringBuilder AppendIndentFormat(this StringBuilder sb, int tabCount, string format, params object[] values)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static tab As Char = vbTab.First
			return sb.AppendFormat("{0}{1}", new string(AppendIndentFormat_tab, tabCount), string.Format(format, values));
		}

		public static StringBuilder AppendIndent(this StringBuilder sb, int tabCount, string text)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static tab As Char = vbTab.First
			return sb.AppendFormat("{0}{1}", new string(AppendIndent_tab, tabCount), text);
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
			return "\"" + s + "\"";
		}
	}

}