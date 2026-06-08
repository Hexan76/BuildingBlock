using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.HttpClient.Abstractions.Exceptions
{
    public class TransportException : Exception
    {
        public TransportException(string message)
            : base(message)
        {
        }

        public TransportException(
            string message,
            Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
