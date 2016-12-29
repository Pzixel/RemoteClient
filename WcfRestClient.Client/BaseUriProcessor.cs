using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WcfRestClient.Core;
using WcfRestClient.Utils;

namespace WcfRestClient.Client
{
    internal class BaseUriProcessor : IAsyncRequestProcessor
    {
        private readonly RestClient _client;

        public BaseUriProcessor(Uri baseUri, TimeSpan? timeout)
        {
            _client = new RestClient(baseUri, timeout);
        }

        public Task<T> GetResultAsync<T>(IWcfRequest request)
        {
            if (request.BodyPrameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(request.Descriptor.UriTemplate, request.QueryStringParameters);
            return _client.SendAsync<T>(new HttpMethod(request.Descriptor.Method), queryString, request.BodyPrameters.Values.FirstOrDefault());
        }

        public Task ExecuteAsync(IWcfRequest request)
        {
            if (request.BodyPrameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(request.Descriptor.UriTemplate, request.QueryStringParameters);
            return _client.SendAsync(new HttpMethod(request.Descriptor.Method), queryString, request.BodyPrameters.Values.FirstOrDefault());
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}