using System;

namespace RemoteClient.WcfTest.WCF.TestClasses
{
    public interface ISampleClient : ISampleService, IDisposable
    {
    }
}
