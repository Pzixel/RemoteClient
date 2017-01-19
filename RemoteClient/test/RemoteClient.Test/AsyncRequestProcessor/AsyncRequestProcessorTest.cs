using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RemoteClient.Core;
using Xunit;

namespace RemoteClient.Test.AsyncRequestProcessor
{
    public class AsyncRequestProcessorTest
    {
        [Fact]
        public async Task SampleAsync()
        {
            var asyncRequestProcessor = new AsyncProcessorSample();
            using (var task = ServiceClient<ISample>.New(asyncRequestProcessor))
            {
                const int intSample = 10;
                var guidSample = Guid.Empty;
                var dictSample = new Dictionary<string, double> {["A"] = 274};
                var request = await task.GetWcfRequest(intSample, guidSample, dictSample);

                Assert.Equal("HEAD", request.Descriptor.Method);
                Assert.Equal("sample?foo={foo}&bar={bar}", request.Descriptor.UriTemplate);
                Assert.Equal(OperationWebMessageFormat.Json, request.Descriptor.RequestFormat);
                Assert.Equal(OperationWebMessageFormat.Json, request.Descriptor.ResponseFormat);

                Assert.True(request.QueryStringParameters.ContainsKey("foo"));
                Assert.Equal(intSample, request.QueryStringParameters["foo"]);
                Assert.True(request.QueryStringParameters.ContainsKey("bar"));
                Assert.Equal(guidSample, request.QueryStringParameters["bar"]);
                Assert.True(request.BodyPrameters.ContainsKey("someLoad"));

                var dict = request.BodyPrameters["someLoad"] as IReadOnlyDictionary<string, double>;
                Assert.NotNull(dict);
                Assert.True(dict.SequenceEqual(dictSample));
            }
            Assert.True(asyncRequestProcessor.IsDisposed);
        }
    }
}

