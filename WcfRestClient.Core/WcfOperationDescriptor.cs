using System.ServiceModel.Web;

namespace WcfRestClient.Core
{
    public struct WcfOperationDescriptor : IWcfOperationDescriptor
    {
        public string UriTemplate { get; }
        public string Method { get; }
        public WebMessageBodyStyle BodyStyle { get; }
        public WebMessageFormat RequestFormat { get; }
        public WebMessageFormat ResponseFormat { get; }

        public WcfOperationDescriptor(string uriTemplate, string method, WebMessageBodyStyle bodyStyle, WebMessageFormat requestFormat, WebMessageFormat responseFormat)
        {
            UriTemplate = uriTemplate;
            Method = method;
            BodyStyle = bodyStyle;
            RequestFormat = requestFormat;
            ResponseFormat = responseFormat;
        }
    }
}