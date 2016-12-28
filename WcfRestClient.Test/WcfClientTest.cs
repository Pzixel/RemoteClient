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
        private static readonly Uri BaseAddress = new Uri("http://localhost:8080/", UriKind.Absolute);

        [Test]
        public async Task TestHello()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            using (var client = SampleClient.New(BaseAddress))
            {
                var hello = await client.GetHello();

                Assert.AreEqual(hello, "Hello");
            }
            manager.CloseAll();
        }

        [Test]
        public async Task TestList()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            const string testString = "My Super String+3:00+15=3:15";

            using (var client = SampleClient.New(BaseAddress))
            {
                var message = await client.GetMessage(testString, Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToList());

                Assert.AreEqual(message.Item1, "Hello " + testString);
                Assert.AreEqual(message.Item2.Count, 100);
                Assert.IsTrue(message.Item2.All(x => x != Guid.Empty));
            }
            manager.CloseAll();
        }

        [Test]
        public async Task TestNullableDate()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            var dt = DateTime.UtcNow;

            using (var client = SampleClient.New(BaseAddress))
            {
                var firstDate = await client.GetFirstDate(dt, null);

                Assert.AreEqual(firstDate.Date, dt.Date);
                Assert.AreEqual(firstDate.Hour, dt.Hour);
                Assert.AreEqual(firstDate.Minute, dt.Minute);
                Assert.AreEqual(firstDate.Second, dt.Second);
            }
            manager.CloseAll();
        }
    }
}
