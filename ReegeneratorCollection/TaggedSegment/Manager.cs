

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EnvDTE;
using ReegeneratorCollection.Attributes;
using ReegeneratorCollection.Extensions;

namespace ReegeneratorCollection.TaggedSegment {
    /// <summary>
    /// Parse and generate code wrapped with xml information, so it can be easily found and replaced 
    /// </summary>
    /// <remarks></remarks>
    public partial class Manager<T> where T : GeneratorAttribute, new() {


        public Manager() {
            Init();

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
            

            var rendererAttr = _GenAttribute.TagPrototype.Attribute("Renderer");
            var commentPattern = string.Format(commentPatternFormat, _GenAttribute.TagPrototype.Name.LocalName, rendererAttr.Name, rendererAttr.Value, XmlWriter.CodeCommentPrefix);
            _CommentRegex = new Regex(commentPattern, General.DefaultRegexOption);
            string regPattern = string.Format(regionPatternFormat, _GenAttribute.TagPrototype.Name.LocalName, rendererAttr.Name, rendererAttr.Value, RegionBeginKeyword, RegionEndKeyword);
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
        public FoundSegment FindInsertionPoint(Writer writer) {

            return FindSegments(writer, AttributeProcessor.TagTypes.InsertPoint).FirstOrDefault();
        }
        public FoundSegment[] FindGeneratedSegments(Writer writer) {

            return FindSegments(writer, AttributeProcessor.TagTypes.Generated).Where((x) => x.FoundTag.SegmentClass == writer.GenAttribute.SegmentClass).ToArray();

        }
        public IEnumerable<FoundSegment> FindSegments(Writer writer, AttributeProcessor.TagTypes tagType) {
            return FindSegments(writer).Where((x) => x.FoundTag.Type == tagType);
        }

        /// <summary>
        /// Find tagged segment within GenInfo.SearchStart and GenInfo.SearchEnd
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Not using EditPoint.FindPattern because it can only search from startpoint to end of doc, no way to limit to selection
        /// Not using DTE Find because it has to change params of current find dialog, might screw up normal find usage
        ///  </remarks>
        public FoundSegment[] FindSegments(Writer info) {

            var regex = GetRegexByType(info.SegmentType);

            //Using regex in FindPattern does
            var text = info.GetSearchText();
            var matches = regex.Matches(text);
            var segments = new List<FoundSegment>();
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

                var segment = new FoundSegment(this, info.GenAttribute, matchStart, matchEnd);
                if (segment.IsValid) {
                    segments.Add(segment);
                }
            }
            return segments.ToArray();
        }
        public bool IsAnyOutdated(Writer info) {
            var segments = FindGeneratedSegments(info);
            return !segments.Any() || segments.Any((x) => x.IsOutdated());
        }


        /// <summary>
        /// Insert or Replace text in taggedRange if outdated (or set to always generate)
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool InsertOrReplace(Writer info) {
            var taggedRanges = FindGeneratedSegments(info);
            var needInsert = false;
            if (taggedRanges.Length == 0) {
                //if none found, then insert
                needInsert = true;
            }
            else {
                //if any is outdated, delete, and reinsert
                foreach (var t in
                    from t1 in taggedRanges
                    where t1.IsOutdated()
                    select t1) {

                    t.Delete();
                    needInsert = true;
                }
            }
            if (!needInsert) {
                return false;
            }

            info.InsertAndFormat();
            //!Open file if requested
            if (info.OpenFileOnGenerated && info.Class != null) {
                if (!info.Class.ProjectItem.IsOpen) {
                    info.Class.ProjectItem.Open();
                }
            }
            return true;
        }
        public void Remove(Writer info) {
            var taggedRanges = FindSegments(info);
            foreach (var t in taggedRanges) {
                t.Delete();
            }

        }
    }
}