using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using EnvDTE;
using Newtonsoft.Json;
using RgenLib.Extensions;
using RgenLib.TaggedSegment.Json;

namespace RgenLib.TaggedSegment {
    public partial class Manager<T> where T : TaggedCodeRenderer, new() {
        public class GeneratedSegment : Tag {

            static GeneratedSegment() {
                InitRegex();
            }



            private readonly TextRange _range;
            public TextRange Range {
                get { return _range; }
            }



            public GeneratedSegment(TextRange range) {
                _range = range;
            }

            public bool IsOutdated(OptionTag option) {
                if (RegenMode != option.RegenMode) return true;

                switch (RegenMode) {
                    case RegenModes.Always:
                        return true;
                    default:
                        return Version != option.Version;

                }

            }

            public bool IsAnyOutdated(Writer info) {
                var segments = Find(info);
                return !segments.Any() || segments.Any(x => x.IsOutdated(info.OptionTag));
            }

            #region Regex
            public const string RegionBeginKeyword = "#region";
            public const string RegionEndKeyword = "#endregion";

            // ReSharper disable StaticFieldInGenericType
            private static Dictionary<TagFormat, Dictionary<SegmentTypes, Regex>> _regexDict;
  

    
            static private void InitRegex() {
                _regexDict = new Dictionary<TagFormat, Dictionary<SegmentTypes, Regex>>();
                //initialize regex
                const string xmlRegionPatternFormat = @"
                    [^\S\r\n]* #match whitespace (space/tabs due to document formatting)
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
                    \{4}(\r\n)?";

                const string xmlCommentPatternFormat = @"
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

                //quotes are doubled to escape them inside literal string
                //curly braces are doubled to escape them for string.format
                const string jsonRegionPatternFormat = @"
                    [^\S\r\n]* #match tabs/space, but not newline, before region 
                (\{3}\s*(?<textinfo>[^\r\n]*?)
                            (?<prefix>{0}:)(?<json>\{{{1}:""{2}""[^\r\n]*\}})\s*)
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
                            \{4}(\s*)?";

                var rendererAttr = TagPrototype.Attribute(RendererAttributeName);
                var tagName = TagPrototype.Name.LocalName;

                var templateName = typeof(T).Name;
                _regexDict.Add(TagFormat.Xml, new Dictionary<SegmentTypes, Regex>());
                var xmlCommentPattern = string.Format(xmlCommentPatternFormat, tagName, rendererAttr.Name, rendererAttr.Value, Constants.CodeCommentPrefix);
                _regexDict[TagFormat.Xml].Add(SegmentTypes.Statements,
                    new Regex(xmlCommentPattern, Constants.DefaultRegexOption));
                var xmlRegPattern = string.Format(xmlRegionPatternFormat, tagName, rendererAttr.Name, rendererAttr.Value, RegionBeginKeyword, RegionEndKeyword);
                _regexDict[TagFormat.Xml].Add(SegmentTypes.Region, new Regex(xmlRegPattern, Constants.DefaultRegexOption));

                _regexDict.Add(TagFormat.Json, new Dictionary<SegmentTypes, Regex>());
                var jsonRegPattern = string.Format(jsonRegionPatternFormat,
                                                    Constants.JsonTagPrefix,
                                                    TemplateNamePropertyName,
                                                    templateName,
                                                    RegionBeginKeyword, RegionEndKeyword);
                _regexDict[TagFormat.Json].Add(SegmentTypes.Region, new Regex(jsonRegPattern, Constants.DefaultRegexOption));
            }
            #endregion


            #region Parse

            /// <summary>
            /// Extract valid xml inside Region Name and within inline comment
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// </remarks>
            public static XElement ExtractXml(TextRange range) {

                var firstline = range.StartPoint.CreateEditPoint().GetLineText();
                var segmentType = firstline.Trim().StartsWith(RegionBeginKeyword) ? SegmentTypes.Region : SegmentTypes.Statements;
                var text = range.GetText();
                var xmlContent = "";
                switch (segmentType) {
                    case SegmentTypes.Region:
                        xmlContent = _regexDict[TagFormat.Xml][SegmentTypes.Region].Replace(text, "${xml}");
                        break;
                    case SegmentTypes.Statements:
                        xmlContent = _regexDict[TagFormat.Xml][SegmentTypes.Statements].Replace(text, "${tag}${content}${tagend}");
                        break;
                }

                return XDocument.Parse(xmlContent).Root;
            }
            /// <summary>
            /// Extract valid xml inside Region Name and within inline comment
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// </remarks>
            public static string ExtractJson(TextRange range) {

                var firstline = range.StartPoint.CreateEditPoint().GetLineText();
                var segmentType = firstline.Trim().StartsWith(RegionBeginKeyword) ? SegmentTypes.Region : SegmentTypes.Statements;
                var text = range.GetText();
                var json = "";
                switch (segmentType) {
                    case SegmentTypes.Region:
                        json = _regexDict[TagFormat.Json][SegmentTypes.Region].Replace(text, "${json}");
                        break;
                    //case SegmentTypes.Statements:
                    //    json = XmlCommentRegex.Replace(text, "${tag}${content}${tagend}");
                    //    break;
                }

                return json;

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
            private static object ParseXmlAttributeValue(PropertyInfo propInfo, string value) {
                //Debug.DebugHere();

                object parsed;
                var propType = propInfo.PropertyType;
                if (propType.IsEnum) {
                    //if enum, remove the Enum qualifier (e.g TagTypes.InsertPoint => InserPoint)
                    parsed = Enum.Parse(typeof(RegenModes), value);
                }
                else if (propType == typeof(DateTime) || propType == typeof(DateTime?)) {
                    parsed = DateTime.ParseExact(value, Constants.TagDateFormat, Constants.TagDateCulture);
                }
                else if (propType == typeof(string)) {
                    //remove quotes
                    parsed = value.Trim('\"');
                }

                else {
                    parsed = value;
                }
                return parsed;
            }


            private static GeneratedSegment ParseJson(TextRange range) {
                Debug.DebugHere();
                var tag = new GeneratedSegment(range);
                var json = ExtractJson(range);
                JsonConvert.PopulateObject(json, tag);
                return tag;
            }
            private static GeneratedSegment ParseTextRange(TextRange range, TagFormat tagFormat) {
                try {
                    switch (tagFormat) {
                        case TagFormat.Json:
                            return ParseJson(range);
                        case TagFormat.Xml:
                            return ParseXml(range);
                    }
                }
                catch (Exception ex) {

                    Debug.DebugHere(ex);
                    throw;
                }
                return null;
            }
            private static GeneratedSegment ParseXml(TextRange range) {
                try {
                    var tag = new GeneratedSegment(range);
                    var xmlProps = XmlAttributeAttribute.GetXmlProperties(typeof(GeneratedSegment));
                    var xTag = ExtractXml(range);
                    foreach (var attr in xTag.Attributes()) {
                        var name = attr.Name.LocalName;


                        //skip renderer name
                        if (name == RendererAttributeName) {
                            continue;
                        }

                        var prop = xmlProps[name];
                        prop.SetValue(tag, ParseXmlAttributeValue(prop, attr.Value));

                    }
                    return tag;
                }
                catch (Exception ex) {

                    Debug.DebugHere(ex);
                    throw;
                }

            }

            #endregion


            #region Find

            /// <summary>
            /// Find textPoint marked with '<code>'<Gen Type="InsertPoint" /></code>
            /// </summary>
            /// <param name="writer"></param>
            /// <returns></returns>
            /// <remarks>
            /// </remarks>
            static public GeneratedSegment FindInsertionPoint(Writer writer) {

                return Find(writer, TagTypes.InsertPoint).FirstOrDefault();
            }
            static public GeneratedSegment[] Find(Writer writer) {

                return Find(writer, TagTypes.Generated).Where(x => x.Category == writer.OptionTag.Category).ToArray();

            }
            static public IEnumerable<GeneratedSegment> Find(Writer writer, TagTypes tagType) {
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
            static public GeneratedSegment[] FindSegments(Writer writer) {

                var regex = _regexDict[writer.TagFormat][writer.SegmentType];
                //Using regex in FindPattern does
                var text = writer.GetSearchText();
                var matches = regex.Matches(text);
                var segments = new List<GeneratedSegment>();
                foreach (var m in matches.Cast<Match>()) {
                    EditPoint matchStart = null;
                    EditPoint matchEnd = null;

                    if (m.Success) {
                        //Convert match into start and end TextPoints
                        matchStart = writer.SearchStart.CreateEditPoint();
                        matchStart.CharRightExact(m.Index);
                        matchEnd = matchStart.CreateEditPoint();
                        matchEnd.CharRightExact(m.Length);

                    }

                    var range = new TextRange(matchStart, matchEnd);
                    if (range.IsValid) {
                        var tag = ParseTextRange(range, writer.Manager.TagFormat);
                        segments.Add(tag);
                    }
                }
                return segments.ToArray();
            }





            #endregion

        }
    }
}