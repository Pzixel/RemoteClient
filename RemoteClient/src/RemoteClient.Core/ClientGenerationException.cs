using System;

namespace RemoteClient.Core
{
    public class ClientGenerationException : InvalidOperationException
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
    }
}
