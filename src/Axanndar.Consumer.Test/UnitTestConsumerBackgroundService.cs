using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Axanndar.Consumer.Worker;
using Axanndar.Consumer;
using Axanndar.Consumer.Models;
using Axanndar.Consumer.Factory.Interfaces;
using System.Collections.Generic;
using ActiveMQ.Artemis.Client;
using AutoFixture;
using Axanndar.Consumer.Logger.Interfaces;
using Axanndar.Consumer.Worker.Interfaces;

namespace Axanndar.Consumer.Test
{
    public class UnitTestConsumerBackgroundService
    {
        private readonly Fixture _fixture = new Fixture();
        private readonly Mock<IArtemisClientConnectionFactory> _mockFactory;
        private readonly Mock<IConnection> _mockConnection;
        private readonly Mock<IConsumer> _mockConsumer;
        private Mock<BaseConsumer> _mockBaseConsumer;
        private Mock<ILoggerConsumer> _mockLogger;
        private Mock<ICorrelationIdProvider> _mockCorrelationIdProvider;
        private ConsumerBackgroundService<BaseConsumer> _service;

        public UnitTestConsumerBackgroundService()
        {
            _mockFactory = new Mock<IArtemisClientConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _mockConsumer = new Mock<IConsumer>();
            _mockBaseConsumer = null!;
            _mockLogger = new Mock<ILoggerConsumer>();
            _mockCorrelationIdProvider = new Mock<ICorrelationIdProvider>();
            _service = null!;
        }

        private void CreateService(Models.ConsumerConfiguration config)
        {
            _mockBaseConsumer = new Mock<BaseConsumer>(config, _mockFactory.Object);
            _service = new ConsumerBackgroundService<BaseConsumer>(
                _mockBaseConsumer.Object,
                _mockLogger.Object,
                _mockCorrelationIdProvider.Object
            );
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotRun_WhenConsumerIsNotActive()
        {
            CreateService(_fixture.Build<Models.ConsumerConfiguration>()
                .With(x => x.IsActive, false)
                .With(x => x.RetryTime, 0)
                .Create());
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<ActiveMQ.Artemis.Client.Endpoint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConsumer.Object);

            // Should return immediately if not active
            await _service.StartAsync(CancellationToken.None);
            await Task.Delay(200);
            _mockConnection.Verify(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_CallsCreateConsumerAndReceiveMessage_WhenActiveAndRunning()
        {
            CreateService(_fixture.Build<Models.ConsumerConfiguration>()
                .With(x => x.IsActive, true)
                .With(x => x.RetryTime, 0)
                .Create());
            _mockConnection.SetupGet(x => x.IsOpened).Returns(true);
            _mockConnection.Setup(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConsumer.Object);
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<ActiveMQ.Artemis.Client.Endpoint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);

            CancellationTokenSource cts = new CancellationTokenSource(200);

            await _service.StartAsync(cts.Token);
            await Task.Delay(200);
            _mockConnection.Verify(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockBaseConsumer.Verify(c => c.ReceiveMessage(It.IsAny<Message>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_CallsCreateConsumerAndReceiveMessage_WhenActiveAndNotRunning()
        {
            CreateService(_fixture.Build<Models.ConsumerConfiguration>()
                .With(x => x.IsActive, true)
                .With(x => x.RetryTime, 0)
                .Create());
            _mockConnection.SetupGet(x => x.IsOpened).Returns(false);
            _mockConnection.Setup(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConsumer.Object);
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<ActiveMQ.Artemis.Client.Endpoint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);

            CancellationTokenSource cts = new CancellationTokenSource(200);

            await _service.StartAsync(cts.Token);
            await Task.Delay(200);
            _mockConnection.Verify(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockBaseConsumer.Verify(c => c.ReceiveMessage(It.IsAny<Message>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_CallsCreateConsumerAndReceiveMessage_WhenActiveARunning_Exception()
        {
            CreateService(_fixture.Build<Models.ConsumerConfiguration>()
                .With(x => x.IsActive, true)
                .With(x => x.RetryTime, 0)
                .Create());
            _mockConnection.SetupGet(x => x.IsOpened).Returns(true);
            _mockConnection.Setup(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConsumer.Object);
            _mockBaseConsumer.Setup(x => x.ReceiveMessage(It.IsAny<Message>())).ThrowsAsync(new Exception());
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<ActiveMQ.Artemis.Client.Endpoint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);

            CancellationTokenSource cts = new CancellationTokenSource(200);

            await _service.StartAsync(cts.Token);
            await Task.Delay(200);
            _mockConnection.Verify(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockBaseConsumer.Verify(c => c.ReceiveMessage(It.IsAny<Message>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ExecuteAsync_WaitsOnException()
        {            
            CreateService(_fixture.Build<Models.ConsumerConfiguration>()
               .With(x => x.IsActive, true)
               .With(x => x.RetryTime, 0)
               .Create());
            _mockConnection.SetupGet(x => x.IsOpened).Returns(true);
            _mockConnection.Setup(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("fail"));
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<ActiveMQ.Artemis.Client.Endpoint>>(), It.IsAny<CancellationToken>())).ReturnsAsync(_mockConnection.Object);

            CancellationTokenSource cts = new CancellationTokenSource(100);

            await _service.StartAsync(cts.Token);
            await Task.Delay(200);
            _mockConnection.Verify(x => x.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }
    }
}
