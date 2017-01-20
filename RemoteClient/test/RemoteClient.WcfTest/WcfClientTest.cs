using System;
using System.Linq;
using System.Threading.Tasks;
using RemoteClient.WcfTest.WCF;
using RemoteClient.WcfTest.WCF.TestClasses;
using Xunit;

namespace RemoteClient.WcfTest
{
    public class WcfClientTest
    {
        private static readonly Uri BaseAddress = new Uri("http://localhost:8080/", UriKind.Absolute);

        [Fact]
        public async Task TestHelloAsync()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            using (var client = SampleClient.New(BaseAddress))
            {
                var hello = await client.GetHello();

                Assert.Equal(hello, "Hello");
            }
            manager.CloseAll();
        }

        [Fact]
        public async Task TestListAsync()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            const string testString = "My Super String+3:00+15=3:15";

            using (var client = SampleClient.New(BaseAddress))
            {
                var message = await client.GetMessage(testString, Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToList());

                Assert.Equal(message.Item1, "Hello " + testString);
                Assert.Equal(message.Item2.Count, 100);
                Assert.True(message.Item2.All(x => x != Guid.Empty));
            }
            manager.CloseAll();
        }

        [Fact]
        public async Task TestNullableDateAsync()
        {
            var manager = new ServiceManager();
            manager.RunAll(BaseAddress);

            var dt = DateTime.UtcNow;

            using (var client = SampleClient.New(BaseAddress))
            {
                var firstDate = await client.GetFirstDate(dt, null);

                Assert.Equal(firstDate.Date, dt.Date);
                Assert.Equal(firstDate.Hour, dt.Hour);
                Assert.Equal(firstDate.Minute, dt.Minute);
                Assert.Equal(firstDate.Second, dt.Second);
            }
            manager.CloseAll();
        }
    }
}
