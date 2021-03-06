using System.Windows.Forms;
using System.Xml.Linq;
using EnvDTE80;
using EnvDTE;
using Kodeo.Reegenerator.Generators;
using Kodeo.Reegenerator.Wrappers;
using RgenLib.Attributes;
using RgenLib.Extensions;
using RgenLib.TaggedSegment;
using ManagerType = RgenLib.TaggedSegment.Manager<RgenLib.Templates.PropertyNameEnum>;
namespace RgenLib.Templates {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Kodeo.Reegenerator;

    [CodeSnippet]
    public partial class PropertyNameEnum {
        /// <summary>
        /// Method that gets called prior to calling <see cref="Render"/>.
        /// Use this method to initialize the properties to be used by the render process.
        /// You can access the project item attached to this generator by using the <see cref="ProjectItem"/> property.
        /// </summary>
        public override void PreRender() {
            base.PreRender();
            // var projectItem = base.ProjectItem;
        }

        static PropertyNameEnum() {
            _version = new Version(1, 0, 0);
        }

        public PropertyNameEnum() {
            Manager = new ManagerType(this, TagFormat.Json);
        }

        private ManagerType Manager;
        public void WritePropertyNames(CodeClass2 cls)
        {
          

            var props = cls.GetProperties().Select(p => p.ToPropertyInfo()).ToArray();
            var output = new System.IO.StringWriter();
            GenEnum(output, props, true);

            var writer = Manager.CreateWriter() ;
            writer.Class = cls;
            writer.SegmentType = SegmentTypes.Region;
            writer.SearchStart = cls.StartPoint;
            writer.SearchEnd = cls.EndPoint;
            writer.OptionTag = new ManagerType.OptionTag() { Version = Version, RegenMode = RegenModes.Always, Trigger = new TriggerInfo( TriggerTypes.CodeSnippet)};
            writer.Content = output.ToString();
            writer.InsertStart =cls.GetStartPoint(vsCMPart.vsCMPartBody);
            writer.InsertOrReplace();

        }
        public string ClassName { get; set; }
        public override RenderResults Render()
        {
            var cls = GetClassAtCursor();
            if (cls == null) {
                MessageBox.Show("No class found at cursor");
            }
            else {
                WritePropertyNames(cls);

            }
            //unused. Call the GenEnum function instead
            return null;
        }

        private CodeClass2 GetClassAtCursor()
        {
            ClassName = "PropertyNames";
            var cls =ElementAtCursor.GetClassAtCursor(ProjectItem.DteObject.DTE);
            //if we are inside the generated class, use the parent class instead
            if (cls.Name == ClassName)
            {
                cls =(CodeClass2) cls.Parent;
            }
            return cls;
        }

        private Type _optionType;
        public override Type OptionAttributeType {
            get
            {
                _optionType = _optionType ?? typeof (PropertyNamesOptionAttribute);
                return _optionType;
            }
        }
    }
}