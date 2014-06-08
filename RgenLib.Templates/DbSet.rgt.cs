using EnvDTE;
using RgenLib.Extensions;

namespace RgenLib.Templates {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Kodeo.Reegenerator;

    public partial class DbSet {
        /// <summary>
        /// Method that gets called prior to calling <see cref="Render"/>.
        /// Use this method to initialize the properties to be used by the render process.
        /// You can access the project item attached to this generator by using the <see cref="ProjectItem"/> property.
        /// </summary>
        public override void PreRender() {
            base.PreRender();
            // var projectItem = base.ProjectItem;
        }
        public override Kodeo.Reegenerator.Generators.RenderResults Render() {
            var cls = ElementAtCursor.GetClassAtCursor(ProjectItem.DteObject.DTE);
            //if (cls.GetType().IsSubclassOf(typeof(DbContext)))
            return null;
        }
    }
}