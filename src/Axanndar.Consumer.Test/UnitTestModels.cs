using Xunit;
using Axanndar.Consumer.Models;
using System.Collections.Generic;

namespace Axanndar.Consumer.Test
{
    public class UnitTestModels
    {
        [Fact]
        public void ConsumerConfiguration_DefaultValues_AreCorrect()
        {
            var config = new ConsumerConfiguration();
            Assert.Null(config.IdEndpoint);
            Assert.Equal(5000, config.RetryTime); // Corretto da 5 a 5000
            Assert.True(config.IsActive);
            Assert.Null(config.Address);
            Assert.Equal(0, config.RoutingType);
            Assert.Null(config.Endpoints);
        }

        [Fact]
        public void ConsumerConfiguration_EnumRoutingType_ReturnsEnum()
        {
            ConsumerConfiguration config = new ConsumerConfiguration { RoutingType = (int)ActiveMQ.Artemis.Client.RoutingType.Anycast };
            Assert.Equal(ActiveMQ.Artemis.Client.RoutingType.Anycast, config.EnumRoutingType);
        }

        [Fact]
        public void ConsumerConfigurationEndpoint_Properties_SetAndGet()
        {
            ConsumerConfigurationEndpoint endpoint = new ConsumerConfigurationEndpoint
            {
                Host = "localhost",
                Port = 1234,
                User = "user",
                Password = "pass"
            };
            Assert.Equal("localhost", endpoint.Host);
            Assert.Equal(1234, endpoint.Port);
            Assert.Equal("user", endpoint.User);
            Assert.Equal("pass", endpoint.Password);
        }

        [Fact]
        public void ConsumerConfiguration_CanSetEndpoints()
        {
            List<ConsumerConfigurationEndpoint> endpoints = new List<ConsumerConfigurationEndpoint>
            {
                new ConsumerConfigurationEndpoint { Host = "host1", Port = 1 },
                new ConsumerConfigurationEndpoint { Host = "host2", Port = 2 }
            };
            ConsumerConfiguration config = new ConsumerConfiguration { Endpoints = endpoints };
            Assert.Equal(endpoints, config.Endpoints);
        }
    }
}
