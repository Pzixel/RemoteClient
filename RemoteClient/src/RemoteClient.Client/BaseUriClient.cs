using System;
using RemoteClient.Client.Utils;
using RemoteClient.Core;

namespace RemoteClient.Client
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
