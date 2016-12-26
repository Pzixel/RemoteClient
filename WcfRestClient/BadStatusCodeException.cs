using System;
using System.Net;
using System.Runtime.Serialization;

namespace WcfRestClient
{
    [Serializable]
    public class BadStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BadStatusCodeException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public BadStatusCodeException(HttpStatusCode statusCode) : this(statusCode, "BadStatusCodeException occurred", null)
        {
        }

        public BadStatusCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override string ToString()
        {
            return $"{nameof(BadStatusCodeException)}: Message = {Message}, Status code = '{StatusCode}'";
        }
    }
}
