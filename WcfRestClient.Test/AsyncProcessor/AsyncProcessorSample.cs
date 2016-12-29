using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WcfRestClient.Core;

namespace WcfRestClient.Test.AsyncProcessor
{
    public class AsyncProcessorSample : IAsyncRequestProcessor
    {
        public Task<T> GetResultAsync<T>(IWcfRequest descriptor)
        {
            foreach (var frame in new StackTrace().GetFrames())
            {
                Console.WriteLine(frame.GetMethod().Name);
            }
            return Task.FromResult(default(T));
        }

        public Task ExecuteAsync(IWcfRequest descriptor)
        {
            foreach (var frame in new StackTrace().GetFrames())
            {
                Console.WriteLine(frame.GetMethod().Name);
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }
    }
}