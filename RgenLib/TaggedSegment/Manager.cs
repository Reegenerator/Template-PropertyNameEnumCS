using System.Collections.Generic;

namespace RgenLib.TaggedSegment {
    /// <summary>
    /// Parse and generate code wrapped with xml information, so it can be easily found and replaced 
    /// </summary>
    /// <remarks></remarks>
    public partial class Manager<T> where T : TaggedCodeRenderer, new() {
       
        public Manager(T renderer, TagFormat tagFormat)
        {
            _tagFormat = tagFormat;
            _renderer = renderer;
            _propertyToXml = XmlAttributeAttribute.GetPropertyToXmlAttributeTranslation(_renderer.OptionAttributeType);

        }
        private readonly TagFormat _tagFormat;

        public TagFormat TagFormat {
            get { return _tagFormat; }
        }

        private readonly Dictionary<string, string> _propertyToXml;
        private readonly T _renderer;

        public T Renderer {
            get { return _renderer; }
        }

        public Writer CreateWriter() {
            return new Writer(this);
        }



        public void Remove(Writer info) {
            var taggedRanges = GeneratedSegment.FindSegments(info);
            foreach (var t in taggedRanges) {
                t.Range.DeleteText();
            }

        }


        public TypeCache OptionAttributeTypeCache {
            get { return TypeResolver.ByType(_renderer.OptionAttributeType); }
        }

      



      
    }
}