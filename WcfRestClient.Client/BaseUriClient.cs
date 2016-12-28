using System;
using WcfRestClient.Core;
using WcfRestClient.Utils;

namespace WcfRestClient.Client
{
    public static class BaseUriClient<T> where T : IDisposable
    {
        public static T New(Uri baseUri, string baseSubUri, TimeSpan? timeout)
        {
            var processor = new BaseUriProcessor(baseUri.Concat(baseSubUri), timeout);
            return ServiceClient<T>.New(processor);
        }
    }
}
