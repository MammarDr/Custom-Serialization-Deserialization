using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace CustomAttribute
{

    /*
     * My Custom Attributes
     * Required - DefaultValue - Order - NickName
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

    public enum enPatternTarget
    {
        Key, Value, Both
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MyPatternAttribute : Attribute
    {
        public string Pattern { get; }

        public MyPatternAttribute(string Pattern) => this.Pattern = Pattern;

    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class MyKeyValuePatternAttribute : MyPatternAttribute
    {

        public enPatternTarget Target { get; }


        public MyKeyValuePatternAttribute(string Pattern, enPatternTarget Target) : base(Pattern)
        {
            this.Target = Target;
        }

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
            ArgumentNullException.ThrowIfNull(member, nameof(member));

            return member.IsDefined(typeof(MyNonSerializableAttribute), false);
        }

        static public string GetNickName(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));

            MyNickNameAttribute? attr = member.GetCustomAttribute(typeof(MyNickNameAttribute), false) as MyNickNameAttribute;
            return attr?.Name ?? member.Name;
        }

        static public object? GetDefaultValue(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));

            MyDefaultValueAttribute? attr = member?.GetCustomAttribute(typeof(MyDefaultValueAttribute), false) as MyDefaultValueAttribute;
            return attr?.Value;
        }

       static public string? GetPattern(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));

            MyPatternAttribute? attr = member.GetCustomAttribute(typeof(MyPatternAttribute), false) as MyPatternAttribute;
            return attr?.Pattern;
        }

        static public KeyValuePair<string?, string?> GetKeyValuePattern(MemberInfo member)
        {
            ArgumentNullException.ThrowIfNull(member, nameof(member));

            IEnumerable<MyKeyValuePatternAttribute> pattern = member.GetCustomAttributes<MyKeyValuePatternAttribute>(false);

            string ? keyPattern = null, valuePattern = null;

            if (pattern != null)
            {
                foreach (var p in pattern)
                {
                    if (p.Target == enPatternTarget.Key)
                        keyPattern = p.Pattern;
                    else if (p.Target == enPatternTarget.Value)
                        valuePattern = p.Pattern;
                    else
                    {
                        keyPattern = p.Pattern;
                        valuePattern = p.Pattern;
                        break;
                    }
                }
            }

            return new KeyValuePair<string?, string?>(keyPattern, valuePattern);
        }

        static public int? GetOrder(MemberInfo member)
        {
            if (!member.IsDefined( typeof(MyOrderAttribute) )) return null;

            MyOrderAttribute? attr = member.GetCustomAttribute(typeof(MyOrderAttribute), false) as MyOrderAttribute;

            return attr?.Order;
        }

    }
}
