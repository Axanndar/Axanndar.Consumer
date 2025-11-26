using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Axanndar.Consumer.Logger.Interfaces;

namespace Axanndar.Consumer.Test
{
    public class UnitTestLoggerConsumerDefault
    {
        [Fact]
        public void LogTrace_CallsLoggerWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger<LoggerConsumerDefault>>();
            var loggerConsumer = new LoggerConsumerDefault(mockLogger.Object);
            loggerConsumer.LogTrace("corrId", "endpoint", $"Test msg");
            mockLogger.Verify(x => x.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CorrelationId: corrId") && v.ToString().Contains("Endpoint: endpoint") && v.ToString().Contains($"Test msg")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogInfo_CallsLoggerWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger<LoggerConsumerDefault>>();
            var loggerConsumer = new LoggerConsumerDefault(mockLogger.Object);
            loggerConsumer.LogInfo("corrId", "endpoint", "Info msg");
            mockLogger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CorrelationId: corrId") && v.ToString().Contains("Endpoint: endpoint") && v.ToString().Contains("Info msg")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }

        [Fact]
        public void LogError_CallsLoggerWithCorrectParameters()
        {
            var mockLogger = new Mock<ILogger<LoggerConsumerDefault>>();
            var loggerConsumer = new LoggerConsumerDefault(mockLogger.Object);
            var ex = new Exception("err");
            loggerConsumer.LogError("corrId", "endpoint", ex);
            mockLogger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CorrelationId: corrId") && v.ToString().Contains("Endpoint: endpoint") && v.ToString().Contains("err")),
                ex,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ), Times.Once);
        }
    }
}
