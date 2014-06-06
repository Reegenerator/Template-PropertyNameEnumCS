using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;

namespace ReegeneratorCollection.Attributes {
    /// <summary>
    /// BaseClass for Generator Attributes
    /// </summary>
    /// <remarks>
    /// We cannot use nested class for the attribute (e.g. inside the renderer, because then other project would require 
    /// reference to Kodeo.Regenerator.dll, since the renderer is derived from CodeRenderer
    /// </remarks>
    public class GeneratorAttribute : Attribute {
    


    }

}