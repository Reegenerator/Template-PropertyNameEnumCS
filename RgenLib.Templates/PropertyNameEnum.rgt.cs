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

        public PropertyNameEnum() {
            Manager = new ManagerType(this);
        }

        private ManagerType Manager;
        public void WritePropertyNames(CodeClass2 cls)
        {
          

            var props = cls.GetProperties().Select(p => p.ToPropertyInfo()).ToArray();
            var output = new System.IO.StringWriter();
            GenEnum(output, props, true);

            var writer = Manager.CreateWriter() ;
            writer.Class = cls;
            writer.SearchStart = cls.StartPoint;
            writer.SearchEnd = cls.EndPoint;
            writer.OptionTag = new ManagerType.OptionTag() { RegenMode = RegenModes.Always };
            writer.Content = output.ToString();
            writer.InsertStart =cls.GetStartPoint(vsCMPart.vsCMPartBody);
            writer.InsertOrReplace();

        }

        public override RenderResults Render()
        {
            var cls = RgenLib.Extensions.ElementAtCursor.GetClassAtCursor(ProjectItem.DteObject.DTE);
            if (cls == null) {
                MessageBox.Show("No class found at cursor");
            }
            else {
                WritePropertyNames(cls);

            }
            //unused. Call the GenEnum function instead
            return null;
        }

        

        private Type _OptionType;
        public override Type OptionAttributeType {
            get
            {
                _OptionType = _OptionType ?? typeof (PropertyNamesOptionAttribute);
                return _OptionType;
            }
        }
    }
}