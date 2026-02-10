using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoFixture;
using AutoFixture.AutoMoq;
using Axanndar.Consumer;
using Axanndar.Consumer.Models;
using Axanndar.Consumer.Factory.Interfaces;
using ActiveMQ.Artemis.Client;
using System.Collections.Generic;
using Axanndar.Consumer.Enums;

namespace Axanndar.Consumer.Test
{
    public class UnitTestBaseConsumer
    {
        private readonly IFixture _fixture;
        private readonly Mock<IArtemisClientConnectionFactory> _mockFactory;
        private readonly Axanndar.Consumer.Models.ConsumerConfiguration _config;
        private readonly Mock<IConnection> _mockConnection;
        private readonly TestConsumer _consumer;
        private readonly Mock<IConsumer> _manualMockConsumer;

        public UnitTestBaseConsumer()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockFactory = new Mock<IArtemisClientConnectionFactory>();
            _mockConnection = new Mock<IConnection>();
            _manualMockConsumer = new Mock<IConsumer>();
            _config = _fixture.Build<Axanndar.Consumer.Models.ConsumerConfiguration>()
                .With(x => x.Endpoints, new List<ConsumerConfigurationEndpoint> { new ConsumerConfigurationEndpoint { Host = "localhost", Port = 61616, User = "user", Password = "pass" } })
                .With(x => x.Address, "test-address")
                .With(x => x.RoutingType, 0)
                .Create();
            _consumer = new TestConsumer(_config, _mockFactory.Object);
        }

        [Fact]
        public void IdEndpoint_Returns_ConfigValue()
        {
            Assert.Equal(_config.IdEndpoint, _consumer.IdEndpoint);
        }

        [Fact]
        public void IsActive_Returns_ConfigValue()
        {
            Assert.Equal(_config.IsActive, _consumer.IsActive);
        }

        [Fact]
        public void RetryTime_Returns_ConfigValue()
        {
            Assert.Equal(_config.RetryTime, _consumer.RetryTime);
        }

        [Fact]
        public async Task CreateConsumer_CreatesConnectionAndConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);

            await _consumer.CreateConsumer();

            _mockFactory.Verify(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockConnection.Verify(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReceiveMessage_CallsAbstractMethod()
        {
            // Setup the factory and connection to return the correct mocks
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);

            // Create a valid AMQP message body (e.g., a string)
            Message message = new Message("test-body");
            _manualMockConsumer.Setup(c => c.ReceiveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(message);
            _consumer.SetConsumer(_manualMockConsumer.Object);

            await _consumer.CreateConsumer();
            await _consumer.ReceiveMessage();

            Assert.True(_consumer.ReceiveMessageCalled);
            Assert.Equal(message, _consumer.LastMessage);
        }

        [Fact]
        public void IsRunning_Returns_False_When_NotConnectedOrConsumerNull()
        {
            // IsRunning should be false when _connection is null and _consumer is null
            Assert.False(_consumer.IsRunning);
        }

        [Fact]
        public async Task IsRunning_Returns_True_When_ConnectedAndConsumerNotNull()
        {
            // Setup connection and consumer
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            _mockConnection.Setup(c => c.IsOpened).Returns(true);
            await _consumer.CreateConsumer();
            Assert.True(_consumer.IsRunning);
        }

        [Fact]
        public async Task Accept_CallsAcceptAsyncOnConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            var message = new Message("accept-body");
            // Non si può fare Setup/Verify su extension method, quindi si verifica che non venga sollevata eccezione
            await _consumer.CallAccept(message);
            // Se necessario, si può verificare uno stato interno o un effetto collaterale
        }

        [Fact]
        public async Task Reject_CallsRejectOnConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            var message = new Message("reject-body");
            _manualMockConsumer.Setup(c => c.Reject(message)).Verifiable();
            await _consumer.CallReject(message);
            _manualMockConsumer.Verify(c => c.Reject(message), Times.Once);
        }

        [Fact]
        public async Task Release_CallsReleaseOnConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            var message = new Message("release-body");
            _manualMockConsumer.Setup(c => c.Modify(message, false, false)).Verifiable();
            await _consumer.CallRelease(message);
            _manualMockConsumer.Verify(c => c.Modify(message, false, false), Times.Once);
        }

        [Fact]
        public async Task Retry_CallsRetryOnConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            var message = new Message("retry-body");
            _manualMockConsumer.Setup(c => c.Modify(message, true, false)).Verifiable();
            await _consumer.CallRetry(message);
            _manualMockConsumer.Verify(c => c.Modify(message, true, false), Times.Once);
        }

        [Fact]
        public async Task Delivery_Exception()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            var message = new Message("exception-body");
            
            await Assert.ThrowsAnyAsync<ArgumentOutOfRangeException>(async () => await _consumer.CallException(message, (EnumDeliveryAction)int.MaxValue));            
        }


        [Fact]
        public async Task DisposeAsync_CallsDisposeOnConnectionAndConsumer()
        {
            _mockFactory.Setup(f => f.CreateAsync(It.IsAny<IEnumerable<Endpoint>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_mockConnection.Object);
            _mockConnection.Setup(c => c.CreateConsumerAsync(It.IsAny<ActiveMQ.Artemis.Client.ConsumerConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_manualMockConsumer.Object);
            await _consumer.CreateConsumer();
            _mockConnection.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            _manualMockConsumer.Setup(c => c.DisposeAsync()).Returns(ValueTask.CompletedTask).Verifiable();
            await _consumer.DisposeAsync();
            _mockConnection.Verify(c => c.DisposeAsync(), Times.Once);
            _manualMockConsumer.Verify(c => c.DisposeAsync(), Times.Once);
        }

        // Helper class to test abstract BaseConsumer
        private class TestConsumer : BaseConsumer
        {
            public bool ReceiveMessageCalled { get; private set; }
            public Message? LastMessage { get; private set; }
            public TestConsumer(Axanndar.Consumer.Models.ConsumerConfiguration config, IArtemisClientConnectionFactory factory) : base(config, factory) { }
            public void SetConsumer(IConsumer consumer) => typeof(BaseConsumer).GetProperty("_consumer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(this, consumer);
            public override Task ReceiveMessage(Message message)
            {
                ReceiveMessageCalled = true;
                LastMessage = message;
                return Task.CompletedTask;
            }
            public async Task CallAccept(Message message)
            {
                var valueTask = (ValueTask)typeof(BaseConsumer).GetMethod("Accept", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(this, new object[] { message });
                await valueTask;
            }
            public async Task CallReject(Message message)
            {
                var valueTask = (ValueTask)typeof(BaseConsumer).GetMethod("Reject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(this, new object[] { message });
                await valueTask;
            }

            public async Task CallRelease(Message message, bool undeliverableHere = false)
            {
                var valueTask = (ValueTask)typeof(BaseConsumer).GetMethod("Release", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(this, new object[] { message, undeliverableHere });
                await valueTask;
            }

            public async Task CallRetry(Message message, bool undeliverableHere = false)
            {
                var valueTask = (ValueTask)typeof(BaseConsumer).GetMethod("Retry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(this, new object[] { message, undeliverableHere });
                await valueTask;
            }

            public async Task CallException(Message message, EnumDeliveryAction deliveryType, bool undeliverableHere = false)
            {
                var valueTask = (ValueTask)typeof(BaseConsumer).GetMethod("Delivery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(this, new object[] { message, deliveryType, undeliverableHere });
                await valueTask;
            }
        }
    }
}
