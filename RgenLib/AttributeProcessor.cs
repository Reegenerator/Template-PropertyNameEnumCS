using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using RgenLib.Extensions;
using RgenLib.Attributes;
using RgenLib.TaggedSegment;

namespace RgenLib {
   public class AttributeProcessor {
            private const string NameSuffix = "_GenAttribute";

        

        public CodeProperty2 ParentProperty { get; set; }
        public CodeFunction2 ParentFunction { get; set; }
        public CodeElement2 ParentElement { get; set; }

        public bool IsInsertionPoint { get; set; }
        public TypeCache TypeCache { get; set; }

        public TagTypes Type { get; set; }

        /// <summary>
        /// Use to differentiate segments when we are calling FindSegments
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>
        /// Without a class differentiator, when searching for a class level segment, it will match all segments within the class
        /// and will cause unintended deletion when the segment needs to be updated
        /// </remarks>
        [XmlAttribute("Class")]
        public string SegmentClass { get; set; }

  

        //INSTANT C# NOTE: These were formerly VB static local variables:
        private static readonly Assembly GetTypeFromTagName_assm = typeof(GeneratorOptionAttribute).Assembly;

        public virtual XElement TagPrototype {
            get {
                return null;
            }
        }
        #region Constructors

        public AttributeProcessor() {
            //?+Do not create public constructors with parameters
            //?It will be hard to determine, using CodeAttribute (In CodeAttributeArgument Name is empty, only Value is shown), which constructor is being used (the info is design time)
            //?Use named parameter instead when trying to set the properties (Name and Value are shown)
            Init();
        }

        internal AttributeProcessor(CodeFunction2 f) {
            ParentFunction = f;
            Init(f.AsCodeElement());
        }
        internal AttributeProcessor(CodeProperty2 p) {
            ParentProperty = p;
            Init(p.AsCodeElement());
        }

        internal AttributeProcessor(CodeClass cc) {
            Init(cc.AsCodeElement());
            CopyPropertyFromAttributeArguments(cc.AsCodeElement().GetCustomAttribute(GetType()).GetCodeAttributeArguments());
        }

        private void Init(CodeElement2 ele) {
            Init();
            ParentElement = ele;
            var attrs = ele.GetCustomAttribute(GetType()).GetCodeAttributeArguments();
            CopyPropertyFromAttributeArguments(attrs);
        }
        public virtual void Init() {
            //Do once per application run. Can't make shared because we need to get actual derived class
            var type = GetType();
            TypeCache = TypeResolver.ByType(type);
            //Store members with XmlAttributeAttribute into a dictionary, to be used when writing the xml
            if (!(XmlPropertiesByType.ContainsKey(type))) {
                var xmlmembers = new Dictionary<string, PropertyInfo>();
                var members = TypeCache.GetMembers().ToArray();
                foreach (var m in members) {
                    var xmlName = m.GetCustomAttribute<XmlAttributeAttribute>();
                    if (xmlName == null) {
                        continue;
                    }
                    xmlmembers.Add(xmlName.Name, (PropertyInfo)TypeCache[m.Name]);
                }
                XmlPropertiesByType.Add(type, xmlmembers);
            }


        }


        internal AttributeProcessor(XElement xele) {
            Init();
            CopyPropertyFromTag(xele);

        }

        #endregion

        public Dictionary<string, PropertyInfo> GetXmlProperties()
		{
			return XmlPropertiesByType[GetType()];
		}
        public new virtual GeneratorOptionAttribute MemberwiseClone() {
            return (GeneratorOptionAttribute)base.MemberwiseClone();
        }
        public void CopyPropertyFromTag(XElement xele) {
            var xmlProps = GetXmlProperties();
            foreach (var attr in xele.Attributes()) {
                var name = attr.Name.LocalName;
                if (name == "Renderer") {
                    continue;
                }
                PropertyInfo propInfo = null;
                //If a property has an XmlProperty attribute, it will be rendered using that name, instead of the property name
                //Check XmlProperties first
                if (!(xmlProps.TryGetValue(name, out propInfo))) {
                    //if not found, get by property name
                    propInfo = TypeCache.TryGetMember(attr.Name.LocalName) as PropertyInfo;
                }

                if (propInfo != null) {
                    SetPropertyFromAttributeArgumentString(propInfo, attr.Value);
                }
            }
        }

        private void CopyPropertyFromAttributeArguments(IEnumerable<CodeAttributeArgument> args) {
            foreach (var arg in args) {
                PropertyInfo propInfo = null;

                //?if enum, strip qualifier in value
                propInfo = (PropertyInfo)TypeCache[arg.Name];

                SetPropertyFromAttributeArgumentString(propInfo, arg.Value);
            }
        }
   

        [XmlAttribute("Ver")]
        public virtual Version Version { get; set; }

        /// <summary>
        /// Mode to be written in xml tag
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks>Can be overridden by GenInfo.RegenMode</remarks>
        [XmlAttribute("Mode")]
        public virtual RegenModes RegenMode { get; set; }


        private string _TagName;

        public virtual string TagName {
            get {
                if (string.IsNullOrEmpty(_TagName)) {
                    var nm = GetType().Name;
                    _TagName = nm.Substring(0, nm.Length - NameSuffix.Length);
                }
                return _TagName;
            }
        }

        public static Type GetTypeFromTagName(string tag) {
            //INSTANT C# NOTE: VB local static variable moved to class level:
            //			Static assm As Assembly = GetType(GeneratorAttribute).Assembly
            return GetTypeFromTagName_assm.GetType(tag + NameSuffix);
        }


        public virtual bool IsIgnored { get; set; }
        private bool _ApplyToDerivedClasses = true;
        public virtual bool ApplyToDerivedClasses {
            get {
                return _ApplyToDerivedClasses;
            }
            set {
                _ApplyToDerivedClasses = value;
            }
        }

        public virtual bool AreArgumentsEquals(GeneratorOptionAttribute other) {
            return Version == other.Version;
        }
    }
}
