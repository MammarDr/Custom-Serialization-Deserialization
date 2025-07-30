
using CustomAttribute;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomUtility
{
    internal class MyUtility
    {

        public static string EscapeSpecialCharacters(string value)
        {
            if (value == null) return "null";

            StringBuilder sb = new StringBuilder(); 

            foreach (char c in value)
            {
                switch (c)
                {
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    default:   sb.Append(c);     break;

                }
            }
            return sb.ToString();
        }

        static public bool IsAnonymousOrNestedObjectType(object? value)
        {
            if(value == null) return false;

            Type type = value.GetType();

            // type == typeof(ArrayObject)
            if ( IsAnonymousType(value)) return true;

            if (type == typeof(object)) return true;

            return false;   
        }
        static public bool IsAnonymousType(object? value)
        {
            if (value == null) return false;

            Type type = value.GetType();

            return Attribute.IsDefined(type, typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute))
                   && type.IsGenericType
                   && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && type.Namespace == null;
        }

        static public bool IsEnum(object? value) => value != null ? value.GetType().IsEnum : false;
        

        static public bool IsNullableType(object? value)
        {
            if(value == null) return true;
            return Nullable.GetUnderlyingType(value.GetType()) != null;
        }
        
        static public bool IsDefaultOrEmptyValue(object? value)
        {
            if (value == null) return true;

            Type type = value.GetType();

            if (type == typeof(string)) return string.IsNullOrEmpty(value as string);

            if (Nullable.GetUnderlyingType(type) != null)
                return value.Equals(Activator.CreateInstance(type));

            return false;
        }

        static public bool IsIndexed(object? value) => (value is IEnumerable) && value is not string; 

        static public bool IsRegexSafeType(object? value) => IsSimpleType(value);

        static public bool IsStruct(Type type) => type.IsValueType && !type.IsPrimitive && !type.IsEnum;

        static public bool IsNumericalType(object value)
        {

            Type underlying = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

            return underlying == typeof(byte)  || underlying == typeof(sbyte)  ||
                   underlying == typeof(short) || underlying == typeof(ushort) ||
                   underlying == typeof(int)   || underlying == typeof(uint)   ||
                   underlying == typeof(long)  || underlying == typeof(ulong)  ||
                   underlying == typeof(float) || underlying == typeof(double) ||
                   underlying == typeof(decimal);

        }
        static public bool IsSimpleType(object? value)
        {
            Type? type = value?.GetType();
            
            if (type == null) return false;
           
            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        static public bool? GetRegexResult(object? Value, string Pattern)
        {
            if (!IsRegexSafeType(Value)) return null;

            if (Value == null) return false;

            string? Val = Value.ToString();

            MatchCollection matches = Regex.Matches(input:   Val,
                                                    pattern: Pattern);

            return matches.Count > 0;
        }

        static public IEnumerable? TestPattern(IEnumerable? collection, string? pattern)
        {
            if (collection is null || pattern is null) return null;

            List<object> result = new List<object>();

            foreach (var item in collection)
            {
                if (item is null)
                    result.Add(item);

                else if (IsIndexed(item))
                {
                    object? obj = item;

                    if (item is IDictionary dect)
                        obj = TestPattern(dect, patterns: null);
                    else
                        obj = TestPattern(item as IEnumerable, pattern);

                    result.Add(obj);
                }
                else if (IsSimpleType(item) && TestPattern(item, pattern) != null)
                    result.Add(item);
            }

            return result;
        }

        static public List<DictionaryEntry>? TestPattern(IDictionary? dictionary, KeyValuePair<string?, string?>? patterns)
        {
            if (dictionary is null || patterns is null) return null;

            List<DictionaryEntry> result = new List<DictionaryEntry>();

            string? keyPattern = null, valuePattern = null;

            if (patterns != null)
            {
                keyPattern = patterns.Value.Key;
                valuePattern = patterns.Value.Value;
            }

            foreach (DictionaryEntry pair in dictionary)
            {
                object? val = null;

                if (keyPattern is not null && TestPattern(pair.Key, keyPattern) == null)
                    continue;

                if (valuePattern is not null)
                {
                    if (IsIndexed(pair.Value))
                    {
                        if (pair.Value is IDictionary dect)
                            val = TestPattern(dect, patterns);
                        else
                            val = TestPattern(pair.Value as IEnumerable, valuePattern);
                    }
                    else if (IsSimpleType(pair.Value?.GetType()) && TestPattern(pair.Value, valuePattern) != null)
                        val = pair.Value;
                }

                result.Add(new DictionaryEntry(pair.Key, val));
            }

            return result;
        }

        static public object? TestPattern(object? value, string? pattern)
        {

            if (value is null || string.IsNullOrWhiteSpace(pattern)) return null;

            if (IsIndexed(value))
            {
                if (value is IDictionary dect)
                    return TestPattern(dect, patterns: null);
                else
                    return TestPattern(value as IEnumerable, pattern);
            }

            return GetRegexResult(value, pattern) == true ? value : null;
        }
    }
}
