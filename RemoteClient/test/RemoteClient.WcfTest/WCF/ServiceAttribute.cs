using System;

namespace RemoteClient.WcfTest.WCF
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(string subAddress)
        {
            SubAddress = subAddress;
        }

        public string SubAddress { get; }
    }
}
