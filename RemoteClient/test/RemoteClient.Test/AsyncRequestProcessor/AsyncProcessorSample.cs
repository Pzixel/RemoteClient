using System;
using System.Threading.Tasks;
using WcfRestClient.Core;

namespace RemoteClient.Test.AsyncRequestProcessor
{
    public class AsyncProcessorSample : IAsyncRequestProcessor
    {
        public Task<T> GetResultAsync<T>(IWcfRequest request)
        {
            if (typeof(T) == typeof(IWcfRequest))
                return Task.FromResult((T) request);
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IWcfRequest request)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}