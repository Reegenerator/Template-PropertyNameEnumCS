
using System;
using RgenLib.TaggedSegment;

namespace RgenLib
{
    /// <summary>
    /// Mark which properties of GeneratorOptionAttribute are to be written in the Tag by <see cref="Manager{T}.Writer"/> 
    /// </summary>
	public class XmlAttributeAttribute : Attribute
	{
        /// <summary>
        /// Specifies an alternate (usually shorter) name of property name to be written as xml attribute
        /// </summary>
		public string Name {get; set;}
		public XmlAttributeAttribute(string attrName)
		{
			Name = attrName;
		}
	}

}