using System.Collections.Generic;

namespace WcfRestClient.Core
{
    public interface IWcfRequest
    {
        WcfOperationDescriptor Descriptor { get; }
        IReadOnlyDictionary<string, object> QueryStringParameters { get; }
        IReadOnlyDictionary<string, object> BodyPrameters { get; }
    }
}
