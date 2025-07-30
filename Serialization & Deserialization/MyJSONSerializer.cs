using CustomAttribute;
using CustomJSON;
using CustomSerializer;
using System.Reflection;
using System.Text;


namespace CustomJSONSerializer
{
    /// <summary>
    /// Serializes objects into a custom JSON-like format.
    /// </summary>
    public class MyJSONSerializer : MySerializer
    {

        private MyJSONSerializer(Type type) : base(type) { }


        /// <summary>
        /// Creates a serializer instance for the specified type, unless it is marked as non-serializable.
        /// </summary>
        /// <param name="type">The type to be serialized.</param>
        /// <returns>
        /// A <see cref="MyJSONSerializer"/> instance if the type is not null and not marked with <c>[MyNonSerializable]</c>;
        /// otherwise, <c>null</c>.
        /// </returns>
        static public MyJSONSerializer CreateInstance(Type type)
        {
            if (type == null || type.IsDefined(typeof(MyNonSerializableAttribute), inherit: false))
                return null;

            return new MyJSONSerializer(type);
        }

        public override bool Serialize(TextWriter Writer, object Origin)
        {
            return Serialize(Writer, Origin, 0x0000, false);
        }

        public bool Serialize(TextWriter Writer, object Origin, bool OnlyValue)
        {
            return Serialize(Writer, Origin, 0x0000, OnlyValue);
        }

        public bool Serialize(TextWriter Writer, object Origin, JSONIgnoreTargets Validation)
        {
            return Serialize(Writer, Origin, Validation, false);
        }


        public bool Serialize(TextWriter Writer, object Origin, JSONIgnoreTargets Validation, bool OnlyValue)
        {
            ArgumentNullException.ThrowIfNull(Writer);
            ArgumentNullException.ThrowIfNull(Origin);

            StringBuilder serializedTXT = new StringBuilder();

            List<string> data = new List<string>();

            try
            {
                MemberInfo[] membersList = AppendMembersToList();


                if(!OnlyValue)
                {
                    serializedTXT.Append(MyJSON.OpenObjectBracket(Validation));

                    data.Add(MyJSON.CreateTypeNameFormat("Type", _type, MyJSON.GetWhiteSpace(Validation)));

                    data.Add(MyJSON.CreateKeyObjectFormat(membersList, "Data", Origin, Validation));

                    serializedTXT.Append(string.Join(MyJSON.GetSeperator(Validation), data));

                    serializedTXT.Append(MyJSON.CloseObjectBracket(Validation));

                } else
                {
                    serializedTXT.Append(MyJSON.CreateKeyObjectFormat(membersList, null, Origin, Validation, ""));
                }

                
            }
            catch (Exception)
            {
                return false;
            }

            Writer.Write(serializedTXT);

            return true;

        }

        public object Deserializer(TextReader reader)
        {
            bool isValid = true;
            char ch;

            StringBuilder Brackets = new StringBuilder();
            StringBuilder Key = new StringBuilder();
            StringBuilder Value = new StringBuilder();

            string content = reader.ReadToEnd();
            //MyJSON.FormatToList(content);

            // List<string>



            return isValid ? new object() : null;
        }

    }
}
