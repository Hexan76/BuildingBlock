using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.HttpClient.Abstractions.Exceptions
{
    public sealed class TransportTimeoutException
        : TransportException
    {
        public TransportTimeoutException(string message)
            : base(message)
        {
        }
    }
}
