using System;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.CodeDom;
using System.Diagnostics;
using Microsoft.SqlServer.Server;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;


namespace MySerilization
{
    static public class MyUtility
    {
        static public bool IsIndexed(object value) { return (value is IEnumerable) && !(value is string); }

        static public bool IsRegexSafeType(Type type)
        {
            return IsSimpleType(type);
        }
        static public bool IsSimpleType(Type type)
        {
            if (type == null) return false;

            return type.IsPrimitive ||
                   type == typeof(string) ||
                   type == typeof(decimal) ||
                   type == typeof(DateTime) ||
                   type == typeof(Guid);
        }

        static public bool? GetRegexResult(object Value, string Pattern)
        {
            if (!IsRegexSafeType(Value?.GetType())) return null;

            string Val = Value?.ToString();

            if (Value == null) return false;

            MatchCollection matches = Regex.Matches(Val, Pattern);

            return matches.Count > 0;
        }
    }

    /*
     * My Custom Attributes
     * Required - Default - Order - NickName
     * Serializable - NonSerializable(Ignore)
     */

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MySerializableAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyNonSerializableAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyRequiredAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyDefaultValueAttribute : Attribute
    {
        public object Value { get; }
        public MyDefaultValueAttribute(object value) => Value = value;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyNickNameAttribute : Attribute
    {
        public string Name { get; }

        public MyNickNameAttribute(string Name) => this.Name = Name;

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyOrderAttribute : Attribute
    {
        public int Order { get; }

        public MyOrderAttribute(int Order)
        {
            if (Order < 0) Order = 0;
            else this.Order = Order;
        }

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyTxtTypeAttribute : Attribute
    {
        public string Text { get; }

        public MyTxtTypeAttribute(Type type) => Text = type.FullName;

        public MyTxtTypeAttribute(string text) => Text = text;

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyPatternAttribute : Attribute
    {
        public string Pattern { get; }

        public MyPatternAttribute(string Pattern) => this.Pattern = Pattern;

    }

    static public class MyAttributes
    {

        public class MyOrderWrapper
        {
            public int order { get; set; }
            public MemberInfo member { get; set; }
        }

        static public bool ToIgnore(MemberInfo member)
        {
            return member.IsDefined(typeof(MyNonSerializableAttribute), false);
        }

        static public string GetNickName(MemberInfo member)
        {
            object attr = member?.GetCustomAttribute(typeof(MyNickNameAttribute), false);

            return ((MyNickNameAttribute)attr)?.Name ?? member.Name;
        }

        static public object GetDefaultValue(MemberInfo member)
        {
            // Attribute.GetCustomAttribute(member,typeof(MyDefaultValueAttribute)) as MyDefaultValueAttribute;
            // var attr = member.GetCustomAttribute(typeof(MyDefaultValueAttribute)) as MyDefaultValueAttribute;
            object attr = member?.GetCustomAttribute(typeof(MyDefaultValueAttribute), false);

            return ((MyDefaultValueAttribute)attr)?.Value;
        }

        static public string GetPattern(MemberInfo member)
        {
            object attr = member?.GetCustomAttribute(typeof(MyPatternAttribute), false);

            return ((MyPatternAttribute)attr)?.Pattern;
        }

        static public object TestPattern(IList list, string pattern)
        {
            if (list == null) return null;

            IList<object> StrongList = list.Cast<object>().ToList();

            for (int i = StrongList.Count - 1; i >= 0 ; i--)
            {
                var obj = StrongList[i];

                if (MyUtility.IsIndexed(obj))
                {
                    StrongList[i] = TestPattern((IList)obj, pattern);

                    if (((IList)obj).Count == 0)
                        StrongList.Remove(obj);
                }

                else if (MyUtility.IsSimpleType(obj?.GetType()) && MyAttributes.TestPattern(obj, pattern) == null)
                    StrongList.Remove(obj);

            } 


            return (IList)StrongList;
        }

        static public object TestPattern(object value, string pattern)
        {

            if (value == null) return null;

            if (MyUtility.IsIndexed(value)) return TestPattern((IList)value, pattern);

            bool? result = MyUtility.GetRegexResult(value, pattern);

            return result == true ? value : null;
        }


        static public int? GetOrder(MemberInfo member)
        {
            if (!member.IsDefined((typeof(MyOrderAttribute)))) return null;

            object attr = member.GetCustomAttribute(typeof(MyOrderAttribute), false);

            return ((MyOrderAttribute)attr)?.Order;
        }

    }

    static public class MyJSON
    {
        
      
       static public string FormatValue(object value)
        {
            if (value == null) return "null";

            // object.ReferenceEquals(value.GetType(), typeof(string))
            if (value is string s) return $"\"{s}\""; 

            if (value is bool b) return b.ToString().ToLower();

            if (value is IEnumerable enumerable)
            {
                var values = new List<string>();

                foreach (var item in enumerable)
                {
                    values.Add(FormatValue(item)); 
                }

                return $"[{string.Join(",", values)}]";

            }

            if(MyUtility.IsSimpleType(value.GetType())) return value.ToString();

            return "object";
        }

        static public bool CreateKeyValueFormat(StringBuilder sb, MemberInfo member, object value)
        {
            return member != null && CreateKeyValueFormat(sb, member.Name, value);
        }

        static public bool CreateKeyValueFormat(StringBuilder sb, Type type)
        {
            return type != null && CreateKeyValueFormat(sb, "Type", type.Name);
        }

        static public bool CreateKeyValueFormat(StringBuilder sb, String Key, object Value, string spacing = "  ")
        {
            sb.Append("\n" + spacing + $"\"{Key}\" : {FormatValue(Value)},");
            return true;
        }

        static public bool CreateKeyObjectFormat(StringBuilder sb, String Property, MemberInfo[] members, object obj, string spacing = "  ")
        {
            if (members == null || obj == null) return false;

            sb.Append($"\n{spacing}\"{Property}\" : ");

            OpenWrapper(sb);

            foreach (MemberInfo member in members)
            {

                if (member == null) continue;


                string Key = " ";


                if (!member.IsDefined(typeof(MyNickNameAttribute), false)) Key = member.Name;
                else
                {
                    string name = MyAttributes.GetNickName(member);

                    if (name != null)
                        Key = name;
                    else
                        Key = member.Name;

                }


                object value = member is PropertyInfo prop ? prop.GetValue(obj) :
                    member is FieldInfo field ? field.GetValue(obj) : null;

                Type memberType = member is PropertyInfo p ? p.PropertyType :
                                    member is FieldInfo f ? f.FieldType : null;

                if (value != null && memberType == typeof(object))
                {
                    MyJSONSerializer serializer = MyJSONSerializer.CreateInstance(value.GetType());
                    CreateKeyObjectFormat(sb, Key, serializer.AppendMembersToList(), value, $"{spacing}  ");
                    continue;
                }
 
                bool IsRequired = member.IsDefined(typeof(MyRequiredAttribute), false);

                // Attribute.IsDefined(member, typeof(MyDefaultValueAttribute))
                if (value == null && member.IsDefined(typeof(MyDefaultValueAttribute), false))
                    value = MyAttributes.GetDefaultValue(member);
                

                if (value != null && member.IsDefined(typeof(MyPatternAttribute), false))
                {
                    string pattern = MyAttributes.GetPattern(member);

                    if (pattern != null && pattern.Length > 0)
                        value = MyAttributes.TestPattern(value, pattern);
                }

                if (value == null && member.IsDefined(typeof(MyDefaultValueAttribute), false))
                    value = MyAttributes.GetDefaultValue(member);


                if (value == null && IsRequired) return false;

                CreateKeyValueFormat(sb, Key, value, $"{spacing}  ");
            }

            CloseWrapper(sb, spacing);

            return true;
        }

        static public void FinalizeFormat(StringBuilder sb)
        {
            if (sb[sb.Length - 1] == ',') sb[sb.Length - 1] = '\n';
            if (sb[sb.Length - 1] == '}') sb.Append("\n");
        }

        static public void OpenWrapper(StringBuilder sb)
        {
            sb.Append("{");
        }

        static public void CloseWrapper(StringBuilder sb, string spacing = "")
        {
            FinalizeFormat(sb);
            sb.Append(spacing + "},");
        }

        static public bool Deformat(ref string data)
        {
            data = data.Replace("\n", "").Replace(" ", "");

            return true;
        }

        static public List<List<string>> FormatToList(string data)
        {
            Deformat(ref data);

            string[] parts = data.Split(',');

            return new List<List<string>>();
        }
    }

    abstract public class MySerializer
    {

        protected Type _type;
        protected ushort _fields_size;
        protected ushort _property_size;
        protected ushort _method_size;

        protected MySerializer(Type type)
        {
            _type = type;
            _fields_size = (ushort)type.GetFields().Count();
            _property_size = (ushort)type.GetProperties().Count();
            _method_size = (ushort)type.GetMethods().Count();
        }

        public abstract bool Serialize(TextWriter writer, object obj);

        protected void FieldsToList(List<MyAttributes.MyOrderWrapper> list)
        {

            foreach (FieldInfo field in _type.GetFields())
            {
                if (field == null || MyAttributes.ToIgnore(field)) continue;
                list.Add(new MyAttributes.MyOrderWrapper { member = field, order = MyAttributes.GetOrder(field) ?? list.Count });
            }

        }

        protected void PropertiesToList(List<MyAttributes.MyOrderWrapper> list)
        {

            foreach (PropertyInfo prop in _type.GetProperties())
            {
                if (prop == null || MyAttributes.ToIgnore(prop)) continue;
                list.Add(new MyAttributes.MyOrderWrapper { member = prop, order = MyAttributes.GetOrder(prop) ?? list.Count });
            }

        }

        protected void MethodsToList(List<MyAttributes.MyOrderWrapper> list)
        {

            foreach (MethodInfo method in _type.GetMethods())
            {
                if (method == null || MyAttributes.ToIgnore(method)) continue;
                list.Add(new MyAttributes.MyOrderWrapper { member = method, order = MyAttributes.GetOrder(method) ?? list.Count });
            }

        }

        public MemberInfo[] AppendMembersToList()
        {
            MemberInfo[] members = new MemberInfo[_property_size + _fields_size];

            AppendMembersToList(ref members);

            return members;

        }

        public void AppendMembersToList(ref MemberInfo[] members)
        {
            List<MyAttributes.MyOrderWrapper>
                    memberWrapper = new List<MyAttributes.MyOrderWrapper>();


            FieldsToList(memberWrapper);

            PropertiesToList(memberWrapper);

            members = memberWrapper.OrderBy(member => member.order)
                     .Select(member => member.member)
                     .ToArray();
        }
    }

    /*
     * My Custom Serializer
     * JSON Format
     */

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

       

        public override bool Serialize(TextWriter writer, object obj)
        {

            if (obj == null || writer == null) return false;

            StringBuilder serializedTXT = new StringBuilder();

            MemberInfo[] membersList = AppendMembersToList();

            MyJSON.OpenWrapper(serializedTXT);

            if (!MyJSON.CreateKeyValueFormat(serializedTXT, _type))
                return false;

            if (!MyJSON.CreateKeyObjectFormat(serializedTXT, "Data", membersList, obj))
                return false;

            MyJSON.CloseWrapper(serializedTXT);

            MyJSON.FinalizeFormat(serializedTXT);

            writer.Write(serializedTXT);

            Console.WriteLine(serializedTXT.ToString());

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
            MyJSON.FormatToList(content);

           // List<string>



            return isValid ? new object() : null;
        }

    }


    [MySerializable]
    public class Product
    {
        [MyRequired]
        [MyOrder(0)]
        public Guid? ProductId { get; set; }

        [MyDefaultValue("N/A")]
        [MyOrder(2)]
        public string Description { get; set; }

        [MyRequired]
        [MyOrder(1)]
        [MyNickName("Label")]
        public string Name { get; set; }

        [MyPattern("^[A-Z]{2}-\\d{4}$")] // e.g., AB-1234
        [MyOrder(4)]
        public string SKU { get; set; }

        [MyDefaultValue(0.0)]
        [MyOrder(3)]
        public double? Price { get; set; }

        [MyDefaultValue(true)]
        [MyOrder(5)]
        public bool? IsAvailable { get; set; }

        [MyNonSerializable]
        public string InternalNotes { get; set; }

        [MyPattern("^cat")]
        public List<string> Categories { get; set; }

        [MyPattern("^feat")]
        public object[] Features { get; set; }

        public int[] Ratings { get; set; }

        [MyDefaultValue(null)]
        public object Metadata { get; set; }
    }


    public class Program
    {

        static void Main(string[] args)
        {

            MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(Product));

            if (myserializer == null) return;


            Product product1 = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Laptop",
                Description = "High-end gaming laptop",
                Price = 1499.99,
                SKU = "EL-2023",
                IsAvailable = true,
                InternalNotes = "Check supplier ID 3345",
                Categories = new List<string> { "catElectronics", "catGaming" },
                Features = new object[] { "featureRGB", "featureCooling" },
                Ratings = new int[] { 5, 4, 4, 5 },
                Metadata = new { Warranty = "2 years", Color = "Black" }
            };

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                Console.WriteLine("Product 01 :");
                if (!myserializer.Serialize(writer, product1)) Console.WriteLine("\nFailed to serialize the object\n");
            }

            Console.WriteLine("\n-------------------------------------------\n");
          
            var product2 = new Product
            {
                Name = null, // Should trigger [MyRequired]
                SKU = "XX-9999",
                Categories = new List<string> { "catOffice" },
                Features = new object[] { "featureErgo" }
            };

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                Console.WriteLine("Product 02 :");
                if (!myserializer.Serialize(writer, product2)) Console.WriteLine("Failed to serialize the object");
            }

            Console.WriteLine("\n-------------------------------------------\n");

            var product3 = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Desk Chair",
                SKU = "123-XYZ", // Invalid: does not match pattern ^[A-Z]{2}-\d{4}
                Categories = new List<string> { "catFurniture" },
                Features = new object[] { "featureAdjustable" }
            };

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                Console.WriteLine("Product 03 :");
                if (!myserializer.Serialize(writer, product3)) Console.WriteLine("Failed to serialize the object");
            }

            Console.WriteLine("\n-------------------------------------------\n");

            var product4 = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Monitor",
                Price = null,              // Should default to 0.0
                Description = null,        // Should default to "N/A"
                SKU = "MO-2024",
                IsAvailable = null,        // Should default to true
                Categories = null,
                Features = null,
                Metadata = null
            };

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                Console.WriteLine("Product 04 :");
                if (!myserializer.Serialize(writer, product4)) Console.WriteLine("Failed to serialize the object");
            }

            Console.WriteLine("\n-------------------------------------------\n");

            var product5 = new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Smart Hub",
                SKU = "SH-0001",
                Categories = new List<string> { "catSmart", "catHome" },
                Features = new object[]
                {
                    new object[] { "featureVoice", "featureApp" },
                    "featureAutomation"
                },
                Ratings = new int[] { 3, 4, 5 }
            };

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                Console.WriteLine("Product 05 :");
                if (!myserializer.Serialize(writer, product5)) Console.WriteLine("Failed to serialize the object");
            }

            Console.WriteLine("\n-------------------------------------------\n");



          
            
        }
    }
}
