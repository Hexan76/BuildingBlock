using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.HttpClient.Abstractions.Exceptions
{
    public sealed class TransportSerializationException
        : TransportException
    {
        public TransportSerializationException(string message)
            : base(message)
        {
        }

        public TransportSerializationException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
