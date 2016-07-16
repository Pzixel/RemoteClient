using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WcfRestClient;
using static System.Console;

namespace WcfResrClient.Console
{
    internal class BaseUriProcessor : IAsyncRequestProcessor
    {
        private readonly Uri _baseSubUri;

        public BaseUriProcessor(Uri baseSubUri)
        {
            _baseSubUri = baseSubUri;
        }

        public Task<T> GetResultAsync<T>(string uriTemplate, string method, Dictionary<string, object> parameters)
        {
            // some realisation here
            WriteLine(_baseSubUri);
            WriteLine(uriTemplate);
            WriteLine(method);
            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    WriteLine($"{parameter.Key} = {parameter.Value}");
                }
            return Task.FromResult(default(T));
        }

        public Task ExecuteAsync(string uriTemplate, string method, Dictionary<string, object> parameters)
        {
            // some realisation here
            WriteLine(_baseSubUri);
            WriteLine(uriTemplate);
            WriteLine(method);
            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    WriteLine($"{parameter.Key} = {parameter.Value}");
                }
            return Task.FromResult(1);
        }
    }
}