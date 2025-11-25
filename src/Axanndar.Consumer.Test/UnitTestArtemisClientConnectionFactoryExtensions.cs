using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using ActiveMQ.Artemis.Client;
using Axanndar.Consumer.Factory.Interfaces;
using Axanndar.Consumer.Extensions;

namespace Axanndar.Consumer.Test
{
    public class UnitTestArtemisClientConnectionFactoryExtensions
    {
        [Fact]
        public async Task CreateAsync_WithEndpoints_CallsFactoryWithEndpointsAndNoCancellation()
        {
            var mockFactory = new Mock<IArtemisClientConnectionFactory>();
            var endpoints = new List<Endpoint> { Endpoint.Create("host", 1234, "user", "pass") };
            var mockConnection = new Mock<IConnection>().Object;
            mockFactory.Setup(f => f.CreateAsync(endpoints, CancellationToken.None)).ReturnsAsync(mockConnection);

            var result = await ArtemisClientConnectionFactoryExtensions.CreateAsync(mockFactory.Object, endpoints);
            Assert.Equal(mockConnection, result);
            mockFactory.Verify(f => f.CreateAsync(endpoints, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithSingleEndpointAndCancellationToken_CallsFactoryCorrectly()
        {
            var mockFactory = new Mock<IArtemisClientConnectionFactory>();
            var endpoint = Endpoint.Create("host", 1234, "user", "pass");
            var token = new CancellationTokenSource().Token;
            var mockConnection = new Mock<IConnection>().Object;
            mockFactory.Setup(f => f.CreateAsync(It.Is<IEnumerable<Endpoint>>(e => e != null && e.Contains(endpoint)), token)).ReturnsAsync(mockConnection);

            var result = await ArtemisClientConnectionFactoryExtensions.CreateAsync(mockFactory.Object, endpoint, token);
            mockFactory.Verify(f => f.CreateAsync(It.Is<IEnumerable<Endpoint>>(e => e != null && e.Contains(endpoint)), token), Times.Once);
            Assert.Equal(mockConnection, result);
        }

        [Fact]
        public async Task CreateAsync_WithSingleEndpoint_CallsFactoryWithNoCancellation()
        {
            var mockFactory = new Mock<IArtemisClientConnectionFactory>();
            var endpoint = Endpoint.Create("host", 1234, "user", "pass");
            var mockConnection = new Mock<IConnection>().Object;
            mockFactory.Setup(f => f.CreateAsync(It.Is<IEnumerable<Endpoint>>(e => e != null && e.Contains(endpoint)), CancellationToken.None)).ReturnsAsync(mockConnection);

            var result = await ArtemisClientConnectionFactoryExtensions.CreateAsync(mockFactory.Object, endpoint);
            mockFactory.Verify(f => f.CreateAsync(It.Is<IEnumerable<Endpoint>>(e => e != null && e.Contains(endpoint)), CancellationToken.None), Times.Once);
            Assert.Equal(mockConnection, result);
        }
    }
}
