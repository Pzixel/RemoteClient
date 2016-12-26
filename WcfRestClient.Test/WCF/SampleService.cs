using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WcfRestClient.ServiceHoster;

namespace WcfRestClient.Test.WCF
{
    [Service(Constant.ServiceSampleUri)]
    public class SampleService : ISampleService
    {
        public async Task<Tuple<string, List<Guid>>> GetMessage(string someString, List<Guid> listOfGuids)
        {
            await Task.Delay(0).ConfigureAwait(false); //some work
            return new Tuple<string, List<Guid>>("Hello " + someString, listOfGuids);
        }

        public Task<string> GetHello()
        {
            return Task.FromResult("Hello");
        }

        public Task<DateTime> GetFirstDate(DateTime first, DateTime? second)
        {
            return Task.FromResult(first);
        }
    }
}
