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

        public Task<T> GetResultAsync<T>(string uriTemplate, string method, Dictionary<string, object> uriParameters, Dictionary<string, object> bodyParameters)
        {
            if (bodyParameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(uriTemplate, uriParameters);
            return _client.SendAsync<T>(new HttpMethod(method), queryString, bodyParameters.Values.FirstOrDefault());
        }

        public Task ExecuteAsync(string uriTemplate, string method, Dictionary<string, object> uriParameters, Dictionary<string, object> bodyParameters)
        {
            if (bodyParameters.Count > 1)
                throw new Exception("WrappedRequests not supported");
            var queryString = WcfUri.GetUriFromTemlate(uriTemplate, uriParameters);
            return _client.SendAsync(new HttpMethod(method), queryString, bodyParameters.Values.FirstOrDefault());
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}