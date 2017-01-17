using System;
using System.Collections.Generic;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using RemoteClient.Core;

namespace RemoteClient.Test.AsyncRequestProcessor
{
    public interface ISample : IDisposable
    {
        [WebInvoke(Method = "HEAD", UriTemplate = "sample?foo={foo}&bar={bar}", RequestFormat = OperationWebMessageFormat.Json, ResponseFormat = OperationWebMessageFormat.Json)]
        Task<IRemoteRequest> GetWcfRequest(int foo, Guid bar, Dictionary<string, double> someLoad);
    }
}
