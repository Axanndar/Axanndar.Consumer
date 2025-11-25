using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Axanndar.Consumer.Logger.Interfaces
{
    /// <summary>
    /// Defines logging methods for consumer operations, supporting trace, info, and error levels with correlation and endpoint context.
    /// </summary>
    public interface ILoggerConsumer
    {        
        /// <summary>
        /// Logs a trace-level message with correlation and endpoint information.
        /// </summary>
        /// <param name="correlationId">The correlation ID for tracking the log entry.</param>
        /// <param name="IdEndpoint">The endpoint identifier related to the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional arguments for message formatting.</param>
        void LogTrace(string correlationId, string IdEndpoint, string message, params object[] args);

        /// <summary>
        /// Logs an informational message with correlation and endpoint information.
        /// </summary>
        /// <param name="correlationId">The correlation ID for tracking the log entry.</param>
        /// <param name="IdEndpoint">The endpoint identifier related to the log entry.</param>
        /// <param name="message">The log message format string.</param>
        /// <param name="args">Optional arguments for message formatting.</param>
        void LogInfo(string correlationId, string IdEndpoint, string message, params object[] args);

        /// <summary>
        /// Logs an error with correlation and endpoint information.
        /// </summary>
        /// <param name="correlationId">The correlation ID for tracking the log entry.</param>
        /// <param name="IdEndpoint">The endpoint identifier related to the log entry.</param>
        /// <param name="ex">The exception to log.</param>
        void LogError(string correlationId, string IdEndpoint, Exception ex);
    }

    /// <summary>
    /// Default implementation of <see cref="ILoggerConsumer"/> using Microsoft.Extensions.Logging.
    /// Formats log messages with correlation ID, endpoint, and log level.
    /// </summary>
    /// <param name="_logger">The logger instance used for logging.</param>
    public class LoggerConsumerDefault(ILogger<LoggerConsumerDefault> _logger) : ILoggerConsumer
    {
        /// <inheritdoc/>
        public void LogTrace(string correlationId, string IdEndpoint, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.LogTrace("CorrelationId: {correlationId} | LogLevel: Trace | Endpoint: {IdEndpoint} | Message: {formattedMessage}", correlationId, IdEndpoint, formattedMessage);
        }
        /// <inheritdoc/>
        public void LogInfo(string correlationId, string IdEndpoint, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            _logger.LogInformation("CorrelationId: {correlationId} | LogLevel: Information | Endpoint: {IdEndpoint} | Message: {formattedMessage}", correlationId, IdEndpoint, formattedMessage);
        }
        /// <inheritdoc/>
        public void LogError(string correlationId, string IdEndpoint, Exception ex)
        {
            string formattedMessage = ex.ToString();
            _logger.LogError(ex, "CorrelationId: {correlationId} | LogLevel: Error | Endpoint: {IdEndpoint} | Message: {formattedMessage}", correlationId, IdEndpoint, formattedMessage);
        }
    }
}
