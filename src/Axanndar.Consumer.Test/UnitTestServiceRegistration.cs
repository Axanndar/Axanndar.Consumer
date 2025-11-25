using AutoFixture;
using Axanndar.Consumer.Exceptions;
using Axanndar.Consumer.Extensions;
using Axanndar.Consumer.Factory;
using Axanndar.Consumer.Factory.Interfaces;
using Axanndar.Consumer.Logger.Interfaces;
using Axanndar.Consumer.Models;
using Axanndar.Consumer.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Axanndar.Consumer.Test
{
    public class DummyConsumer : BaseConsumer
    {
        public DummyConsumer(ConsumerConfiguration config, IArtemisClientConnectionFactory factory) : base(config, factory) { }
        public override Task ReceiveMessage(ActiveMQ.Artemis.Client.Message message) => Task.CompletedTask;
    }

    public class UnitTestServiceRegistration
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void AddConsumerBackgroundService_WithConfig_RegistersAllServices()
        {
            var services = new ServiceCollection();
            var config = _fixture.Build<ConsumerConfiguration>()
                .With(x => x.IdEndpoint, "test-endpoint")
                .With(x => x.IsActive, true)
                .Create();
            services.AddLogging();
            services.AddConsumerBackgroundService<DummyConsumer>(config);
            var provider = services.BuildServiceProvider();

            // Verifica consumer
            var consumer = provider.GetService<DummyConsumer>();
            Assert.NotNull(consumer);
            Assert.Equal("test-endpoint", consumer.IdEndpoint);
            // Verifica factory
            var factory = provider.GetService<IArtemisClientConnectionFactory>();
            Assert.NotNull(factory);
            // Verifica logger
            var logger = provider.GetService<ILoggerConsumer>();
            Assert.NotNull(logger);
            // Verifica correlation provider
            var correlation = provider.GetService<ICorrelationIdProvider>();
            Assert.NotNull(correlation);
            // Verifica hosted service
            var hosted = provider.GetService<IHostedService>();
            Assert.NotNull(hosted);
        }

        [Fact]
        public void AddConsumerBackgroundService_WithSection_RegistersAllServices()
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                //{"AMQP:test:IdEndpoint", "test"},
                {"Amqp:test:IsActive", "true"},
                {"Amqp:test:RetryTime", "5000"}
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddConsumerBackgroundService<DummyConsumer>("test", config);            
            ServiceProvider provider = services.BuildServiceProvider();

            DummyConsumer? consumer = provider.GetService<DummyConsumer>();
            Assert.NotNull(consumer);
            Assert.Equal("test", consumer.IdEndpoint);
            Assert.NotNull(provider.GetService<IArtemisClientConnectionFactory>());
            Assert.NotNull(((ArtemisClientConnectionFactory)provider.GetService<IArtemisClientConnectionFactory>()!).ClientIdFactory());
            Assert.NotNull(provider.GetService<ILoggerConsumer>());
            Assert.NotNull(provider.GetService<ICorrelationIdProvider>());
            Assert.NotNull(provider.GetService<IHostedService>());
        }

        [Fact]
        public void AddConsumerBackgroundService_ThrowsException_IfConfigMissing()
        {
            var inMemorySettings = new Dictionary<string, string?>(); // vuoto
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            var services = new ServiceCollection();
            Assert.Throws<ConsumerWorkerException>(() =>
                services.AddConsumerBackgroundService<DummyConsumer>("missing", config)
            );
        }
    }
}
