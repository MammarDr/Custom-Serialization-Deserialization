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


namespace MySerilization
{

    /*
     * My Custom Attributes
     * Required - Default - Order - Ignore - NickName
     * Serializable - NonSerializable
     */

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MySerializableAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyNonSerializableAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyRequiredAttribute : Attribute { }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyDefaultValueAttribute : Attribute {
        public object Value { get; }
        public MyDefaultValueAttribute(object value) => Value = value;
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyNickNameAttribute : Attribute { 
        public string Name { get; }

        public MyNickNameAttribute(string Name) => this.Name = Name;

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyOrderAttribute : Attribute
    {

        public class MyOrderWrapper
        {
            public int order { get; set; }
            public MemberInfo member {  get; set; }
        }

        public int Order { get; }

        public MyOrderAttribute(int Order)
        {
            if(Order < 0) Order = 0;
            else this.Order = Order;
        }

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyTxtTypeAttribute : Attribute { 
        public string Text { get; }

        public MyTxtTypeAttribute(Type type) => Text = type.FullName;

        public MyTxtTypeAttribute(string text) => Text = text;

    }

    /*
     * My Custom Serializer
     * JSON Format
     */

    public class mySerializer
    {

        private Type _type;
        private ushort _fields_size;
        private ushort _property_size;
        private ushort _method_size;

        private mySerializer(Type type) {        
            _type = type;
            _fields_size = (ushort)type.GetMethods().Count();
            _property_size = (ushort)type.GetProperties().Count();
            _method_size = (ushort)type.GetMethods().Count();
        }

        static public mySerializer CreateInstance(Type type)
        {
            return type?.IsDefined(typeof(MyNonSerializableAttribute), false) == null ? null : new mySerializer(type);
        }

        private bool createKeyValueFormat(StringBuilder sb, MemberInfo member, object value)
        {
            return member != null ? createKeyValueFormat(sb, member.Name, value) : false;
        }

        private bool createKeyValueFormat(StringBuilder sb, Type type)
        {
            return type != null ? createKeyValueFormat(sb, "Type", type.Name) : false;
        }

        private bool createKeyValueFormat(StringBuilder sb, String Key, object Value, bool isKeyObject = false)
        {
            if (Value == null) Value = "NULL";
            else if (Value is string) // object.ReferenceEquals(value.GetType(), typeof(string))
            {
                Value = $"\"{Value}\"";
            }

            sb.Append((isKeyObject ? "\n   " : "\n ") + $"\"{Key}\" : {Value},");
            return true;
        }

        private bool createKeyObjectFormat(StringBuilder sb, String Property, MemberInfo[] members, object obj)
        {
            if (members == null || obj == null) return false;
            
            sb.Append($"\n \"{Property}\" : ");

            OpenWrapper(sb);

            foreach (MemberInfo member in members)
            {

                if (member == null) continue;

                object value = null;

                string Key = " ";


                if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(obj);
                }
                else if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(obj);
                }


                if (!member.IsDefined(typeof(MyNickNameAttribute), false)) Key = member.Name; 
                else
                {
                    string name = getAttrNickName(member);

                    if (name != null) Key = name;
                    else Key = member.Name;

                }


                if (value == null)
                {
                    // Attribute.IsDefined(member, typeof(MyDefaultValueAttribute))
                    if (member.IsDefined(typeof(MyDefaultValueAttribute), false))
                    {
                        object val = getAttrDefaultValue(member);

                        if (val != null) value = val;
                    }

                    if (value == null && member.IsDefined(typeof(MyRequiredAttribute), false)) return false;
                }

                createKeyValueFormat(sb, Key, value, true);
            }

            CloseWrapper(sb, true);

            return true;
        }

        private void finalizeFormat(StringBuilder sb)
        {
            if (sb[sb.Length - 1] == ',') sb[sb.Length - 1] = '\n';
            if (sb[sb.Length - 1] == '}') sb.Append("\n");
        }

        private void OpenWrapper(StringBuilder sb)
        {
            sb.Append("{");
        }

        private void CloseWrapper(StringBuilder sb, bool isKeyObject = false)
        {
            finalizeFormat(sb);
            sb.Append((isKeyObject ? "   " : "") + "},");
        }

        private bool hasAttrIgnore(MemberInfo member)
        {
            return member.IsDefined(typeof(MyNonSerializableAttribute), false);
        }

        private string getAttrNickName(MemberInfo member)
        {
            object attr = member?.GetCustomAttribute(typeof(MyNickNameAttribute), false);

            return ((MyNickNameAttribute)attr)?.Name ?? member.Name;
        }

        private object getAttrDefaultValue(MemberInfo member)
        {
            // Attribute.GetCustomAttribute(member,typeof(MyDefaultValueAttribute)) as MyDefaultValueAttribute;
            // var attr = member.GetCustomAttribute(typeof(MyDefaultValueAttribute)) as MyDefaultValueAttribute;
            object attr = member?.GetCustomAttribute(typeof(MyDefaultValueAttribute), false);

            return ((MyDefaultValueAttribute)attr)?.Value;
        }

        private int? getAttrOrder(MemberInfo member)
        {
            if (!member.IsDefined((typeof(MyOrderAttribute)))) return null;

            object attr = member.GetCustomAttribute(typeof(MyOrderAttribute), false);

            return ((MyOrderAttribute)attr)?.Order;
        }

        private void FieldsToList(List<MyOrderAttribute.MyOrderWrapper> list)
        {

            foreach (FieldInfo field in _type.GetFields())
            {
                if (field == null || hasAttrIgnore(field)) continue;
                list.Add(new MyOrderAttribute.MyOrderWrapper { member = field, order = getAttrOrder(field) ?? list.Count });
            }

        }

        private void PropertiesToList(List<MyOrderAttribute.MyOrderWrapper> list)
        {

            foreach (PropertyInfo prop in _type.GetProperties())
            {
                if (prop == null || hasAttrIgnore(prop)) continue;
                list.Add(new MyOrderAttribute.MyOrderWrapper { member = prop, order = getAttrOrder(prop) ?? list.Count });
            }

        }

        private void MethodsToList(List<MyOrderAttribute.MyOrderWrapper> list)
        {

            foreach (MethodInfo method in _type.GetMethods())
            {
                if (method == null || hasAttrIgnore(method)) continue;
                list.Add(new MyOrderAttribute.MyOrderWrapper { member = method, order = getAttrOrder(method) ?? list.Count });
            }

        }

        public MemberInfo[] appendMembersToList()
        {
            MemberInfo[] members = new MemberInfo[_property_size + _method_size];

            appendMembersToList(ref members);

            return members;

        }

        public void appendMembersToList(ref MemberInfo[] members)
        {
            List<MyOrderAttribute.MyOrderWrapper> 
                    memberWrapper = new List<MyOrderAttribute.MyOrderWrapper>();


            FieldsToList(memberWrapper);

            PropertiesToList(memberWrapper);

            members = memberWrapper.OrderBy(member => member.order)
                     .Select(member => member.member)
                     .ToArray();
        }

        public bool Serializer(TextWriter writer, object obj)
        {

            if(obj == null || writer == null) return false;

            StringBuilder    serializedTXT =    new StringBuilder();

            MemberInfo[]     membersList   =    appendMembersToList();

            OpenWrapper(serializedTXT);

            if (!createKeyValueFormat(serializedTXT, _type)) 
                return false;

            if(!createKeyObjectFormat(serializedTXT, "Data", membersList, obj)) 
                return false;

            CloseWrapper(serializedTXT);

            finalizeFormat(serializedTXT);

            writer.Write(serializedTXT);

            Console.WriteLine(serializedTXT.ToString());

            return true;

        }

    }

    // Class

    [MySerializable]
    public class Person
    {
        [MyRequired] //
        [MyOrder(0)]  public int? ID;

        
        [MyDefaultValue("Bob")] //
        [MyOrder(2)]  public string Name { get; set; }

        [MyNonSerializable] //
        public string City { get; set; }

        [MyDefaultValue(true)] //
        [MyOrder(5)] public bool? isAlive { get; set; }

        [MyNickName("ZipCode")] //
        [MyOrder(4)] public int? PostalCode { get; set; }

        [MyDefaultValue(18)] //
        [MyOrder(1)] public double? Age { get; set; }

        [MyTxtType(typeof(Person))]
        [MyDefaultValue(null)] //
        [MyOrder(3)] public object instance {  get; set; }
    }

    public class Program
    {

        static void Main(string[] args)
        {

            Person person = new Person { ID = 4, Name = "Ahmed", City = "Oued", PostalCode = 1006, Age = 2.5 };

            mySerializer myserializer = mySerializer.CreateInstance(typeof(Person));

            if (myserializer == null) return;

            using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
            {
                myserializer.Serializer(writer, person);
            }

        }
    }
}