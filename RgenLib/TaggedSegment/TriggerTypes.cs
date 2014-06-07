using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RgenLib.TaggedSegment {
    /// <summary>
    /// Cause of code generation
    /// </summary>
    /// <remarks></remarks>
    public enum TriggerTypes {
        /// <summary>
        /// Code generation is triggered because the class is marked with a GeneratorAttribute 
        /// </summary>
        /// <remarks></remarks>
        Attribute,
        /// <summary>
        /// Code generation is triggered because the baseClass is marked with a GeneratorAttribute
        /// </summary>
        /// <remarks></remarks>
        BaseClassAttribute,
        /// <summary>
        /// Code generation was triggered by calling it as Reegenerator CodeSnippet
        /// </summary>
        CodeSnippet
    }
    public enum TagTypes {
        Generated,
        InsertPoint
    }


    public enum RegenModes {
        Default,
        OnVersionChanged = Default,
        Once,
        Always
    }

}
