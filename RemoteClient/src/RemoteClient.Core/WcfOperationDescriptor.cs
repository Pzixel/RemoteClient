namespace WcfRestClient.Core
{
    public struct WcfOperationDescriptor
    {
        public string UriTemplate { get; }
        public string Method { get; }
        public OperationWebMessageFormat RequestFormat { get; }
        public OperationWebMessageFormat ResponseFormat { get; }

        public WcfOperationDescriptor(string uriTemplate, string method, OperationWebMessageFormat requestFormat, OperationWebMessageFormat responseFormat)
        {
            UriTemplate = uriTemplate;
            Method = method;
            RequestFormat = requestFormat;
            ResponseFormat = responseFormat;
        }
    }
}