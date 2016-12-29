using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WcfRestClient.Core;
using WcfRestClient.Test.AsyncProcessor;

namespace WcfRestClient.Test
{
    [TestFixture]
    public class IAsyncRequestProcessorTest
    {
        [Test]
        public void Sample()
        {
            var task = ServiceClient<ISample>.New(new AsyncProcessorSample());
        }
    }
}
