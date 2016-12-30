using System;
using System.Threading.Tasks;
using WcfRestClient.Core;

namespace WcfRestClient.Test.AsyncProcessor
{
    public class AsyncProcessorSample : IAsyncRequestProcessor
    {
        public Task<T> GetResultAsync<T>(IWcfRequest descriptor)
        {
            if (typeof(T) == typeof(IWcfRequest))
                return Task.FromResult((T) descriptor);
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IWcfRequest descriptor)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}