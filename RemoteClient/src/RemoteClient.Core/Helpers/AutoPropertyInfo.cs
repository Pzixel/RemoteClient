using System.Reflection;

namespace RemoteClient.Core.Helpers
{
    internal struct AutoPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; }
        public FieldInfo BackingFieldInfo { get; }

        public AutoPropertyInfo(PropertyInfo propertyInfo, FieldInfo backingFieldInfo)
        {
            PropertyInfo = propertyInfo;
            BackingFieldInfo = backingFieldInfo;
        }
    }
}
