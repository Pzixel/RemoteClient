using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using NUnit.Framework;
using WcfRestClient.Core;
using WcfRestClient.Test.AsyncProcessor;

namespace WcfRestClient.Test
{
    [TestFixture]
    public class IAsyncRequestProcessorTest
    {
        [Test]
        public async Task Sample()
        {
            using (var task = ServiceClient<ISample>.New(new AsyncProcessorSample()))
            {
                const int intSample = 10;
                var guidSample = Guid.Empty;
                var dictSample = new Dictionary<string, double> {["A"] = 274};
                var request = await task.GetWcfRequest(intSample, guidSample, dictSample);

                Assert.AreEqual("HEAD", request.Descriptor.Method);
                Assert.AreEqual("sample?foo={foo}&bar={bar}", request.Descriptor.UriTemplate);
                Assert.AreEqual(OperationWebMessageFormat.Json, request.Descriptor.RequestFormat);
                Assert.AreEqual(OperationWebMessageFormat.Json, request.Descriptor.ResponseFormat);

                Assert.IsTrue(request.QueryStringParameters.ContainsKey("foo"));
                Assert.AreEqual(intSample, request.QueryStringParameters["foo"]);
                Assert.IsTrue(request.QueryStringParameters.ContainsKey("bar"));
                Assert.AreEqual(guidSample, request.QueryStringParameters["bar"]);
                Assert.IsTrue(request.BodyPrameters.ContainsKey("someLoad"));

                var dict = request.BodyPrameters["someLoad"] as IReadOnlyDictionary<string, double>;
                Assert.IsNotNull(dict);
                Assert.IsTrue(dict.SequenceEqual(dictSample));
            }
        }
    }
}

