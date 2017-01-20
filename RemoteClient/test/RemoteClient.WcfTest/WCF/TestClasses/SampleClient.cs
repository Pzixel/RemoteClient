using System;
using RemoteClient.Client;

namespace RemoteClient.WcfTest.WCF.TestClasses
{
    public static class SampleClient
    {
        public static ISampleClient New(Uri baseUri, TimeSpan? timeout = null)
        {
            return BaseUriClient<ISampleClient>.New(baseUri, Constant.ServiceSampleUri, timeout);
        }
    }
}
