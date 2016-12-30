using System;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using WcfRestClient.Core;

namespace WcfRestClient.Test.AsyncProcessor
{
    public interface ISample : IDisposable
    {
        [WebInvoke(Method = "HEAD", UriTemplate = "sample?foo={foo}&bar={bar}", BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Task<IWcfRequest> GetWcfRequest(int foo, Guid bar, Dictionary<string, double> someLoad);
    }
}
