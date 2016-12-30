using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace WcfRestClient.Test.WCF
{
    [ServiceContract]
    public interface ISampleService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/getMessage/{someString}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<Tuple<string, List<Guid>>> GetMessage(string someString, List<Guid> listOfGuids);

        [OperationContract]
        [WebGet(UriTemplate = "/hello", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<string> GetHello();

        [OperationContract]
        [WebInvoke(Method = "GET", UriTemplate = "/getfirstDate?first={first}&second={second}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<DateTime> GetFirstDate(DateTime first, DateTime? second);    
    }
}
