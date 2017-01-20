using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace RemoteClient.WcfTest.WCF.Dispatch
{
    public class NullableWebHttpBehavior : WebHttpBehavior
    {
        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return NullableQueryStringConverter.Instance;
        }
    }
}
