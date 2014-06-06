
using EnvDTE;

namespace ReegeneratorCollection.TaggedSegment
{

    /// <summary>
    /// Creat own class because the built in TextRange is not usable (used for regular expression search result)
    /// </summary>
    /// <remarks></remarks>
	public class TextRange
	{
		public TextPoint StartPoint {get; set;}
		public TextPoint EndPoint {get; set;}

		/// <summary>
		/// Valid if both StartPoint and EndPoint are not null
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public bool IsValid
		{
			get
			{
				return StartPoint != null && EndPoint != null;
			}
		}
		public TextRange()
		{

		}
		public TextRange(TextPoint sp, TextPoint ep)
		{
			StartPoint = sp;
			EndPoint = ep;
		}
		public void ReplaceText(string text)
		{
			StartPoint.CreateEditPoint().ReplaceText(EndPoint, text,
                (int)( vsEPReplaceTextOptions.vsEPReplaceTextAutoformat | vsEPReplaceTextOptions.vsEPReplaceTextNormalizeNewlines));
		}

		public void Delete()
		{
			if (IsValid)
			{
				var ep = StartPoint.CreateEditPoint();
				ep.Delete(EndPoint);

			}
		}

		public string GetText()
		{
			return StartPoint.CreateEditPoint().GetText(EndPoint);
		}
	}
}