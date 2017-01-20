using System;
using System.Net;

namespace RemoteClient.Client.Utils
{
    public class BadStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public BadStatusCodeException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public BadStatusCodeException(HttpStatusCode statusCode) : this(statusCode, "The web server returned a status code indicating failure", null)
        {
        }

        public override string ToString()
        {
            return $"{nameof(BadStatusCodeException)}: Message = {Message}, Status code = '{StatusCode}'";
        }
    }
}
