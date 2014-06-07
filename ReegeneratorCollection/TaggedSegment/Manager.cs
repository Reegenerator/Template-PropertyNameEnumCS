

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EnvDTE;
using RgenLib.Attributes;
using RgenLib.Extensions;

namespace RgenLib.TaggedSegment {
    /// <summary>
    /// Parse and generate code wrapped with xml information, so it can be easily found and replaced 
    /// </summary>
    /// <remarks></remarks>
    public partial class Manager<T> where T : TaggedCodeRenderer, new() {

        public Manager(T renderer) {
            _Renderer = renderer;
            _propertyToXml = XmlAttributeAttribute.GetPropertyToXmlAttributeTranslation(_Renderer.OptionAttributeType);

            Init();

        }
        private readonly Dictionary<string, string> _propertyToXml;
        private readonly T _Renderer;
        public T Renderer {
            get { return _Renderer; }
        }

        public Writer CreateWriter() {
            return new Writer(this);
        }

        #region Regex
        public const string RegionBeginKeyword = "#region";
        public const string RegionEndKeyword = "#endregion";

        private Regex _CommentRegex;
        public Regex CommentRegex {
            get {
                return _CommentRegex;
            }
        }

        private Regex _RegionRegex;

        public Regex RegionRegex {
            get {
                return _RegionRegex;
            }
        }

        private void Init() {
            //initialize regex
            const string regionPatternFormat = @"
            (\{3}\s*(?<textinfo>[^<\r\n]*?)(?<xml><{0}\s*{1}='{2}'.*?/>))\s*
            (?<content> 
                (?>
		        (?! \{3} |\{4}) .
	        |
		        \{3} (?<Depth>)
	        |
		        \{4} (?<-Depth>)
	        )*
	        (?(Depth)(?!))
        
            )
            \{4}";

            const string commentPatternFormat = @"
            (
            {3}(?<tag><{0}\s*{1}='{2}'\s*[^<>]*/>)
            )
            |           
            (
                {3}(?<tag><{0}\s*{1}='{2}'\s*
                    [^<>]*#Match everything but tag symbols
                    (?<!/)>)\s*#Match only > but not />
                (?<content>.*?)(?<!</{0}>)
                {3}(?<tagend></Gen>)\s*
            )";


            var rendererAttr = Renderer.TagPrototype.Attribute(Tag.RendererAttributeName);
            var tagName = Renderer.TagPrototype.Name.LocalName;

            var commentPattern = string.Format(commentPatternFormat, tagName, rendererAttr.Name, rendererAttr.Value, XmlWriter.CodeCommentPrefix);
            _CommentRegex = new Regex(commentPattern, General.DefaultRegexOption);
            var regPattern = string.Format(regionPatternFormat, tagName, rendererAttr.Name, rendererAttr.Value, RegionBeginKeyword, RegionEndKeyword);
            _RegionRegex = new Regex(regPattern, General.DefaultRegexOption);


        }

        private Regex GetRegexByType(Types segmentType) {
            switch (segmentType) {
                case Types.Region:
                    return RegionRegex;
                case Types.Statements:
                    return CommentRegex;
                default:
                    return null;
            }

        }
        #endregion


        #region Properties

        //private static T _GenAttribute;

        ///// <summary>
        ///// Generator Attribute
        ///// </summary>
        ///// <value></value>
        ///// <returns></returns>
        ///// <remarks></remarks>
        ////INSTANT C# TODO TASK: The following line could not be converted:
        //static T GenAttribute {
        //    get { return _GenAttribute ?? (_GenAttribute = new T()); }
        //}




        #endregion

        /// <summary>
        /// Find textPoint marked with '<code>'<Gen Type="InsertPoint" /></code>
        /// </summary>
        /// <param name="writer"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public GeneratedSegment FindInsertionPoint(Writer writer) {

            return FindSegments(writer, TagTypes.InsertPoint).FirstOrDefault();
        }
        public GeneratedSegment[] FindExistingSegments(Writer writer) {

            return FindSegments(writer, TagTypes.Generated).Where(x => x.Category == writer.Tag.Category).ToArray();

        }
        public IEnumerable<GeneratedSegment> FindSegments(Writer writer, TagTypes tagType) {
            return FindSegments(writer).Where(x => x.TagType == tagType);
        }

        /// <summary>
        /// Find tagged segment within GenInfo.SearchStart and GenInfo.SearchEnd
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Not using EditPoint.FindPattern because it can only search from startpoint to end of doc, no way to limit to selection
        /// Not using DTE Find because it has to change params of current find dialog, might screw up normal find usage
        ///  </remarks>
        public GeneratedSegment[] FindSegments(Writer info) {

            var regex = GetRegexByType(info.SegmentType);

            //Using regex in FindPattern does
            var text = info.GetSearchText();
            var matches = regex.Matches(text);
            var segments = new List<GeneratedSegment>();
            foreach (var m in matches.Cast<Match>()) {
                EditPoint matchStart = null;
                EditPoint matchEnd = null;

                if (m.Success) {
                    //Convert match into start and end TextPoints
                    matchStart = info.SearchStart.CreateEditPoint();
                    matchStart.CharRightExact(m.Index);
                    matchEnd = matchStart.CreateEditPoint();
                    matchEnd.CharRightExact(m.Length);

                }

                var range = new TextRange(matchStart, matchEnd);
                if (range.IsValid) {
                    var tag = ParseTextRange(range);
                    segments.Add(tag);
                }
            }
            return segments.ToArray();
        }
        public bool IsAnyOutdated(Writer info) {
            var segments = FindExistingSegments(info);
            return !segments.Any() || segments.Any(x => x.IsOutdated(info.Tag));
        }



        public void Remove(Writer info) {
            var taggedRanges = FindSegments(info);
            foreach (var t in taggedRanges) {
                t.Range.DeleteText();
            }

        }


        public TypeCache OptionAttributeTypeCache {
            get { return TypeResolver.ByType(_Renderer.OptionAttributeType); }
        }

      


        /// <summary>
        /// Parse Attribute Argument into the actual string value
        /// </summary>
        /// <param name="propInfo"></param>
        /// <param name="value"></param>
        /// <remarks>
        /// Attribute argument is presented exactly as it was typed
        /// Ex: SomeArg="Test" would result in the Argument.Value "Test" (with quote)
        /// Ex: SomeArg=("Test") would result in the Argument.Value ("Test") (with parentheses and quote)
        /// </remarks>
        private string ParseXmlAttributeValue(PropertyInfo propInfo, string value) {
            string stringValue = null;
            var propType = propInfo.PropertyType;
            if (propType.IsEnum) {
                //if enum, remove the Enum qualifier (e.g TagTypes.InsertPoint => InserPoint)
                stringValue = value.StripQualifier();
            }
            else if (propType == typeof(string)) {
                //remove quotes
                stringValue = value.Trim('\"');
            }
            else {
                stringValue = value;
            }
            return stringValue;
        }



        /// <summary>
        /// Extract valid xml inside Region Name and within inline comment
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public XElement ExtractXml(TextRange range) {
            var firstline = range.StartPoint.CreateEditPoint().GetLineText();
            var segmentType = firstline.Trim().StartsWith(RegionBeginKeyword) ? Types.Region : Types.Statements;
            var text = range.GetText();
            var xmlContent = "";
            switch (segmentType) {
                case Types.Region:
                    xmlContent = RegionRegex.Replace(text, "${xml}");
                    break;
                case Types.Statements:
                    xmlContent = CommentRegex.Replace(text, "${tag}${content}${tagend}");
                    break;
            }

            return XDocument.Parse(xmlContent).Root;
        }
        private GeneratedSegment ParseTextRange(TextRange range) {
            string name;
            try {
                var tag = new GeneratedSegment(range);
                var xmlProps = XmlAttributeAttribute.GetXmlProperties(_Renderer.OptionAttributeType);
                var xTag = ExtractXml(range);
                foreach (var attr in xTag.Attributes()) {
                    name = attr.Name.LocalName;

                    //skip renderer name
                    if (name == Tag.RendererAttributeName) {
                        continue;
                    }

                    var prop = xmlProps[name];
                    prop.SetValue(tag, ParseXmlAttributeValue(prop, attr.Value));

                }
                return tag;
            }
            catch (Exception ex) {

                Debug.DebugHere();
                throw;
            }

        }
    }
}