using System;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace WcfRestClient.ServiceHoster.Extensions
{
    internal static class OperationDescriptionExtension
    {
        public static WebMessageBodyStyle? GetBodyStyle(this OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return wga.BodyStyle;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.BodyStyle;
            }

            return null;
        }

        public static bool IsGetOrDeleteOperation(this OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return true;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.Method == "GET" || wia.Method == "DELETE";
            }

            return false;
        }

        public static MessagePartDescription GetParameter(this OperationDescription operation, int index)
        {
            return operation.Messages[0].Body.Parts[index];
        }

        public static bool IsUriParameter(this OperationDescription operation, string paramName)
        {
            string uriTemplate = operation.GetUriTemplate();
            string paramBracedName = "{" + paramName + "}";
            return uriTemplate.IndexOf(paramBracedName, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetUriTemplate(this OperationDescription operation)
        {
            var wga = operation.Behaviors.Find<WebGetAttribute>();
            if (wga != null)
            {
                return wga.UriTemplate;
            }

            var wia = operation.Behaviors.Find<WebInvokeAttribute>();
            if (wia != null)
            {
                return wia.UriTemplate;
            }
            throw new InvalidOperationException();
        }
    }
}
