using System;

namespace RemoteClient.Core
{
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
    }
}
