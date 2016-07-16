using System;
using WcfRestClient;

namespace WcfResrClient.Console
{
    public static class FooClient
    {
        public static IFoo Processor = ServiceClient<IFoo>.New(new BaseUriProcessor(new Uri("report", UriKind.Relative)));
    }
}
