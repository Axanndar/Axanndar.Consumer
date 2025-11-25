using ActiveMQ.Artemis.Client.AutoRecovering.RecoveryPolicy;
using ActiveMQ.Artemis.Client.MessageIdPolicy;
using Axanndar.Consumer.Constants;
using Axanndar.Consumer.Exceptions;
using Axanndar.Consumer.Factory;
using Axanndar.Consumer.Factory.Interfaces;
using Axanndar.Consumer.Interfaces;
using Axanndar.Consumer.Logger;
using Axanndar.Consumer.Logger.Interfaces;
using Axanndar.Consumer.Models;
using Axanndar.Consumer.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Axanndar.Consumer.Extensions
{
    /// <summary>
    /// Provides extension methods for registering consumer background services and related Artemis messaging components
    /// into the dependency injection container. Supports flexible configuration, multiple consumer instances, and custom logger implementations.
    /// This class centralizes the setup logic for consumers, connection factories, recovery policies, and logging, enabling
    /// scalable and maintainable registration of messaging infrastructure in .NET applications.
    /// </summary>
    public static class ServiceRegistration
    {
        /// <summary>
        /// Registers a consumer of type <typeparamref name="TConsumer"/> with the specified configuration and optional Artemis connection settings.
        /// Allows specifying a custom logger consumer implementation.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer, derived from <see cref="BaseConsumer"/>.</typeparam>
        /// <typeparam name="TLoggerConsumer">The logger consumer implementation, derived from <see cref="ILoggerConsumer"/>.</typeparam>
        /// <param name="services">The service collection to add the consumer and related services to.</param>
        /// <param name="consumerConfiguration">The consumer configuration instance for the specific endpoint.</param>
        /// <param name="automaticRecoveryEnabled">Whether automatic recovery is enabled for the Artemis connection factory.</param>
        /// <param name="recoveryPolicy">The recovery policy to use for the Artemis connection factory.</param>
        /// <param name="loggerFactory">The logger factory to use for Artemis logging.</param>
        /// <param name="messageIdPolicyFactory">The message ID policy factory for Artemis.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddConsumerBackgroundService<TConsumer, TLoggerConsumer>(this IServiceCollection services, Models.ConsumerConfiguration consumerConfiguration, bool automaticRecoveryEnabled = true, IRecoveryPolicy? recoveryPolicy = null, ILoggerFactory? loggerFactory = null, Func<IMessageIdPolicy>? messageIdPolicyFactory = null) where TConsumer : BaseConsumer where TLoggerConsumer : class, ILoggerConsumer
        {
            // Set default recovery policy if not provided
            recoveryPolicy = recoveryPolicy ?? RecoveryPolicyFactory.ExponentialBackoff(initialDelay: TimeSpan.FromMicroseconds(1000), fastFirst: true);
            // Set default logger factory if not provided
            loggerFactory = loggerFactory ?? new NullLoggerFactory();
            // Set default message ID policy factory if not provided
            messageIdPolicyFactory = messageIdPolicyFactory ?? ActiveMQ.Artemis.Client.MessageIdPolicy.MessageIdPolicyFactory.DisableMessageIdPolicy;

            services.TryAddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();
            services.TryAddSingleton<ILoggerConsumer, TLoggerConsumer>();
            services.AddSingleton(consumerConfiguration);
            services.AddSingleton<IArtemisClientConnectionFactory, ArtemisClientConnectionFactory>(_ => new ArtemisClientConnectionFactory(consumerConfiguration.IdEndpoint!)
            {
                AutomaticRecoveryEnabled = automaticRecoveryEnabled,
                ClientIdFactory = () => $"{consumerConfiguration.IdEndpoint}_{Guid.NewGuid().ToString()}",
                RecoveryPolicy = recoveryPolicy,
                LoggerFactory = loggerFactory,
                MessageIdPolicyFactory = messageIdPolicyFactory
            });           
            services.AddSingleton<TConsumer>(servceProvider =>
            {
                Models.ConsumerConfiguration localConsumerConfiguration = servceProvider.GetServices<Models.ConsumerConfiguration>().Single(x => x.IdEndpoint == consumerConfiguration.IdEndpoint);
                IArtemisClientConnectionFactory localArtemisClientConnectionFactory = servceProvider.GetServices<IArtemisClientConnectionFactory>().Single(x => x.IdEndpoint == consumerConfiguration.IdEndpoint);
                return ActivatorUtilities.CreateInstance<TConsumer>(servceProvider, localConsumerConfiguration, localArtemisClientConnectionFactory)!;
            });
            services.AddHostedService<ConsumerBackgroundService<TConsumer>>();
            return services;
        }

        /// <summary>
        /// Registers a consumer of type <typeparamref name="TConsumer"/> with the specified configuration and optional Artemis connection settings.
        /// Uses <see cref="LoggerConsumerDefault"/> as the default logger consumer type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer, derived from <see cref="BaseConsumer"/>.</typeparam>
        /// <param name="services">The service collection to add the consumer and related services to.</param>
        /// <param name="consumerConfiguration">The consumer configuration instance for the specific endpoint.</param>
        /// <param name="automaticRecoveryEnabled">Whether automatic recovery is enabled for the Artemis connection factory.</param>
        /// <param name="recoveryPolicy">The recovery policy to use for the Artemis connection factory.</param>
        /// <param name="loggerFactory">The logger factory to use for Artemis logging.</param>
        /// <param name="messageIdPolicyFactory">The message ID policy factory for Artemis.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddConsumerBackgroundService<TConsumer>(this IServiceCollection services, Models.ConsumerConfiguration consumerConfiguration, bool automaticRecoveryEnabled = true, IRecoveryPolicy? recoveryPolicy = null, ILoggerFactory? loggerFactory = null, Func<IMessageIdPolicy>? messageIdPolicyFactory = null) where TConsumer : BaseConsumer
        {
            services.AddConsumerBackgroundService<TConsumer, LoggerConsumerDefault>(consumerConfiguration, automaticRecoveryEnabled, recoveryPolicy, loggerFactory, messageIdPolicyFactory);
            return services;
        }

        /// <summary>
        /// Registers a consumer of type <typeparamref name="TConsumer"/> using configuration from the specified section and endpoint identifier.
        /// Allows specifying a custom logger consumer implementation.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer, derived from <see cref="BaseConsumer"/>.</typeparam>
        /// <typeparam name="TLoggerConsumer">The logger consumer implementation, derived from <see cref="ILoggerConsumer"/>.</typeparam>
        /// <param name="services">The service collection to add the consumer and related services to.</param>
        /// <param name="idEndpoint">The endpoint identifier for the consumer configuration section.</param>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="automaticRecoveryEnabled">Whether automatic recovery is enabled for the Artemis connection factory.</param>
        /// <param name="recoveryPolicy">The recovery policy to use for the Artemis connection factory.</param>
        /// <param name="loggerFactory">The logger factory to use for Artemis logging.</param>
        /// <param name="messageIdPolicyFactory">The message ID policy factory for Artemis.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddConsumerBackgroundService<TConsumer, TLoggerConsumer>(this IServiceCollection services, string idEndpoint, IConfiguration configuration, bool automaticRecoveryEnabled = true, IRecoveryPolicy? recoveryPolicy = null, ILoggerFactory? loggerFactory = null, Func<IMessageIdPolicy>? messageIdPolicyFactory = null) where TConsumer : BaseConsumer where TLoggerConsumer : class, ILoggerConsumer
        {            
            services.AddConsumerBackgroundService<TConsumer, TLoggerConsumer>(GetConsumerConfiguration(configuration, idEndpoint)!, automaticRecoveryEnabled, recoveryPolicy, loggerFactory, messageIdPolicyFactory);
            return services;
        }

        /// <summary>
        /// Registers a consumer of type <typeparamref name="TConsumer"/> using configuration from the specified section and endpoint identifier.
        /// Uses <see cref="LoggerConsumerDefault"/> as the default logger consumer type.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer, derived from <see cref="BaseConsumer"/>.</typeparam>
        /// <param name="services">The service collection to add the consumer and related services to.</param>
        /// <param name="idEndpoint">The endpoint identifier for the consumer configuration section.</param>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="automaticRecoveryEnabled">Whether automatic recovery is enabled for the Artemis connection factory.</param>
        /// <param name="recoveryPolicy">The recovery policy to use for the Artemis connection factory.</param>
        /// <param name="loggerFactory">The logger factory to use for Artemis logging.</param>
        /// <param name="messageIdPolicyFactory">The message ID policy factory for Artemis.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddConsumerBackgroundService<TConsumer>(this IServiceCollection services, string idEndpoint, IConfiguration configuration, bool automaticRecoveryEnabled = true, IRecoveryPolicy? recoveryPolicy = null, ILoggerFactory? loggerFactory = null, Func<IMessageIdPolicy>? messageIdPolicyFactory = null) where TConsumer : BaseConsumer
        {            
            services.AddConsumerBackgroundService<TConsumer>(
                GetConsumerConfiguration(configuration, idEndpoint)!,
                automaticRecoveryEnabled,
                recoveryPolicy,
                loggerFactory,
                messageIdPolicyFactory
            );
            return services;
        }

        /// <summary>
        /// Helper method to retrieve the <see cref="ConsumerConfiguration"/> for a specific endpoint from the configuration.
        /// Throws <see cref="ConsumerWorkerException"/> if not found.
        /// </summary>
        /// <param name="configuration">The application configuration instance.</param>
        /// <param name="idEndpoint">The endpoint identifier.</param>
        /// <returns>The <see cref="ConsumerConfiguration"/> for the specified endpoint.</returns>
        private static Models.ConsumerConfiguration? GetConsumerConfiguration(IConfiguration configuration, string idEndpoint)
        {
            Models.ConsumerConfiguration consumerConfig = configuration.GetSection(ConstAppConfiguration.PropertyName.AMQP).GetSection(idEndpoint).Get<Models.ConsumerConfiguration>() ?? throw new ConsumerWorkerException($"Consumer configuration for endpoint {idEndpoint} not found.");
            consumerConfig.IdEndpoint = idEndpoint;
            return consumerConfig;
        }
    }
}
