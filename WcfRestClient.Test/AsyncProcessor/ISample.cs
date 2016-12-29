using System;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace WcfRestClient.Test.AsyncProcessor
{
    public interface ISample : IDisposable
    {
        [WebGet(UriTemplate = "sample")]
        Task Foo();
    }
}
