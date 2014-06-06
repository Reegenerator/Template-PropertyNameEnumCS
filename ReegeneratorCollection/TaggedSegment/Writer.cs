
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using ReegeneratorCollection.Attributes;
using ReegeneratorCollection.Extensions;
using TextPoint = EnvDTE.TextPoint;
using System.Reflection;

namespace ReegeneratorCollection.TaggedSegment {
    public partial class Manager<T> where T : GeneratorAttribute, new()
    {

        /// <summary>
        /// Holds information required to generate code segments
        /// </summary>
        /// <remarks></remarks>
        public class Writer {



            public Writer() {
                //Do nothing
                GenAttribute = new T();
            }

            /// <summary>
            /// Create a new writer with the same Class, TriggeringBaseClass and GeneratorAttribute
            /// </summary>
            /// <param name="parentWriter">
            /// source of properties to be copied
            /// </param>
            /// <param name="segClass"></param>
            /// <remarks></remarks>
            public Writer(Writer parentWriter, string segClass = "") {
                Class = parentWriter.Class;
                TriggeringBaseClass = parentWriter.TriggeringBaseClass;
                //Clone instead of reusing parent's attribute, because they may have different property values
                GenAttribute = (T)parentWriter.GenAttribute.MemberwiseClone();
                GenAttribute.SegmentClass = segClass;
            }



            public CodeClass2 TriggeringBaseClass { get; set; }
            public CodeClass2 Class { get; set; }
            public T GenAttribute { get; set; }
            public TextPoint SearchStart { get; set; }
            public TextPoint SearchEnd { get; set; }
            public TextPoint InsertStart { get; set; }
            public TextPoint InsertedEnd { get; set; }
            public Types SegmentType { get; set; }
            public string Content { get; set; }
            public string ProcessedContent { get; set; }
            public string TagComment { get; set; }
            private bool _OpenFileOnGenerated = true;
            public bool OpenFileOnGenerated {
                get {
                    return _OpenFileOnGenerated;
                }
                set {
                    _OpenFileOnGenerated = value;
                }
            }
            public bool HasError { get; set; }

            private StringBuilder _Status;
            public StringBuilder Status {
                get {
                    if (_Status == null) {
                        _Status = new StringBuilder();
                    }
                    return _Status;
                }
            }

            /// <summary>
            /// True if the code generation was triggered by the base of current class
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            public bool IsTriggeredByBaseClass {
                get {
                    return TriggeringBaseClass != null && TriggeringBaseClass != Class;
                }
            }


            public void OutlineText() {

                var endWithoutNewline = InsertedEnd.CreateEditPoint();
                endWithoutNewline.CharLeft(1);
                InsertStart.CreateEditPoint().OutlineSection(endWithoutNewline);
            }

            public TextPoint GetContentEndPoint() {
                EditPoint endP = InsertStart.CreateEditPoint();
                endP.CharRightExact(Content.Length);
                return endP;
            }

            public string GetSearchText() {
                return SearchStart.CreateEditPoint().GetText(SearchEnd);
            }

            public TextPoint InsertAndFormat() {
                var text = GenText();
                InsertedEnd = InsertStart.InsertAndFormat(text);
                return InsertedEnd;
            }

            #region Tag Generation //!――――――――――――――――――――――――――――――――――――――――――――――――――――――

            public XElement GenXmlTag() {
                //set to nothing if it's by Attribute(default) so Trigger attribute is not written out
                GeneratorAttribute.TriggerTypes? triggerType = null;
                if (IsTriggeredByBaseClass) {
                    triggerType = GeneratorAttribute.TriggerTypes.BaseClassAttribute;
                }

                var triggerInfo = (triggerType == GeneratorAttribute.TriggerTypes.BaseClassAttribute) ? TriggeringBaseClass.Name : null;

                var xml = new XElement(GenAttribute.TagPrototype);
                if (triggerType != null) {
                    xml.SetAttributeValue("Trigger", triggerType.ToString());
                }
                if (triggerInfo != null) {
                    xml.SetAttributeValue("TriggerInfo", triggerInfo);
                }
                xml.SetAttributeValue("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                var xmlNameType = typeof(XmlPropertyAttribute);
                var membersWithXmlName = GenAttribute.TypeCache.GetMembers()
                    .Select(m => new { Member = m, XmlName = m.GetCustomAttributes<XmlPropertyAttribute>().FirstOrDefault() })
                    .Where(x => x.XmlName != null);



                foreach (var p in GenAttribute.GetXmlProperties()) {

                    var propValue = p.Value.GetValue(GenAttribute);
                    if (propValue != null) {
                        xml.Add(new XAttribute(p.Key, propValue));
                    }

                }
                return xml;
            }

            public string CreateTaggedCommentText() {
                //?Newline is added surrounding the text because we can't figure out how to add newline in TagXmlWriter
                var xml = GenXmlTag();
                xml.Add(Environment.NewLine + Content + Environment.NewLine);
                return XmlWriter.ToCommentedString(xml);
            }

            public string CreateTaggedRegionName() {
                var xml = GenXmlTag();
                var regionNameXml = XmlWriter.ToRegionNameString(xml);
                return TagComment.Conjoin("\t", regionNameXml);
            }

            public string GenTaggedRegionText() {
                var res = string.Format("#region {0}{1}{2}{1}{3}{1}", CreateTaggedRegionName(), Environment.NewLine, Content, "#endregion");
                return res;
            }

            public string GenText() {
                switch (SegmentType) {
                    case Types.Region:
                        return GenTaggedRegionText();
                    case Types.Statements:
                        return CreateTaggedCommentText();
                    default:
                        throw new Exception("Unknown SegmentType");
                }
            }



            #endregion //!―――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――――

        }

    }
}