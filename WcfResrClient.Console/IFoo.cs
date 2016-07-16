using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace WcfResrClient.Console
{
    public interface IFoo : IBaseFoo
    {
        [WebInvoke(UriTemplate = "/node/register", Method = "POST")]
        Task<string> RegisterNode(int a, int b, int c, int d, int e);
    }

    public interface IBaseFoo
    {
        [WebInvoke(UriTemplate = "www.google.com", Method = "DELETE")]
        Task PostSomething(int x);
    }
}