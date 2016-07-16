namespace WcfRestClient
{
    internal struct WcfOperationDescriptor
    {
        public string UriTemplate { get; }
        public string Method { get; }

        public WcfOperationDescriptor(string uriTemplate, string method) : this()
        {
            UriTemplate = uriTemplate;
            Method = method;
        }
    }
}