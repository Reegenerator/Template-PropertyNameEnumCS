using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RgenLib.Extensions;

namespace RgenLib.TaggedSegment {
    public class GeneratedSegment : Tag {

        private static readonly PropertyInfo[] _properties;
        static GeneratedSegment() {
            _properties = TypeResolver.ByType(typeof(GeneratedSegment)).GetProperties().ToArray();

        }

        private readonly TextRange _range;
        [IgnorePropertyInComparison]
        public TextRange Range { get { return _range; } }
        public DateTime? GenerateDate { get; set; }
        public GeneratedSegment(TextRange range) {
            _range = range;
        }

        public bool IsOutdated(OptionTag option) {
            switch (RegenMode) {
                case RegenModes.Always:
                    return true;
                default:
                    return !Equals(option);

            }

        }

        /// <summary>
        /// return current values of all properties
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<string, object> GetCurrentValues() {
            return _properties.Where(p2 => !p2.HasAttribute<IgnorePropertyInComparisonAttribute>())
                        .ToDictionary(p => p.Name, p => p.GetValue(this));

        }
    }
}
