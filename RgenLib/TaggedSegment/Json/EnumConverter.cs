using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RgenLib.TaggedSegment.Json {

    /// <summary>
    /// An enum converter that writes the value without quotes
    /// </summary>
    /// <remarks>
    /// This is a hacky workaround, by overriding StringEnumConverter and replace WriteValue with WriteRawValue
    /// </remarks>
    class EnumConverter : StringEnumConverter
    {
        private static object EnumMemberNamesPerType;
        private static MethodInfo EnumMemberNamesPerType_Get;
        private static MethodInfo Map_TryGetByFirst;
        private static MethodInfo StringUtils_ToCamelCase;
        static EnumConverter()
        {
            TypeCache typeCache = TypeResolver.ByType<StringEnumConverter>();
            EnumMemberNamesPerType = ((FieldInfo)
                                        typeCache.TypeInfo
                                        .GetMember("EnumMemberNamesPerType", BindingFlags.NonPublic | BindingFlags.Static)
                                        .Single()
                                            ).GetValue(null);

            EnumMemberNamesPerType_Get=(MethodInfo) EnumMemberNamesPerType.GetType().GetMember("Get").Single();
            Map_TryGetByFirst = (MethodInfo) EnumMemberNamesPerType_Get.ReturnType.GetMember("TryGetByFirst").Single();
            // ReSharper disable once PossibleNullReferenceException
            StringUtils_ToCamelCase = (MethodInfo) typeof(StringEnumConverter).Assembly
                                        .GetType("Newtonsoft.Json.Utilities.StringUtils")
                                        .GetMember("ToCamelCase").Single();
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            if (value == null) {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;

            string enumName = e.ToString("G");

            if (char.IsNumber(enumName[0]) || enumName[0] == '-') {
                // enum value has no name so write number
                writer.WriteValue(value);
            }
            else {
                var map = EnumMemberNamesPerType_Get.Invoke(EnumMemberNamesPerType,new object[] {e.GetType()});

                string[] names = enumName.Split(',');
                for (int i = 0; i < names.Length; i++) {
                    string name = names[i].Trim();

                    var parameters =new object[] {name, null};
                    Map_TryGetByFirst.Invoke(map, parameters);
                    //get out parameter
                    string resolvedEnumName =(string) parameters[1];

                    resolvedEnumName = resolvedEnumName ?? name;

                    if (CamelCaseText)
                        resolvedEnumName =(string) StringUtils_ToCamelCase.Invoke(null, new object[]{resolvedEnumName});

                    names[i] = resolvedEnumName;
                }

                string finalName = string.Join(", ", names);
                //this is the only change we need. Using WriteRawValue instead of WriteValue. To remove quotes from value
                writer.WriteRawValue(finalName);
            }
        }

    }
}
