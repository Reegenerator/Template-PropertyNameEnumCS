using System.Globalization;
using System.Text.RegularExpressions;

namespace RgenLib.TaggedSegment {
    static class Constants {
        public const string TagDateFormat = "yyyy'-'MM'-'dd'T'HH':'mm";
        public static readonly CultureInfo TagDateCulture = CultureInfo.InvariantCulture;
        public const string CodeCommentPrefix = "//";
        public const RegexOptions DefaultRegexOption = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline;
        public const string JsonTagPrefix="Reegenerator";
    }
}
