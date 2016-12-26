using System;
using WcfRestClient;
using WcfRestClient.Core;

namespace WcfResrClient.Console
{
    public static class FooClient
    {
        public static IFoo Processor = ServiceClient<IFoo>.New(new BaseUriProcessor(new Uri("report", UriKind.Relative)));
    }
}
