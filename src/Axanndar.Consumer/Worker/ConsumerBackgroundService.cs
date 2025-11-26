using Axanndar.Consumer;
using Axanndar.Consumer.Interfaces;
using Axanndar.Consumer.Logger.Interfaces;
using Axanndar.Consumer.Worker.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Axanndar.Consumer.Worker
{
    /// <summary>
    /// Generic background service that manages the lifecycle of a consumer.
    /// Handles consumer creation, message reception, error logging, and retry logic.
    /// </summary>
    /// <typeparam name="TConsumer">Type of consumer to manage, must derive from <see cref="BaseConsumer"/>.</typeparam>
    public class ConsumerBackgroundService<TConsumer> : BackgroundService, IConsumerBackgroundService where TConsumer : BaseConsumer
    {
        private readonly BaseConsumer _consumer;
        private readonly ICorrelationIdProvider _correlationIdProvider;
        private readonly ILoggerConsumer _logger;        

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumerBackgroundService{TConsumer}"/> class.
        /// </summary>
        /// <param name="consumer">Instance of the consumer to be managed.</param>
        /// <param name="logger">Logger for tracing and error handling.</param>
        /// <param name="correlationIdProvider">Provider for generating and managing the CorrelationId.</param>
        public ConsumerBackgroundService(TConsumer consumer, ILoggerConsumer logger, ICorrelationIdProvider correlationIdProvider)
        {
            _consumer = consumer;
            _logger = logger;
            _correlationIdProvider = correlationIdProvider;
        }

        /// <summary>
        /// Executes the background service logic:
        /// - Checks if the consumer is active
        /// - Creates the consumer and starts receiving messages
        /// - Handles errors and retries in case of exceptions
        /// - Updates the CorrelationId for each received message
        /// </summary>
        /// <param name="stoppingToken">Token for service cancellation.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int retryTime = _consumer.RetryTime;
            if (!_consumer.IsActive)
            {
                _logger.LogInfo(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, "ConsumerBackgroundService is not active");
                return;
            }            
            // Main loop: keeps the background service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _consumer.CreateConsumer();
                    _logger.LogInfo(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, $"Consumer created");
                    try
                    {
                        await using (_consumer)
                        {
                            // Inner loop: processes messages as long as the consumer is running and not cancelled
                            while (!stoppingToken.IsCancellationRequested && _consumer.IsRunning)
                            {
                                await _consumer.ReceiveMessage();
                                // Generate a new correlation ID for each message processed
                                _correlationIdProvider.NewCorrelationId();
                            }

                            if (!_consumer.IsRunning) 
                            {
                              _logger.LogInfo(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, "Consumer has stopped running");
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, ex);                        
                        _logger.LogTrace(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, $"Retry on {retryTime}");
                        // Wait before retrying if an error occurs during message processing
                        await Task.Delay(TimeSpan.FromMilliseconds(retryTime));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, ex);
                    _logger.LogTrace(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, $"Retry on {retryTime}");
                    // Wait before retrying if an error occurs during consumer creation
                    await Task.Delay(TimeSpan.FromMilliseconds(retryTime));
                }

                _logger.LogInfo(_correlationIdProvider.CorrelationId!, _consumer.IdEndpoint!, "ConsumerBackgroundService is terminated");
            }
        }
    }
}
