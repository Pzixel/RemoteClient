using System;
using System.Threading.Tasks;
using RemoteClient.Core;

namespace RemoteClient.Test.AsyncRequestProcessor
{
    public class AsyncProcessorSample : IAsyncRequestProcessor
    {
        public Task<T> GetResultAsync<T>(IRemoteRequest request)
        {
            if (typeof(T) == typeof(IRemoteRequest))
                return Task.FromResult((T) request);
            throw new NotImplementedException();
        }

        public Task ExecuteAsync(IRemoteRequest request)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}