using System.Collections.Generic;

namespace RemoteClient.Core
{
    public interface IRemoteRequest
    {
        WcfOperationDescriptor Descriptor { get; }
        IReadOnlyDictionary<string, object> QueryStringParameters { get; }
        IReadOnlyDictionary<string, object> BodyParameters { get; }
    }
}
