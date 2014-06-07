//Formerly VB project-level imports:

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EnvDTE80;
using Debug = RgenLib.Extensions.Debug;

namespace RgenLib {
    public class TypeCacheList {
        private readonly Dictionary<string, TypeCache> ByNameCache = new Dictionary<string, TypeCache>();
        private readonly Dictionary<Type, TypeCache> ByTypeCache = new Dictionary<Type, TypeCache>();

        public bool Contains(Type type) {
            return ByTypeCache.ContainsKey(type);
        }
        public TypeCache ByName(CodeClass2 cc) {

            if (!(ByNameCache.ContainsKey(cc.FullName))) {
                var tc = new TypeCache(cc.FullName);
                ByNameCache.Add(cc.FullName, tc);
                ByTypeCache.Add(tc.TypeInfo.AsType(), tc);
            }

            return ByNameCache[cc.FullName];
        }

        public TypeCache ByType(Type type) {
            if (!(ByTypeCache.ContainsKey(type))) {
                var tc = new TypeCache(type);
                ByNameCache.Add(tc.TypeInfo.Name, tc);
                ByTypeCache.Add(tc.TypeInfo.AsType(), tc);
            }
            return ByTypeCache[type];
        }

    }

    public class TypeCache {

        private readonly Dictionary<string, MemberInfo> _publicInstanceMembers;
        private readonly Dictionary<string, PropertyInfo> _publicInstanceProperties;
        public TypeInfo TypeInfo { get; set; }

        public TypeCache(string typeName, bool caseSensitiveMembers = false)
            : this(Type.GetType(typeName)) {

        }

        void AddOrReplaceMemberWithQualifiedName(MemberInfo m)
        {
            var dict = _publicInstanceMembers;
            System.Diagnostics.Debug.Assert(m.DeclaringType != null, "m.DeclaringType != null");
            if (dict.ContainsKey(m.Name))
            {
                dict.Remove(m.Name);
            }
            dict.Add(m.DeclaringType.Name + "." + m.Name, m);
        }


        public TypeCache(Type type, bool caseSensitiveMembers = true) {
            var comparer = caseSensitiveMembers ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
            _publicInstanceMembers = new Dictionary<string, MemberInfo>(comparer);
            _publicInstanceProperties = new Dictionary<string, PropertyInfo>();
            try {
                TypeInfo = type.GetTypeInfo();
                foreach (var m in TypeInfo.GetMembers(BindingFlags.Instance | BindingFlags.Public )) {
                    //Handle member with the same name (overridden members)
                    MemberInfo existingMemberWithSameName;
                    if (_publicInstanceMembers.TryGetValue(m.Name, out existingMemberWithSameName))
                    {
                        System.Diagnostics.Debug.Assert(existingMemberWithSameName.DeclaringType != null, "existingMemberWithSameName.DeclaringType != null");
                        System.Diagnostics.Debug.Assert(m.DeclaringType != null, "m.DeclaringType != null");
                        if (m.DeclaringType.IsSubclassOf(existingMemberWithSameName.DeclaringType))
                        {
                            //the overriding member takes precedent 
                            //add declaring typename to existing
                            AddOrReplaceMemberWithQualifiedName(existingMemberWithSameName);
                        }
                        else
                        {
                            //keep the existing member
                            AddOrReplaceMemberWithQualifiedName(m);
                            continue;
                        }
                    }

                    _publicInstanceMembers.Add(m.Name, m);
                    var info = m as PropertyInfo;
                    if (info != null) { _publicInstanceProperties.Add(info.Name, info); }
                }
            }
            catch (Exception ex) {
                Extensions.Debug.DebugHere();
            }
        }

        public IEnumerable<PropertyInfo> GetProperties()
        {
            return _publicInstanceProperties.Values;
        }

        public MemberInfo this[string name] {
            get {
                if (!(_publicInstanceMembers.ContainsKey(name))) {
                    throw new Exception(string.Format("Member {0} not found in {1}", name, TypeInfo.Name));
                }
                return _publicInstanceMembers[name];
            }
        }
        public IEnumerable<MemberInfo> GetMembers() {
            return _publicInstanceMembers.Values;
        }

        public MemberInfo GetMember(string name) {
            return _publicInstanceMembers[name];

        }

        public bool Contains(string name) {
            return _publicInstanceMembers.ContainsKey(name);
        }

        public MemberInfo TryGetMember(string key) {
            MemberInfo value = null;
            _publicInstanceMembers.TryGetValue(key, out value);
            return value;
        }

        public void AddAlias(string name, string alternateName) {
            try {
                _publicInstanceMembers.Add(alternateName, this[name]);

            }
            catch (Exception) {
                Debugger.Launch();
            }
        }
    }

    public class TypeResolver {
        public static TypeCacheList TypeCacheList = new TypeCacheList();
        public static TypeCache ByType(Type type) {
            return TypeCacheList.ByType(type);
        }
        public static bool Contains(Type type) {
            return TypeCacheList.Contains(type);
        }
        public static TypeCache ByName(CodeClass2 cc) {
            return TypeCacheList.ByName(cc);
        }

    }

}