using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Kodeo.Reegenerator.Generators;
using RgenLib.Attributes;

namespace RgenLib {
    public abstract class TaggedCodeRenderer : CodeRenderer {
        public abstract XElement TagPrototype { get; }

        /// <summary>
        /// Add this attribute to mark target code elements for code generation. Also to specify options for the generation.
        /// </summary>
        public abstract Type OptionType { get; }

    }
}
