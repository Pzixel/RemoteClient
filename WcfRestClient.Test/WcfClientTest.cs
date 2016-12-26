using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using WcfRestClient.ServiceHoster;
using WcfRestClient.Test.WCF;

namespace WcfRestClient.Test
{
    [TestFixture]
    public class WcfClientTest
    {
        [Test]
        public async Task Main()
        {
            var baseAddress = new Uri("http://localhost:8080/", UriKind.Absolute);
            var manager = new ServiceManager();
            manager.RunAll(baseAddress);

            const string testString = "My Super String";
            var dt = DateTime.UtcNow;

            using (var client = SampleClient.New(baseAddress))
            {
                var hello = await client.GetHello();
                var message = await client.GetMessage(testString, Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToList());
                var firstDate = await client.GetFirstDate(dt, null);

                Assert.AreEqual(hello, "Hello");
                Assert.AreEqual(message.Item1, "Hello " + testString);
                Assert.AreEqual(message.Item2.Count, 100);
                Assert.IsTrue(message.Item2.All(x => x != Guid.Empty));
                Assert.AreEqual(firstDate, dt);
            }
            manager.CloseAll();
        }
    }
}
