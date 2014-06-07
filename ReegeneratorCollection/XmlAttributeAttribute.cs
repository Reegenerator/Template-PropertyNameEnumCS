
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RgenLib.TaggedSegment;

namespace RgenLib
{
    /// <summary>
    /// Mark which properties of GeneratorOptionAttribute are to be written in the Tag by <see cref="Manager{T}.Writer"/> 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
	public class XmlAttributeAttribute : Attribute
	{

        /// <summary>
        /// stores dictionary of attribute name => property info
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _xmlNameToProperty_ByType;
        /// <summary>
        /// stores dictionary of property name => xml attribute name
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, string>> _propertyToXml_ByType;

        static XmlAttributeAttribute()
        {
            _xmlNameToProperty_ByType = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
            _propertyToXml_ByType = new Dictionary<Type, Dictionary<string, string>>();
        }

        public static Dictionary<string, PropertyInfo> GetXmlProperties(Type type)
        {
            Dictionary<string, PropertyInfo> xmlNameToPropertyDict;
            //if not found, initialize dictionary
            if (!_xmlNameToProperty_ByType.TryGetValue(type, out xmlNameToPropertyDict))
            {
                var typeCache = TypeResolver.ByType(type);
                xmlNameToPropertyDict = new Dictionary<string, PropertyInfo>();
                var members = typeCache.GetProperties().ToArray();
                //add all properties with custom attribute
                foreach (var m in members) {
                    var xmlAttr = m.GetCustomAttribute<XmlAttributeAttribute>();
                    if (xmlAttr == null) { continue; }
                    xmlNameToPropertyDict.Add(xmlAttr.Name, (PropertyInfo)typeCache[m.Name]);
                }
                _xmlNameToProperty_ByType.Add(type, xmlNameToPropertyDict);
            }
            return xmlNameToPropertyDict;
        }
        public static Dictionary<string, string> GetPropertyToXmlAttributeTranslation(Type type) {
            Dictionary<string, string> propertyToXmlDict;
            //if not found, initialize dictionary
            if (!_propertyToXml_ByType.TryGetValue(type, out propertyToXmlDict)) {
                var typeCache = TypeResolver.ByType(type);
                propertyToXmlDict = new Dictionary<string, string>();
                var members = typeCache.GetProperties().ToArray();
                //add all properties 
                foreach (var m in members) {
                    var xmlAttr = m.GetCustomAttribute<XmlAttributeAttribute>();
                    if (xmlAttr == null)
                    {
                        //if there's no xmlAttribute, add the pair with same key and value
                        //so it will be safe to always get the property name
                        propertyToXmlDict.Add(m.Name, m.Name); ;
                    }
                    else
                    {
                        propertyToXmlDict.Add(m.Name, xmlAttr.Name);
                        
                    }
                }
                _propertyToXml_ByType.Add(type, propertyToXmlDict);
            }
            return propertyToXmlDict;
        }
      
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