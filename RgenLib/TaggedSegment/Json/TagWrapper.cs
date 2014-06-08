namespace RgenLib.TaggedSegment.Json {
    /// <summary>
    /// This class is only used to wrap Tag so the json produced is wrapped in {Reegenerator:{...Tag...}}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TagWrapper<T> where T : TaggedCodeRenderer, new()
    {
        public Manager<T>.Tag Reegenerator { get; set; }

        public TagWrapper(Manager<T>.Tag tag)
        {
            Reegenerator = tag;
        }

        public const string MainPropertyName = "Reegenerator";
        public static TagWrapper<T> Wrap(Manager<T>.Tag tag)
        {
            return new TagWrapper<T>(tag);
        }
    }
}
