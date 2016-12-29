using System;
using System.Threading.Tasks;

namespace WcfRestClient.Core
{
    public interface IAsyncRequestProcessor : IDisposable
    {
        Task<T> GetResultAsync<T>(IWcfRequest descriptor);
        Task ExecuteAsync(IWcfRequest descriptor);
    }
}
