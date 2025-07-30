using CustomAttribute;
using CustomJSON;
using System.Reflection;


namespace CustomSerializer
{
    abstract public class MySerializer
    {

        protected Type _type;
        protected ushort _fields_size;
        protected ushort _method_size;
        protected ushort _property_size;

        protected MySerializer(Type type)
        {
            _type = type;
            _fields_size = (ushort)type.GetFields().Count();
            _property_size = (ushort)type.GetProperties().Count();
            _method_size = (ushort)type.GetMethods().Count();
        }

        public abstract bool Serialize(TextWriter Writer, object Origin);

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

            members = memberWrapper.OrderBy(item => item.order)
                     .Select(item => item.member)
                     .ToArray();
        }
    }
}
