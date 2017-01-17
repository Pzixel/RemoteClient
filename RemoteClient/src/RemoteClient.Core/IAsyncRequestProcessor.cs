using System;
using System.Threading.Tasks;

namespace RemoteClient.Core
{
    public interface IAsyncRequestProcessor : IDisposable
    {
        Task<T> GetResultAsync<T>(IRemoteRequest request);
        Task ExecuteAsync(IRemoteRequest request);
    }
}
