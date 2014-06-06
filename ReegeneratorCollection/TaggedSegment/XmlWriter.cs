using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ReegeneratorCollection.TaggedSegment
{

    /// <summary>
    /// A custom XmlTextWriter that generate xml embedded in comment or region name
    /// </summary>
    /// <remarks>
    /// Setting WriteStartElement("", localname, "") only removes namespace for element, but not the xmlns attribute
    /// Overriding WriteAttributes doesn't work, never gets called, so XML has to be stripped off of namespace beforehand
    /// 
    /// newline before content cannot be done by overriding WriteEndElement(this writes blah end tag)
    /// What we need is a private function in XmlTextWriter, called WriteEndStartTag
    /// </remarks>
	public class XmlWriter : XmlTextWriter
	{
		public const string CodeCommentPrefix = "//";
		public Types SegmentType {get; set;}

		//Property IsRegion As Boolean
		public XmlWriter(StringWriter writer) : base(writer)
		{
			QuoteChar = '\'';
		}

		public override void WriteStartElement(string prefix, string localname, string ns)
		{
			//insert inline comment character before the start tag
			if (SegmentType == Types.Statements)
			{
				WriteString(CodeCommentPrefix);
			}
			base.WriteStartElement(prefix, localname, ns);
		}

		public override void WriteFullEndElement()
		{
			//insert inline comment character before the end tag
			if (SegmentType == Types.Statements)
			{
				WriteString(CodeCommentPrefix);
			}
			base.WriteFullEndElement();
			//add new line
			WriteString(Environment.NewLine);
		}
		public static string ToCommentedString(XElement x)
		{
			return InternalToString(x, Types.Statements);
		}

		/// <summary>
		/// Write xml based on segment type
		/// </summary>
		/// <param name="x"></param>
		/// <param name="segmentType"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		private static string InternalToString(XElement x, Types segmentType)
		{
			StringWriter writer = new StringWriter();
			XmlWriter cw = new XmlWriter(writer) {SegmentType = segmentType};
			//Strip Namespace if it's an Xelement
			x = StripNS(x);
			//write
			x.WriteTo(cw);
			return writer.GetStringBuilder().ToString();
		}
		public static string EscapeQuote(string s)
		{
			const string Quote = "\"";
			const string DoubleQuote = Quote + Quote;
			return s.Replace(Quote, DoubleQuote);
		}

		public static string ToRegionNameString(XElement x)
		{

			var xml = InternalToString(x, Types.Region);
			//Escape quote to double quote, so it will be valid as region name
			var res = EscapeQuote(xml);
			return res;
		}

		public static string ToStringNoNS(XElement xmlDocument)
		{
			return StripNS(xmlDocument).ToString();
		}
		public static XElement StripNS(XElement root)
		{
			var res = new XElement(root);
			res.ReplaceAttributes(root.Attributes().Where((attr) => (!attr.IsNamespaceDeclaration)));
			return res;
		}



	}

}