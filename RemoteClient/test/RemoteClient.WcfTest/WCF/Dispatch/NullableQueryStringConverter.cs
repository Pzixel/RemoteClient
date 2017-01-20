using System;
using System.ServiceModel.Dispatcher;

namespace RemoteClient.WcfTest.WCF.Dispatch
{
    public class NullableQueryStringConverter : QueryStringConverter
    {
        public static NullableQueryStringConverter Instance { get; } = new NullableQueryStringConverter();
        public override bool CanConvert(Type type)
        {
            if (base.CanConvert(type))
                return true;
            var underlyingType = Nullable.GetUnderlyingType(type);
            var canConvert = underlyingType != null && base.CanConvert(underlyingType);
            return canConvert;
        }

        public override object ConvertStringToValue(string parameter, Type parameterType)
        {
            var underlyingType = Nullable.GetUnderlyingType(parameterType);

            // Handle nullable types
            if (underlyingType != null)
            {
                // Define a null value as being an empty or missing (null) string passed as the query parameter value
                return string.IsNullOrEmpty(parameter) ? null : base.ConvertStringToValue(parameter, underlyingType);
            }

            return base.ConvertStringToValue(parameter, parameterType);
        }
    }
}
