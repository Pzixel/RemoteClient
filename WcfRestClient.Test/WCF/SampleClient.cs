using System;
using WcfRestClient.Test.WCF.Common;

namespace WcfRestClient.Test.WCF
{
    public static class SampleClient
    {
        public static ISampleClient New(Uri baseUri, TimeSpan? timeout = null)
        {
            return BaseUriClient<ISampleClient>.New(baseUri, Constant.ServiceSampleUri, timeout);
        }
    }
}
