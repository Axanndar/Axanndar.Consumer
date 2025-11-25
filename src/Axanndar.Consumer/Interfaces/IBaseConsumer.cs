using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Interfaces
{
    /// <summary>
    /// Defines the basic contract for a consumer, including endpoint identification and asynchronous operations for creation and message reception.
    /// </summary>
    internal interface IBaseConsumer : IAsyncDisposable
    {
        /// <summary>
        /// Gets the identifier of the endpoint associated with the consumer.
        /// </summary>
        public string? IdEndpoint { get; }

        /// <summary>
        /// Asynchronously creates or initializes the consumer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task CreateConsumer();

        /// <summary>
        /// Asynchronously receives a message from the endpoint.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ReceiveMessage();
    }
}
