using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Exceptions
{
    public class ConsumerWorkerException : Exception
    {
        public ConsumerWorkerException() : base()
        {
        }

        public ConsumerWorkerException(string message) : base(message)
        {
        }
    }
}
