using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace RemoteClient.WcfTest.WCF.TestClasses
{
    [ServiceContract]
    public interface ISampleService : ISampleBaseService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/hello", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<string> GetHello();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/getfirstDate?first={first}&second={second}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<DateTime> GetFirstDate(DateTime first, DateTime? second);    
    }
}
