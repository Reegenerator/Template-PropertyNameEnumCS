//Formerly VB project-level imports:

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using EnvDTE80;
using Debug = ReegeneratorCollection.Extensions.Debug;

namespace ReegeneratorCollection
{
	public class TypeCacheList
	{
		private readonly Dictionary<string, TypeCache> ByNameCache = new Dictionary<string, TypeCache>();
		private readonly Dictionary<Type, TypeCache> ByTypeCache = new Dictionary<Type, TypeCache>();

		public bool Contains(Type type)
		{
			return ByTypeCache.ContainsKey(type);
		}
		public TypeCache ByName(CodeClass2 cc)
		{

			if (!(ByNameCache.ContainsKey(cc.FullName)))
			{
				var tc = new TypeCache(cc.FullName);
				ByNameCache.Add(cc.FullName, tc);
				ByTypeCache.Add(tc.TypeInfo.AsType(), tc);
			}

			return ByNameCache[cc.FullName];
		}

		public TypeCache ByType(Type type)
		{
			if (!(ByTypeCache.ContainsKey(type)))
			{
				var tc = new TypeCache(type);
				ByNameCache.Add(tc.TypeInfo.Name, tc);
				ByTypeCache.Add(tc.TypeInfo.AsType(), tc);
			}
			return ByTypeCache[type];
		}

	}

	public class TypeCache
	{

		private readonly Dictionary<string, MemberInfo> Cache;
		public TypeInfo TypeInfo {get; set;}

		public TypeCache(string typeName, bool caseSensitiveMembers = false) : this(Type.GetType(typeName))
		{

		}


		public TypeCache(Type type, bool caseSensitiveMembers = false)
		{
			var comparer = caseSensitiveMembers ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
			Cache = new Dictionary<string, MemberInfo>(comparer);

			try
			{
				TypeInfo = type.GetTypeInfo();
				foreach (var m in TypeInfo.GetMembers(BindingFlags.Instance | BindingFlags.Public))
				{
					//Prevent error on multiple cctor
					if (!(Cache.ContainsKey(m.Name)))
					{
						Cache.Add(m.Name, m);
					}

				}

			}
			catch (Exception)
			{
				Debug.DebugHere();
			}
		}
		public MemberInfo this[string name]
		{
			get
			{
				if (!(Cache.ContainsKey(name)))
				{
					throw new Exception(string.Format("Member {0} not found in {1}", name, TypeInfo.Name));
				}
				return Cache[name];
			}
		}
		public IEnumerable<MemberInfo> GetMembers()
		{
			return Cache.Values;
		}

		public MemberInfo GetMember(string name)
		{
			return Cache[name];

		}

		public bool Contains(string name)
		{
			return Cache.ContainsKey(name);
		}

		public MemberInfo TryGetMember(string key)
		{
			MemberInfo value = null;
			Cache.TryGetValue(key, out value);
			return value;
		}

		public void AddAlias(string name, string alternateName)
		{
			try
			{
				Cache.Add(alternateName, this[name]);

			}
			catch (Exception)
			{
				Debugger.Launch();
			}
		}
	}

	public class TypeResolver
	{
		public static TypeCacheList TypeCacheList = new TypeCacheList();
		public static TypeCache ByType(Type type)
		{
			return TypeCacheList.ByType(type);
		}
		public static bool Contains(Type type)
		{
			return TypeCacheList.Contains(type);
		}
		public static TypeCache ByName(CodeClass2 cc)
		{
			return TypeCacheList.ByName(cc);
		}

	}

}