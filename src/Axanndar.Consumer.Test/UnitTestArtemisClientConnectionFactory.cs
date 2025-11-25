using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ActiveMQ.Artemis.Client;
using Axanndar.Consumer.Factory;
using Axanndar.Consumer.Factory.Interfaces;
using Moq;

namespace Axanndar.Consumer.Test
{
    public class UnitTestArtemisClientConnectionFactory
    {
        private readonly ArtemisClientConnectionFactory _factory;
        private const string TestEndpoint = "test-endpoint";

        public UnitTestArtemisClientConnectionFactory()
        {
            _factory = new ArtemisClientConnectionFactory(TestEndpoint);
        }

        [Fact]
        public void CanInstantiate_ArtemisClientConnectionFactory()
        {
            Assert.NotNull(_factory);
        }

        [Fact]
        public void IdEndpoint_IsSetCorrectly()
        {
            Assert.Equal(TestEndpoint, _factory.IdEndpoint);
        }
    }
}
