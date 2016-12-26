using System;

namespace WcfRestClient.ServiceHoster
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
