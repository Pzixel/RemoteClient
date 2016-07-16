using System;
using System.Runtime.Serialization;

namespace WcfRestClient
{
    [Serializable]
    public class ClientGenerationException : Exception
    {
        public ClientGenerationException()
        {
        }

        public ClientGenerationException(string message) : base(message)
        {
        }

        public ClientGenerationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ClientGenerationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
