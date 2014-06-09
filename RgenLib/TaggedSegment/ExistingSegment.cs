//Formerly VB project-level imports:

using System;
using System.Xml.Linq;
using RgenLib.Attributes;
using RgenLib.Extensions;
using TextPoint = EnvDTE.TextPoint;

namespace RgenLib.TaggedSegment
{
    public partial class Manager<T> where T : TaggedCodeRenderer, new()
	{


		/// <summary>
		/// Stores information parsed by TagManager
		/// </summary>
		/// <remarks></remarks>
		public class ExistingSegment : TextRange
		{
			/// <summary>
			/// Tag generated from the found xml tag
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
            public Tag ExistingTag { get; set; }
			/// <summary>
			/// Actual attribute declared on containing property or class
			/// </summary>
			/// <value></value>
			/// <returns></returns>
			/// <remarks></remarks>
			public Manager<T> Manager {get; set;}

		    public ExistingSegment(Tag tag, TextPoint start, TextPoint endP)
			{
				StartPoint = start;
				EndPoint = endP;
                ExistingTag=tag;
			}


			public Types SegmentType {get; set;}




			public bool IsOutdated(Tag desiredTag)
			{
				switch (ExistingTag.RegenMode)
				{
					case RegenModes.Always:
						return true;
					default:
                        return !(ExistingTag.Equals(desiredTag));
						

				}

			}

		
		    #region Find ExistingSegment

		

            //public void Parse()
            //{
                

            //    if (!IsValid)
            //    {
            //        return;
            //    }
            //    var xml = ExtractXml();
            //    var xdoc = XDocument.Parse(xml);
            //    var xr = xdoc.Root;

            //    try
            //    {
            //        var dateAttribute = xr != null ? xr.Attribute("Date"):null;
            //        GenerateDate = dateAttribute != null ? Convert.ToDateTime(xr.Attribute("Date").Value) : (DateTime?)null;
            //        ExistingTag = Manager.ParseXml()
            //        ExistingTag.CopyPropertyFromTag(xr);
            //    }
            //    catch (Exception)
            //    {
            //        Debug.DebugHere();
            //    }



            //}





#endregion

		}

    }
}