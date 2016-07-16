using System.Collections.Generic;
using System.Threading.Tasks;

namespace WcfRestClient
{
    public interface IAsyncRequestProcessor
    {
        Task<T> GetResultAsync<T>(string uriTemplate, string method, Dictionary<string, object> parameters);
        Task ExecuteAsync(string uriTemplate, string method, Dictionary<string, object> parameters);
    }
}
