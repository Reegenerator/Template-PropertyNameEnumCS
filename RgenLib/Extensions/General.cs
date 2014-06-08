//Formerly VB project-level imports:
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using EnvDTE;
using EnvDTE80;
using RgenLib.Attributes;
using ProjectItem = Kodeo.Reegenerator.Wrappers.ProjectItem;
using Solution = Kodeo.Reegenerator.Wrappers.Solution;

namespace RgenLib.Extensions
{
	public static class General
	{

#region Code element helpers

        private const string InterfaceImplementationPattern = @"^.*?\sAs\s.*?(?<impl>Implements\s.*?)$";

	    private static readonly Type IsEqual_attrType = typeof(Attribute);
		private static readonly Regex GetInterfaceImplementation_regex = new Regex(InterfaceImplementationPattern, RgenLib.TaggedSegment.Constants.DefaultRegexOption);
		private static readonly Regex RemoveEmptyLines_regex = new Regex("^\\s+$[\\r\\n]*", RegexOptions.Multiline);
		private static readonly Dictionary<string, Assembly> GetTypeFromProject_cache = new Dictionary<string, Assembly>();
		//private static readonly ConcurrentDictionary<CodeClass, Type> ToPropertyInfo_classCache = new ConcurrentDictionary<CodeClass, Type>();
		private static readonly Type GetGeneratorAttribute_type = typeof(GeneratorOptionAttribute);

		public static IEnumerable<CodeClass2> GetClassesEx(this ProjectItem item)
		{
			var classes = item.GetClasses().Values.SelectMany(x => x).Cast<CodeClass2>();
			return classes;
		}


		public static IEnumerable<CodeClass2> GetClassesWithAttributes(this ProjectItem item, Type[] attributes)
		{
			//Replace nested class + delimiter into . as the format used in CodeAttribute.FullName
			var fullNames = attributes.Select(x=> x.DottedFullName()).ToArray();
			var res = item.GetClassesEx().Where(cclass => fullNames.All(attrName => cclass.Attributes.Cast<CodeAttribute>().Any(cAttr => cAttr.FullName == attrName)));

			return res;
		}



		/// <summary>
		/// Returns full name delimited by only dots (and no +(plus sign))
		/// </summary>
		/// <param name="x"></param>
		/// <returns></returns>
		/// <remarks>Nested class is separated with +, while CodeClass delimit them using dots</remarks>
		public static string DottedFullName(this Type x)
		{

			return x.FullName.Replace("+", ".");
		}

		public static IEnumerable<CodeClass2> GetClassesWithAttribute(this ProjectItem item, Type attribute)
		{
            var fullName = attribute.DottedFullName();
			//   all attributes is in class attribute
            var res = item.GetClassesEx().Where(cclass => cclass.Attributes.Cast<CodeAttribute>().Any(x => x.FullName == fullName));

			return res;
		}

		public static IEnumerable<CodeClass2> GetClassesWithAttribute(this DTE dte, Type attribute)
		{
			var projects = Solution.GetSolutionProjects(dte.Solution).Values;
			var res = from p in projects
			          from eleList in p.GetCodeElements<CodeClass2>().Values
			          from ele in eleList
                      where ele.Attributes.Cast<CodeAttribute>().Any(x => x.AsCodeElement().IsEqual(attribute))
			          select ele;

			return res;
		}

		/// <summary>
		/// Use this to convert Code element into a more generic CodeElement and get CodeElement based extensions
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="cc"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static CodeElement2 AsCodeElement<T>(this T cc)
		{
			return (CodeElement2)cc;
		}
		public static CodeType AsCode<T>(this T cc)
		{

			return (CodeType)cc;
		}

		public static bool HasAttribute(this CodeType ct, Type attrType)
		{
			return ct.GetCustomAttribute(attrType) != null;
		}


#region CodeElement2 Attribute Functions

		public static CodeAttribute2 GetCustomAttribute(this CodeElement2 cc, Type attrType)
		{

            var res = cc.GetCustomAttributes().FirstOrDefault(x => x.AsCodeElement().IsEqual(attrType));
			return res;
		}

		/// <summary>
		/// Get Custom Attributes
		/// </summary>
		/// <param name="ce"></param>
		/// <returns></returns>
		/// <remarks>
		/// Requires Named Argument when declaring the Custom Attribute, otherwise Name will be empty.
		/// Not using reflection because it requires successful build
		/// </remarks>
		public static IEnumerable<CodeAttribute2> GetCustomAttributes(this CodeElement2 ce)
		{
			//!Property
			var prop = ce as CodeProperty2;
			if (prop != null)
			{
				return prop.Attributes.Cast<CodeAttribute2>();
			}

			//!Function
			var func = ce as CodeFunction2;
			if (func != null)
			{
				return func.Attributes.Cast<CodeAttribute2>();
			}

			//!Class
			var cc = ce as CodeClass2;
			if (cc != null)
			{
				return cc.Attributes.Cast<CodeAttribute2>();
			}

			throw new Exception("CodeElement not recognized");
			//return Enumerable.Empty<CodeAttribute2>();
		}

		public static bool HasAttribute(this CodeElement2 ct, Type attrType)
		{
			return ct.GetCustomAttribute(attrType) != null;
		}

#endregion
#region GetCustomAttributes of CodeType/CodeClass 


		public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeType ct)
		{
		    var type = Type.GetType(ct.FullName);
		    if (type == null) throw new Exception(string.Format("Type{0} not found",ct.FullName));
		    return type.CustomAttributes;
		}

	    public static IEnumerable<CustomAttributeData> GetCustomAttributes(this CodeClass cc)
		{
            var type = Type.GetType(cc.FullName);
            if (type == null) throw new Exception(string.Format("Type{0} not found", cc.FullName));
            return type.CustomAttributes;
		}
		public static CustomAttributeData GetCustomAttribute(this CodeType ct, Type attrType)
		{
			return ct.GetCustomAttributes().FirstOrDefault(x => x.AttributeType == attrType);
		}
		public static CustomAttributeData GetCustomAttribute(this CodeClass cc, Type attrType)
		{
			return cc.GetCustomAttributes().FirstOrDefault(x => x.AttributeType == attrType);
		}



		public static IEnumerable<CodeAttributeArgument> GetCodeAttributeArguments(this CodeAttribute2 cattr)
		{
			if (cattr == null)
			{
				return Enumerable.Empty<CodeAttributeArgument>();
			}
			return cattr.Arguments.Cast<CodeAttributeArgument>();
		}
		public static bool IsEqual(this CodeElement2 ele, Type type)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static attrType As Type = GetType(Attribute)
			return ele.FullName == type.FullName || ele.Name == type.Name || (type.IsSubclassOf(IsEqual_attrType) && (ele.FullName + "Attribute" == type.FullName || ele.Name + "Attribute" == type.Name));
		}

#endregion
#region CodeClass members(property, function, variable) helper

		public static IEnumerable<CodeProperty2> GetProperties(this CodeClass cls)
		{
			return cls.Children.OfType<CodeProperty2>();
		}
		public static IEnumerable<CodeFunction2> GetFunctions(this CodeClass cls)
		{
			return cls.Children.OfType<CodeFunction2>();
		}

		public static CodeProperty2[] GetAutoProperties(this CodeClass2 cls)
		{

            var props = cls.GetProperties().ToArray();
		    Func<CodeProperty, bool> isAutoProperty = x => !x.Setter.GetText().Contains("{");
		 
			return props.Where(x => x.ReadWrite == vsCMPropertyKind.vsCMPropertyKindReadWrite &&
               isAutoProperty(x) && x.OverrideKind != vsCMOverrideKind.vsCMOverrideKindAbstract).ToArray();
		}

		public static IEnumerable<CodeVariable> GetVariables(this CodeClass cls)
		{
			return cls.Children.OfType<CodeVariable>();
		}
		public static IEnumerable<CodeVariable> GetDependencyProperties(this CodeClass cls)
		{
			try
			{

				var sharedFields = cls.GetVariables().Where(x => x.IsShared && x.Type.CodeType != null);
				return sharedFields.Where(x => x.Type.CodeType.FullName == "System.Windows.DependencyProperty");
			}
			catch (Exception ex)
			{

			    Debug.DebugHere(ex);
			}
			return null;
		}



		/// <summary>
		/// Get Bases recursively
		/// </summary>
		/// <returns></returns>
		/// <remarks></remarks>
		public static IEnumerable<CodeClass2> GetAncestorClasses(this CodeClass2 cc)
		{
		
            var bases = cc.Bases.OfType<CodeClass2>().ToArray();

			if (bases.FirstOrDefault() == null)
			{
				return bases;
			}
			var grandBases = bases.SelectMany(x => x.GetAncestorClasses());

			return bases.Concat(grandBases);

		}
#endregion
#endregion

		public static string GetText(this CodeProperty2 prop, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
		{
		   
		    try
		    {
                var p = prop.GetStartPoint(part);
                if (p == null) {
                    return "";
                }
                return p.CreateEditPoint().GetText(prop.GetEndPoint(part));
		    }
		    catch (Exception ex)
		    {
		        if (ex is COMException || ex is NotImplementedException)
		        {
                    return "";
		        }
                throw;
		    }


		}
        public static string GetText(this CodeFunction ele, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes) {
            var p = ele.GetStartPoint(part);
            if (p == null) {
                return "";
            }
            return p.CreateEditPoint().GetText(ele.GetEndPoint(part));

        }
		public static string GetText(this CodeClass2 cls, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
		{
            try {
			var startPoint = cls.GetStartPoint(part);
			if (startPoint == null) return "";

                var endPoint = cls.GetEndPoint(part);
		  
		    return endPoint == null ? "" : startPoint.CreateEditPoint().GetText(endPoint);
            }
            catch (NotImplementedException e) {
                //catch random errors when trying to get start / end point
                Console.WriteLine(e.ToString());
                return "";
            }
		}

		public static string GetInterfaceImplementation(this CodeProperty2 prop)
		{

			var g = GetInterfaceImplementation_regex.Match(prop.GetText(vsCMPart.vsCMPartHeader)).Groups["impl"];

			//add space to separate
			if (g.Success)
			{
				return " " + g.Value;
			}
			return null;
		}
		public static EnvDTE.TextPoint GetAttributeStartPoint(this CodeProperty2 prop)
		{
			return prop.GetStartPoint();
		}


		private static Regex _DocCommentRegex;
		/// <summary>
		/// Lazy Regex property to match doc comments
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Regex DocCommentRegex
		{
			get
			{
			    const string docCommentPattern = @"\s*///";
			    return _DocCommentRegex ?? (_DocCommentRegex = new Regex(docCommentPattern));
			}
		}

        /// <summary>
        /// GetStartPoint can throw NotImplementedException. This will retry the start point without explicit attribute
        /// </summary>
        /// <param name="ce"></param>
        /// <param name="part"></param>
        /// <returns></returns>
	    public static EnvDTE.TextPoint GetSafeStartPoint(this CodeElement ce, vsCMPart part = vsCMPart.vsCMPartWholeWithAttributes)
	    {
            try
            {
                return ce.GetStartPoint(part);

            }
            catch (NotImplementedException) {
                //Catch random notimplementedException
                return ce.GetStartPoint();
            }
	    }

	    public static string GetDocComment(this CodeElement ce)
	    {
	        var commentStart = ce.GetCommentStartPoint();
	        if (commentStart == null)
	        {
	            return "";
	        }

            var comment= commentStart.GetText(ce.GetStartPoint());
            return comment;
	    }
		public static EditPoint GetCommentStartPoint(this CodeElement ce)
		{
            try {
                return ce.GetSafeStartPoint().GetCommentStartPoint();

            }
            catch (NotImplementedException) {
                //Catch random notimplementedException
                return null;
            }
		}
		public static EditPoint GetCommentStartPoint(this CodeProperty2 ce)
		{
		  
		    return ce.AsCodeElement().GetCommentStartPoint();
		}

		/// <summary>
		/// Get to the beginning of doc comments for startPoint
		/// </summary>
		/// <param name="startPoint"></param>
		/// <returns></returns>
		/// <remarks>
		/// EnvDte does not have a way to get to the starting point of a code element doc comment. 
		/// If we need to insert some text before a code element that has doc comments we need to go to the beggining of the comments.
		/// </remarks>
		public static EditPoint GetCommentStartPoint(this EnvDTE.TextPoint startPoint)
		{

			var sp = startPoint.CreateEditPoint();
			//keep going 1 line up until the line does not start with doc comment prefix
			do
			{
				sp.LineUp();
            } while (DocCommentRegex.IsMatch(sp.GetLineText()));
			//Go to the beginning of first line of comment, or element itself
			sp.LineDown();
			sp.StartOfLine();
			return sp;
		}

        public static EditPoint GetPositionBeforeClosingBrace(this CodeFunction cf) {
            const string closingBrace = "}";
            var sp = cf.EndPoint.CreateEditPoint();
            
            //keep going 1 char left until we found the } char
            do {
                sp.CharLeft();
            } while (sp.GetText(1) != closingBrace);
            //Go left one more char
            sp.CharLeft();
            return sp;
        }

		public static string ToStringFormatted(this XElement xml)
		{
			var settings = new XmlWriterSettings {OmitXmlDeclaration = true};

		    var result = new StringBuilder();
			using (var writer = XmlWriter.Create(result, settings))
			{

				xml.WriteTo(writer);
			}
			return result.ToString();
		}

#region ExprToString. Convert Member Expression to string.

		public static string[] ExprsToString<T>(params Expression<Func<T, object>>[] exprs)
		{

			var strings = (
			    from x in exprs
			    select ((LambdaExpression)x).ExprToString()).ToArray();
			return strings;
		}

		public static string ExprToString<T, T2>(this Expression<Func<T, T2>> expr)
		{
			return ((LambdaExpression)expr).ExprToString();
		}

		public static string ExprToString(this LambdaExpression memberExpr)
		{
			if (memberExpr == null)
			{
				return "";
			}
		    //when T2 is object, the expression will be wrapped in UnaryExpression of Convert{}
			var convertedToObject = memberExpr.Body as UnaryExpression;
			var currExpr = convertedToObject != null ? convertedToObject.Operand : memberExpr.Body;
			switch (currExpr.NodeType)
			{
				case ExpressionType.MemberAccess:
					var ex = (MemberExpression)currExpr;
					return ex.Member.Name;
			}

			throw new Exception("Expression ToString() extension only processes MemberExpression");
		}

#endregion

		public static List<Type> FindAllDerivedTypes<T>()
		{
			return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
		}

		public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
		{
			var derivedType = typeof(T);
			return assembly.GetTypes().Where(x => x != derivedType && derivedType.IsAssignableFrom(x)).ToList();

		}

		public static IEnumerable<CodeClass2> GetSubclasses(this CodeClass2 cc)
		{
			var fullname = cc.FullName;
			var list = new List<CodeClass2>();
			Kodeo.Reegenerator.Wrappers.CodeElement.TraverseSolutionForCodeElements<CodeClass2>(
                cc.DTE.Solution, list.Add, x => x.FullName != fullname && x.IsDerivedFrom[fullname]);
			return list.ToArray();
		}

		public static string RemoveEmptyLines(this string s)
		{
//INSTANT C# NOTE: VB local static variable moved to class level:
//			Static regex As Regex = new Regex("^\\s+$[\\r\\n]*", RegexOptions.Multiline)
			return RemoveEmptyLines_regex.Replace(s, "");
		}


		public static TResult SelectOrDefault<T, TResult>(this T obj, Func<T, TResult> selectFunc, TResult defaultValue = null) where T :class where TResult : class
		{
		    return obj == null ? defaultValue : selectFunc(obj);
		}

	    /// <summary>
		/// Returns a type from an assembly reference by ProjectItem.Project. Cached.
		/// </summary>
		/// <param name="pi"></param>
		/// <param name="typeName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static Type GetTypeFromProject(this EnvDTE.ProjectItem pi, string typeName)
		{

			var path = pi.GetAssemblyPath();
			if (!(GetTypeFromProject_cache.ContainsKey(path)))
			{
				GetTypeFromProject_cache.Add(path, Assembly.LoadFrom(path));
			}

			var asm = GetTypeFromProject_cache[path];
			var type= asm.GetType(typeName);
	        if (type == null)
	        {
	            throw new Exception(
	                string.Format("Type {0} not found in assembly {1} at {2}. You may need to rebuild the assembly.",
	                    typeName, asm.FullName, path));
	        }
            return type;
		}
		public static Type ToType(this CodeClass cc)
		{
			return cc.ProjectItem.GetTypeFromProject(cc.FullName);
		}

		/// <summary>
		/// Convert CodeProperty2 to PropertyInfo. Cached
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static PropertyInfo ToPropertyInfo(this CodeProperty2 prop)
		{
		    var t = prop.Parent.ToType();
            var classType = TypeResolver.ByType(t );
			return classType.TypeInfo.GetProperty(prop.Name, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

	    public static bool HasAttribute<T>(this MemberInfo mi) where T : Attribute
	    {
	        return mi.GetCustomAttributes<T>().Any();
	    }

		public static GeneratorOptionAttribute GetGeneratorAttribute(this MemberInfo mi)
		{

            var genAttr = mi.GetCustomAttributes().FirstOrDefault(x => x.GetType().IsSubclassOf(GetGeneratorAttribute_type));
            return (GeneratorOptionAttribute)genAttr;
		}

		public static Assembly GetAssemblyOfProjectItem(this EnvDTE.ProjectItem pi)
		{
			var path = pi.GetAssemblyPath();

			if (!string.IsNullOrEmpty(path))
			{
				return Assembly.LoadFrom(path);
			}
		    return null;
		}

		public static string GetAssemblyPath(this EnvDTE.ProjectItem pi)
		{

			//var assemblyName = pi.ContainingProject.Properties.Cast<Property>().FirstOrDefault(x => x.Name == "AssemblyName").SelectOrDefault(x => x.Value);

            return pi.ContainingProject.GetAssemblyPath();
		}

		/// <summary>
		/// Currently unused. If we require a succesful build, a project that requires succesful generation would never build, catch-22
		/// </summary>
		/// <param name="vsProject"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static string GetAssemblyPath(this Project vsProject)
		{
			var fullPath = vsProject.Properties.Item("FullPath").Value.ToString();
			var outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
			var outputDir = Path.Combine(fullPath, outputPath);
			var outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();
			var assemblyPath = Path.Combine(outputDir, outputFileName);
			return assemblyPath;
		}

        const string TypeWithoutQualifierPattern = @"(?<=\.?)[^\.]+?$";
		private static readonly Regex TypeWithoutQualifierRegex = new Regex(TypeWithoutQualifierPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
		public static string StripQualifier(this string s)
		{
			var stripped = TypeWithoutQualifierRegex.Match(s).Value;
			return stripped;
		}

		public static T ParseAsEnum<T>(this string qualifiedName, T defaultValue) where T: struct
		{
			if (string.IsNullOrEmpty(qualifiedName))
			{
				return defaultValue;
			}
			T res;
			Enum.TryParse(qualifiedName.StripQualifier(),out res);
			return res;
		}

		public static T GetOrInit<T>(ref T x, Func<T> initFunc) where T : class
		{
		    return x ?? (x = initFunc());
		}

	    /// <summary>
		/// Create type instance from string
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public static object ConvertFromString(this Type type, string value)
		{
			return TypeDescriptor.GetConverter(type).ConvertFromString(value);
		}

		/// <summary>
		/// Set property value from string representation
		/// </summary>
		/// <param name="propInfo"></param>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		/// <remarks></remarks>
		public static void SetValueFromString(this PropertyInfo propInfo, object obj, string value)
		{
		    var setValue = propInfo.PropertyType == typeof(Version) ? 
                                Version.Parse(value) : 
                                propInfo.PropertyType.ConvertFromString(value);
		    propInfo.SetValue(obj, setValue);
		}

	    public static void AddInterfaceIfNotExists(this CodeClass2 cls, string interfaceName)
		{
		    try
		    {
		      
		        if (!(cls.ImplementedInterfaces.OfType<CodeInterface>().Any(x => x.FullName == interfaceName)))
		        {
		            cls.AddImplementedInterface(interfaceName);
		        }
		    }
		    catch (Exception e)
		    {
                MessageBox.Show("The added interface has to exists in the project." + e);

		    }
		}

	    public static string GetDefaultNamespace(this EnvDTE.ProjectItem item)
	    {
	        return item.ContainingProject.Properties.Item("DefaultNamespace").Value.ToString();
	    }

		public static string DotJoin(this string s, params string[] segments)
		{
			var all = new[] {s}.Concat(segments).ToArray();
			return string.Join(".", all);
		}


		/// <summary>
		/// Returns CodeTypeRef.AsFullName, if null, returns CodeTypeRef.AsString
		/// </summary>
		/// <param name="ctr"></param>
		/// <returns></returns>
		/// <remarks>
		/// If there's compile error AsFullName will be null
		/// </remarks>
		public static string SafeFullName(this CodeTypeRef ctr)
		{
			return (ctr.AsFullName ?? ctr.AsString);
		}

	   static public bool AllPropertiesEquals<T>(this T obj1, T obj2)
	   {
	       var typeCache = TypeResolver.ByType(typeof (T));
	       var props = typeCache.GetProperties();
	       return props.Any(p => !p.GetValue(obj1).Equals(p.GetValue(obj2)));
	   }
     
	}

}