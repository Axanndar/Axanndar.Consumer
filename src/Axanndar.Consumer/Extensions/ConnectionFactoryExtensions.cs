using ActiveMQ.Artemis.Client;
using Axanndar.Consumer.Factory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IArtemisClientConnectionFactory"/> to simplify connection creation.
    /// </summary>
    public static class ArtemisClientConnectionFactoryExtensions
    {
        /// <summary>
        /// Creates a new connection asynchronously using the specified endpoints.
        /// </summary>
        /// <param name="connectionFactory">The connection factory instance.</param>
        /// <param name="endpoints">A collection of endpoints to connect to.</param>
        /// <returns>A task representing the asynchronous connection creation operation.</returns>
        public static Task<IConnection> CreateAsync(this IArtemisClientConnectionFactory connectionFactory, IEnumerable<Endpoint> endpoints)
        {
            return connectionFactory.CreateAsync(endpoints, CancellationToken.None);
        }

        /// <summary>
        /// Creates a new connection asynchronously using a single endpoint and a cancellation token.
        /// </summary>
        /// <param name="connectionFactory">The connection factory instance.</param>
        /// <param name="endpoint">The endpoint to connect to.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous connection creation operation.</returns>
        public static Task<IConnection> CreateAsync(this IArtemisClientConnectionFactory connectionFactory, Endpoint endpoint, CancellationToken cancellationToken)
        {
            return connectionFactory.CreateAsync(new[] { endpoint }, cancellationToken);
        }

        /// <summary>
        /// Creates a new connection asynchronously using a single endpoint.
        /// </summary>
        /// <param name="connectionFactory">The connection factory instance.</param>
        /// <param name="endpoint">The endpoint to connect to.</param>
        /// <returns>A task representing the asynchronous connection creation operation.</returns>
        public static Task<IConnection> CreateAsync(this IArtemisClientConnectionFactory connectionFactory, Endpoint endpoint)
        {
            return connectionFactory.CreateAsync(new[] { endpoint }, CancellationToken.None);
        }
    }
}
