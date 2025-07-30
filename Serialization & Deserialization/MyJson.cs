using CustomAttribute;
using CustomUtility;
using CustomJSONSerializer;
using System.Collections;
using System.Reflection;



namespace CustomJSON
{
    public enum JSONIgnoreTargets
    {
        Property = 0x0001,
        Key = 0x0002,
        Value = 0x0004,
        WhiteSpace = 0x0008,
        NewLine = 0x0010,
        Bracket = 0x0020,
        Quote = 0x0040,
        StringEnum = 0x0080,
    }

    static public class MyJSON
    {
        static private char[] ObjectBrackets    = ['{', '}'];
        static private char[] ArraysBrackets    = ['[', ']'];
        static private char[] ValueBrackets     = ['"', '"'];
        static private char   KeyValueSeperator = ':';
        static private char   ObjectSeperator   = ',';


        static public string OpenObjectBracket(JSONIgnoreTargets Validation = 0x0000, string WhiteSpace = "")
        {
            bool IgnoreNewLine = (Validation & JSONIgnoreTargets.NewLine) != 0;

            return GetWhiteSpace(Validation, WhiteSpace) + ObjectBrackets[0] + (IgnoreNewLine ? "" : "\n");

        }
        static public string CloseObjectBracket(JSONIgnoreTargets Validation = 0x0000, string WhiteSpace = "")
        {
            bool IgnoreNewLine = (Validation & JSONIgnoreTargets.NewLine) != 0;

            return (IgnoreNewLine ? "" : "\n") + GetWhiteSpace(Validation, WhiteSpace) + ObjectBrackets[1];
        }

        static public string GetWhiteSpace(JSONIgnoreTargets Validation, string WhiteSpace = "  ")
        {
            return (Validation & JSONIgnoreTargets.WhiteSpace) != 0 ? "" : WhiteSpace;
        }

        static private string FormatKey(string Key, JSONIgnoreTargets Validation = 0x0000) 
        {
            if ((Validation & JSONIgnoreTargets.Key) != 0) return "";

            if ((Validation & JSONIgnoreTargets.Quote) != 0) return Key;

            return $"\"{Key ?? "null"}\"";
        }
        
        static public string GetSeperator(JSONIgnoreTargets validation)
        {
            if ((validation & JSONIgnoreTargets.NewLine) == 0) return $",\n";

            return ",";
        }

        static public string GetValueFormat(JSONIgnoreTargets Validation, string? Key, object? Value, string WhiteSpace = "  ")
        {
            if (Value == null) return "null";

            if (Value is string txt) return (Validation & JSONIgnoreTargets.Quote) != 0 ? txt : $"\"{MyUtility.EscapeSpecialCharacters(txt)}\"";

            if (Value is DateTime time) return (Validation & JSONIgnoreTargets.Quote) != 0 ? $"{time}" : $"\"{time}\"";

            if (Value is bool boolean) return boolean.ToString().ToLower();

            if (Value is IDictionary dict)
            {
                var values = new List<string>();

                foreach (DictionaryEntry kv in dict)
                {
                    string k = GetValueFormat(Validation | JSONIgnoreTargets.Quote, null, kv.Key);

                    string v = GetValueFormat(Validation, null, kv.Value, WhiteSpace + "  ");

                    string s = (Validation & JSONIgnoreTargets.Key) != 0 ? "" : $"{KeyValueSeperator}";

                    values.Add(GetWhiteSpace(Validation, WhiteSpace + "  ") + FormatKey(k, Validation) + s + v);
                }

                return OpenObjectBracket(Validation)                 + 
                       string.Join(GetSeperator(Validation), values) +
                       CloseObjectBracket(Validation, WhiteSpace);
            }

            if (Value is IEnumerable collection)
            {
                var values = new List<string?>();

                foreach (var item in collection)
                {
                    values.Add(GetValueFormat(Validation, null, item, WhiteSpace));
                }

                return "[" + string.Join(",", values) + "]";
            }

            if (MyUtility.IsSimpleType(Value) || MyUtility.IsNullableType(Value))
            {
                return MyUtility.IsNumericalType(Value) ? $"{Value}" :
                       (Validation & JSONIgnoreTargets.Quote) != 0 ? $"{Value}" : $"\"{Value}\"";
            }

            if (MyUtility.IsEnum(Value))
            {
                if ((Validation & JSONIgnoreTargets.StringEnum) == 0) return $"\"{Value}\"";

                object numericValue = Convert.ChangeType(Value, Enum.GetUnderlyingType(Value.GetType()));
                return $"{numericValue ?? $"\"{Value}\""}";
            }


            return OpenObjectBracket(Validation) + GetAnonymousObjectFormat(Validation | JSONIgnoreTargets.Property | JSONIgnoreTargets.Bracket, Value, WhiteSpace + "  ") + CloseObjectBracket(Validation, WhiteSpace);

        }

        static private string GetAnonymousObjectFormat(JSONIgnoreTargets Validation, object Value, string WhiteSpace = "  ")
        {
            try
            {
                MyJSONSerializer serializer = MyJSONSerializer.CreateInstance(Value.GetType());
                return CreateKeyObjectFormat(serializer.AppendMembersToList(), null, Value, Validation , WhiteSpace);
            }
            catch (Exception ex)
            {
                return "Serialization Failed : {__System_Error__}";
            }
        }
        static public string CreateKeyValueFormat(string? Key, object? value, JSONIgnoreTargets Validation = 0x0000, string WhiteSpace = "  ")
        {
            bool IncludeKey = (Validation & JSONIgnoreTargets.Key) == 0 && Key != null;

            if (!IncludeKey) return GetValueFormat(Validation, Key, value, WhiteSpace);

            string KeyPart = GetWhiteSpace(Validation, WhiteSpace) + FormatKey(Key);

            return KeyPart + KeyValueSeperator + GetValueFormat(Validation, Key, value, WhiteSpace);
        }

        static public string CreateTypeNameFormat(string key, Type type, string WhiteSpace = "")
        {
            return CreateKeyValueFormat(key, type.Name, WhiteSpace);
        }

        static public string CreateKeyValueFormat(string key, object value, string WhiteSpace = "")
        {
            return WhiteSpace + $"\"{key}\"" + KeyValueSeperator + $"\"{value}\"";
        }

        static public string CreateKeyObjectFormat(MemberInfo[] Members, string? Property, object Origin, JSONIgnoreTargets Validation = 0x0000, string WhiteSpace = "  ")
        {
            ArgumentNullException.ThrowIfNull(nameof(Members));

            ArgumentNullException.ThrowIfNull(nameof(Origin));  

            bool IncludeKey = ((Validation & JSONIgnoreTargets.Property) == 0) && Property != null;

            bool IncludeBracket = (Validation & JSONIgnoreTargets.Bracket) == 0;

            List<string> data = new List<string>();

            foreach (MemberInfo member in Members)
            {

                if (member == null) continue;

                string k = "underfined";

                if (!member.IsDefined(typeof(MyNickNameAttribute)))
                    k = member.Name;
                else
                    k = MyAttributes.GetNickName(member) ?? member.Name;

                
                object? v = member is PropertyInfo valProp
                                                        ? valProp.GetValue(Origin) :
                                   member is FieldInfo valField
                                                        ? valField.GetValue(Origin) : null;

                Type? memberType = member is PropertyInfo tProp
                                                        ? tProp.PropertyType :
                                   member is FieldInfo tField
                                                        ? tField.FieldType : null;
                

                if (v is not null && member.IsDefined(typeof(MyPatternAttribute)))
                {
                    if (MyUtility.IsIndexed(v))
                    {
                        if (v is IDictionary dict)
                            v = MyUtility.TestPattern(dict, MyAttributes.GetKeyValuePattern(member)) ?? v;
                        else
                            v = MyUtility.TestPattern(v as IEnumerable, MyAttributes.GetPattern(member)) ?? v;
                    }
                    else
                    {
                        v = MyUtility.TestPattern(v, MyAttributes.GetPattern(member));
                    }
                }


                // value & primitive type members must be Nullable so they can be trackable.
                bool isInitiated = !MyUtility.IsDefaultOrEmptyValue(v);

                if (!isInitiated && member.IsDefined(typeof(MyDefaultValueAttribute)))
                    (v, isInitiated) = (MyAttributes.GetDefaultValue(member), true);


                if (!isInitiated && member.IsDefined(typeof(MyRequiredAttribute)))
                    throw new ArgumentException("Required Member is not initiated");

                data.Add(CreateKeyValueFormat(k, v, Validation, WhiteSpace + "  "));
            }


            string format = string.Join(GetSeperator(Validation), data) ?? "";

            if (IncludeKey)
                Property = GetWhiteSpace(Validation) + FormatKey(Property);

            if(IncludeBracket)
                return  (IncludeKey ? Property + KeyValueSeperator : "") + OpenObjectBracket(Validation) + format + CloseObjectBracket(Validation, WhiteSpace);

            return (IncludeKey ? Property + KeyValueSeperator + format : format);

        }
    }

}
