using System.ServiceModel.Web;

namespace WcfRestClient.Core
{
    public interface IWcfOperationDescriptor
    {
        string UriTemplate { get; }
        string Method { get; }
        WebMessageBodyStyle BodyStyle { get; }
        WebMessageFormat RequestFormat { get; }
        WebMessageFormat ResponseFormat { get; }
    }
}