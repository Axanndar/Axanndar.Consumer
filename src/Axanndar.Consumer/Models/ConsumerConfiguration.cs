using ActiveMQ.Artemis.Client;
using Axanndar.Consumer.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Models
{
    /// <summary>
    /// Encapsulates all settings required to configure a consumer, including endpoint identification, retry logic, message filtering, durability, and endpoint collection.
    /// Used to initialize and control consumer behavior in the application.
    /// </summary>
    public class ConsumerConfiguration
    {
        /// <summary>
        /// The unique identifier for the endpoint.
        /// </summary>
        public string? IdEndpoint { get; set; }

        /// <summary>
        /// The retry time in milliseconds. Default is 5000 (5 seconds).
        /// </summary>
        public int RetryTime { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.RETRY_TIME;

        /// <summary>
        /// Indicates whether the consumer is active. Default is true.
        /// </summary>
        public bool IsActive { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.IS_ACTIVE;

        /// <summary>
        /// The address from which to consume messages.
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// The queue from which to consume messages.
        /// </summary>
        public string? Queue { get; set; }

        /// <summary>
        /// The routing type as an integer value.
        /// </summary>
        public int RoutingType { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.ROUTING_TYPE;

        /// <summary>
        /// The routing type as an enum value.
        /// </summary>
        public RoutingType EnumRoutingType { get => (RoutingType)RoutingType; }

        /// <summary>
        /// The credit value for the consumer.
        /// </summary>
        public int Credit { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.CREDIT;

        /// <summary>
        /// Indicates whether the consumer is durable.
        /// </summary>
        public bool Durable { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.DURABLE;

        /// <summary>
        /// The filter expression for message selection.
        /// </summary>
        public string? FilterExpression { get; set; }

        /// <summary>
        /// Indicates whether the NoLocal filter is enabled.
        /// </summary>
        public bool NoLocalFilter { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.NO_LOCAL_FILTER;

        /// <summary>
        /// Indicates whether the consumer is shared.
        /// </summary>
        public bool Shared { get; set; } = ConstAppConfiguration.DefaultValue.Amqp.SHARED;

        /// <summary>
        /// The collection of endpoint configurations.
        /// </summary>
        public IEnumerable<ConsumerConfigurationEndpoint>? Endpoints { get; set; }
    }

    /// <summary>
    /// Represents the network and authentication settings for a consumer endpoint, including host, port, user, and password.
    /// </summary>
    public class ConsumerConfigurationEndpoint
    {
        /// <summary>
        /// The host name or IP address of the endpoint.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// The port number of the endpoint.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The user name for the endpoint.
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// The password for the endpoint.
        /// </summary>
        public string? Password { get; set; }
    }
}
