
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using RgenLib.Extensions;
using RgenLib.Attributes;
using TextPoint = EnvDTE.TextPoint;
using System.Reflection;

namespace RgenLib.TaggedSegment {
    public partial class Manager<T> where T : TaggedCodeRenderer, new() {

        /// <summary>
        /// Holds information required to generate code segments
        /// </summary>
        /// <remarks></remarks>
        public class Writer {
            private readonly Manager<T> _Manager;

            public Manager<T> Manager { get { return _Manager; } }
            public OptionTag Tag { get; set; }

            public Writer(Manager<T> manager) {
                _Manager = manager;

            }


            /// <summary>
            /// Create a new writer with the same Class, TriggeringBaseClass and GeneratorAttribute
            /// </summary>
            /// <param name="parentWriter">
            /// source of properties to be copied
            /// </param>
            /// <param name="segCategory"></param>
            /// <remarks></remarks>
            public Writer(Writer parentWriter, string segCategory = "") {
                Class = parentWriter.Class;
                TriggeringBaseClass = parentWriter.TriggeringBaseClass;
                //Clone instead of reusing parent's attribute, because they may have different property values
                Tag = (OptionTag)parentWriter.Tag.MemberwiseClone();
                Category = segCategory;
            }


            public string Category { get; set; }
            public CodeClass2 TriggeringBaseClass { get; set; }
            public CodeClass2 Class { get; set; }
            //public T Renderer { get; set; }
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
                get { return _Status ?? (_Status = new StringBuilder()); }
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

                var endPointExcludingNewline = InsertedEnd.CreateEditPoint();
                endPointExcludingNewline.CharLeft(1);
                InsertStart.CreateEditPoint().OutlineSection(endPointExcludingNewline);
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
                //set to null if it's default, so it doesn't need to be written in the tag
                var triggerType = IsTriggeredByBaseClass ? (TriggerTypes?)TriggerTypes.BaseClassAttribute : null;
                var triggerInfo = (triggerType == TriggerTypes.BaseClassAttribute) ? TriggeringBaseClass.Name : null;

                var xml = new XElement(_Manager.Renderer.TagPrototype);
                if (triggerType != null) {
                    xml.SetAttributeValue("Trigger", triggerType.ToString());
                }
                if (triggerInfo != null) {
                    xml.SetAttributeValue("TriggerInfo", triggerInfo);
                }
             
                xml.SetAttributeValue("Date", DateTime.Now.ToString(CultureInfo.InvariantCulture));

                var xmlNameType = typeof(XmlAttributeAttribute);
                //var membersWithXmlName = _Manager.Renderer.OptionAttributeTypeCache.GetMembers()
                //    .Select(m => new { Member = m, XmlName = m.GetCustomAttributes<XmlAttributeAttribute>().FirstOrDefault() })
                //    .Where(x => x.XmlName != null);

                foreach (var keyValuePair in Manager.GetXmlAttributes()) {

                    var propValue = keyValuePair.Value.GetValue(Tag);
                    //only write the xml attribute if it has value, to keep the tag concise
                    if (propValue != null) {
                        xml.Add(new XAttribute(keyValuePair.Key, propValue));
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


            /// <summary>
            /// Insert or Replace text in taggedRange if outdated (or set to always generate)
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            public bool InsertOrReplace() {
                var generatedSegments = Manager.FindExistingSegments(this);
                var needInsert = false;
                if (generatedSegments.Length == 0) {
                    //if none found, then insert
                    needInsert = true;
                }
                else {
                    //if any is outdated, delete, and reinsert
                    foreach (var t in
                        from t1 in generatedSegments
                        where t1.IsOutdated(Tag)
                        select t1) {

                        t.Range.DeleteText();
                        needInsert = true;
                    }
                }
                if (!needInsert) {
                    return false;
                }

                InsertAndFormat();
                //!Open file if requested
                if (OpenFileOnGenerated && Class != null) {
                    if (!Class.ProjectItem.IsOpen) {
                        Class.ProjectItem.Open();
                    }
                }
                return true;
            }

        }

    }
}