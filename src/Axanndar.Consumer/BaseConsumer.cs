using ActiveMQ.Artemis.Client;
using Amqp.Handler;
using Axanndar.Consumer.Constants;
using Axanndar.Consumer.Enums;
using Axanndar.Consumer.Extensions;
using Axanndar.Consumer.Factory.Interfaces;
using Axanndar.Consumer.Interfaces;
using Axanndar.Consumer.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer
{
    /// <summary>
    /// Abstract base class for consumers implementing the <see cref="IBaseConsumer"/> interface.
    /// </summary>
    public abstract class BaseConsumer : IBaseConsumer
    {
        // Consumer configuration
        private readonly Models.ConsumerConfiguration _consumerConfiguration;
        // Factory to create Artemis connections
        private readonly IArtemisClientConnectionFactory _connectionFactory;
        // Active Artemis connection
        private IConnection? _connection { get; set; }
        // Active Artemis consumer
        private IConsumer? _consumer { get; set; }

        /// <summary>
        /// Gets the endpoint identifier.
        /// </summary>
        public string? IdEndpoint => _consumerConfiguration.IdEndpoint;

        /// <summary>
        /// Gets a value indicating whether the consumer is active.
        /// </summary>
        public bool IsActive => _consumerConfiguration.IsActive;

        /// <summary>
        /// Gets a value indicating whether the connection and consumer are active.
        /// </summary>
        public bool IsRunning => (_connection?.IsOpened ?? false) && _consumer != null;

        /// <summary>
        /// Gets the configured retry time.
        /// </summary>
        public int RetryTime => _consumerConfiguration.RetryTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseConsumer"/> class.
        /// </summary>
        /// <param name="consumerConfiguration">The consumer configuration.</param>
        /// <param name="connectionFactory">The connection factory.</param>
        public BaseConsumer(Models.ConsumerConfiguration consumerConfiguration, IArtemisClientConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _consumerConfiguration = consumerConfiguration;                        
        }

        /// <summary>
        /// Creates the Artemis connection and consumer.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        public async Task CreateConsumer()
        {
            if (IsRunning) return;            
            IEnumerable<Endpoint> endpoints = _consumerConfiguration.Endpoints!.Select(endpointConfig => Endpoint.Create(endpointConfig.Host, endpointConfig.Port, endpointConfig.User, endpointConfig.Password));
            _connection = await _connectionFactory.CreateAsync(endpoints);            
            _consumer = await _connection.CreateConsumerAsync(new ActiveMQ.Artemis.Client.ConsumerConfiguration
            {
                Address = _consumerConfiguration.Address,
                Queue = _consumerConfiguration.Queue,
                RoutingType = _consumerConfiguration.EnumRoutingType,
                Credit = _consumerConfiguration.Credit,
                Durable = _consumerConfiguration.Durable,
                FilterExpression = _consumerConfiguration.FilterExpression,
                NoLocalFilter = _consumerConfiguration.NoLocalFilter,
                Shared = _consumerConfiguration.Shared,
            });
        }

        /// <summary>
        /// Receives a message from the consumer and passes it to the abstract method.
        /// </summary>
        public async Task ReceiveMessage()
        {            
            await ReceiveMessage(await _consumer!.ReceiveAsync());
        }

        /// <summary>
        /// Abstract method to be implemented for handling the received message.
        /// </summary>
        /// <param name="message">The received message.</param>
        public abstract Task ReceiveMessage(Message message);

        /// <summary>
        /// Rejects the received message.
        /// </summary>
        /// <param name="message">The message to reject.</param>
        protected async ValueTask Reject(Message message)
        {            
            await Delivery(message, EnumDeliveryAction.Reject);            
        }

        /// <summary>
        /// Accepts the received message.
        /// </summary>
        /// <param name="message">The message to accept.</param>
        protected async ValueTask Accept(Message message)
        {
            await Delivery(message, EnumDeliveryAction.Accept);
        }

        /// <summary>
        /// Releases the received message, allowing it to be retried later (not increment delivery count).
        /// </summary>
        /// <param name="message">The message to release.</param>
        protected async ValueTask Release(Message message, bool undeliverableHere = false)
        {
            await Delivery(message, EnumDeliveryAction.Release, undeliverableHere);
        }

        /// <summary>
        /// Releases the received message, allowing it to be retried later (increment delivery count).
        /// </summary>
        /// <param name="message">The message to retry.</param>
        protected async ValueTask Retry(Message message, bool undeliverableHere = false)
        {
            await Delivery(message, EnumDeliveryAction.Retry, undeliverableHere);
        }

        /// <summary>
        /// Dispatches the delivery action to the broker.
        /// </summary>
        /// <param name="message">The message to deliver to the broker.</param>
        /// <param name="deliveryType">The delivery action to perform (Accept, Reject, Release, Retry).</param>
        /// <param name="undeliverableHere">If true, indicates the message is undeliverable on this consumer.</param>
        /// <returns>A completed ValueTask when the broker operation is finished.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="deliveryType"/> is not a supported value.</exception>        
        protected async ValueTask Delivery(Message message, EnumDeliveryAction deliveryType, bool undeliverableHere = false)
        {
            switch (deliveryType)
            {
                case EnumDeliveryAction.Accept:
                    await _consumer.AcceptAsync(message);
                    break;
                case EnumDeliveryAction.Reject:
                    _consumer!.Reject(message);
                    break;
                case EnumDeliveryAction.Release:
                    _consumer!.Modify(message, false, undeliverableHere);
                    break;
                case EnumDeliveryAction.Retry:
                    _consumer!.Modify(message, true, undeliverableHere);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deliveryType), deliveryType, "DeliveryType is not valid");
            }   
        }

        /// <summary>
        /// Releases the resources of the connection and the consumer.
        /// </summary>
        public async ValueTask DisposeAsync()
        {            
           if (_connection != null) await _connection.DisposeAsync();
           if (_consumer != null) await _consumer.DisposeAsync();
        }        
    }
}
