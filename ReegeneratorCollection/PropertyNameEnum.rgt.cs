using System.Windows.Forms;
using EnvDTE80;
using EnvDTE;
using Kodeo.Reegenerator.Generators;
using Kodeo.Reegenerator.Wrappers;
using ReegeneratorCollection.Extensions;
using ReegeneratorCollection.TaggedSegment;

namespace ReegeneratorCollection {
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

        private Manager<ReegeneratorCollection.Attributes.PropertyNamesAttribute> Manager = new Manager<ReegeneratorCollection.Attributes.PropertyNamesAttribute>();
        public void WritePropertyNames(CodeClass2 cls) {
            var props = cls.GetProperties().Select(p => p.ToPropertyInfo()).ToArray();
            var output = new System.IO.StringWriter();
            GenEnum(output, props, true);
            var code = output.ToString();

            var start =cls.GetStartPoint(vsCMPart.vsCMPartBody);
            start.CreateEditPoint().InsertAndFormat(code);

        }

        public override RenderResults Render() {
            var cls = Extensions.ElementAtCursor.GetClassAtCursor(ProjectItem.DteObject.DTE);
            if (cls == null) {
                MessageBox.Show("No class found at cursor");
            }
            else {
                WritePropertyNames(cls);

            }
            //unused. Call the GenEnum function instead
            return null;
        }
    }
}