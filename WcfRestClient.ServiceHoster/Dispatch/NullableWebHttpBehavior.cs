using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WcfRestClient.ServiceHoster.Dispatch
{
    public class NullableWebHttpBehavior : WebHttpBehavior
    {
        protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription)
        {
            return NullableQueryStringConverter.Instance;
        }
    }
}
