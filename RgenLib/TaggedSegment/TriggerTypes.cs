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
        AttributeInBaseClass,
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
        Never = Default,
        OnVersionChanged,
        Always
    }

    public enum TagFormat {
        Default,
        Json = Default,
        Xml
    }
}
