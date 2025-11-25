using System;
using Axanndar.Consumer.Logger.Interfaces;

namespace Axanndar.Consumer.Logger
{
    /// <summary>
    /// Provides a unique correlation ID for tracking operations and requests.
    /// </summary>
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        /// <summary>
        /// Gets the current correlation ID.
        /// </summary>
        public string? CorrelationId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationIdProvider"/> class and generates a new correlation ID.
        /// </summary>
        public CorrelationIdProvider()
        {
            NewCorrelationId();
        }

        /// <summary>
        /// Generates a new unique correlation ID.
        /// </summary>
        public void NewCorrelationId()
        {
            CorrelationId = Guid.NewGuid().ToString();
        }
    }
}
