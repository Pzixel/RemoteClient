using RemoteClient.Core;

// ReSharper disable once CheckNamespace
namespace System.ServiceModel.Web
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WebInvokeAttribute : Attribute
    {
        OperationWebMessageFormat requestMessageFormat;
        OperationWebMessageFormat responseMessageFormat;

        public bool IsBodyStyleSetExplicitly { get; }

        public bool IsRequestFormatSetExplicitly { get; private set; }

        public bool IsResponseFormatSetExplicitly { get; private set; }

        public string Method { get; set; }

        public OperationWebMessageFormat RequestFormat
        {
            get
            {

                return requestMessageFormat;
            }
            set
            {
                requestMessageFormat = value;
                IsRequestFormatSetExplicitly = true;
            }
        }

        public OperationWebMessageFormat ResponseFormat
        {
            get
            {

                return responseMessageFormat;
            }
            set
            {
                responseMessageFormat = value;
                IsResponseFormatSetExplicitly = true;
            }
        }

        public string UriTemplate { get; set; }
    }
}