using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace RemoteClient.WcfTest.WCF.TestClasses
{
    [ServiceContract]
    public interface ISampleBaseService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/getMessage/{someString}", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json)]
        Task<Tuple<string, List<Guid>>> GetMessage(string someString, List<Guid> listOfGuids);
    }
}