using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using RemoteClient.Client.Utils;
using RemoteClient.Core;

namespace RemoteClient.Client
{
    internal class BaseUriProcessor : IAsyncRequestProcessor
    {
        private readonly RestClient _client;

        public BaseUriProcessor(Uri baseUri, TimeSpan? timeout)
        {
            _client = new RestClient(baseUri, timeout);
        }

        public Task<T> GetResultAsync<T>(IRemoteRequest request)
        {
            if (request.BodyParameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(request.Descriptor.UriTemplate, request.QueryStringParameters);
            return _client.SendAsync<T>(new HttpMethod(request.Descriptor.Method), queryString, request.BodyParameters.Values.FirstOrDefault());
        }

        public Task ExecuteAsync(IRemoteRequest request)
        {
            if (request.BodyParameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(request.Descriptor.UriTemplate, request.QueryStringParameters);
            return _client.SendAsync(new HttpMethod(request.Descriptor.Method), queryString, request.BodyParameters.Values.FirstOrDefault());
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}