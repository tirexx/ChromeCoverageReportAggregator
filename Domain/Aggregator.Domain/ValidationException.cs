using System;
using System.Runtime.Serialization;

namespace Aggregator.Domain
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException()
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}