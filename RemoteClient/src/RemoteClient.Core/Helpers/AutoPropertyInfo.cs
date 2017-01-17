using System.Reflection;

namespace RemoteClient.Core.Helpers
{
    internal struct AutoPropertyInfo
    {
        public PropertyInfo PropertyInfo { get;}
        public FieldInfo FieldInfo { get;}

        public AutoPropertyInfo(PropertyInfo propertyInfo, FieldInfo fieldInfo)
        {
            PropertyInfo = propertyInfo;
            FieldInfo = fieldInfo;
        }
    }
}
