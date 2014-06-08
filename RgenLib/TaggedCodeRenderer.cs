using System;
using Kodeo.Reegenerator.Generators;

namespace RgenLib {
    public abstract class TaggedCodeRenderer : CodeRenderer {

        /// <summary>
        /// Add this attribute to mark target code elements for code generation. Also to specify options for the generation.
        /// </summary>
        public abstract Type OptionAttributeType { get; }

        protected static Version _version;
        public Version Version { get { return _version; } }
    }
}
