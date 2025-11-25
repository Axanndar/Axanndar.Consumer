using ActiveMQ.Artemis.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Factory.Interfaces
{
    public interface IArtemisClientConnectionFactory
    {
        public string IdEndpoint { get; }
        Task<IConnection> CreateAsync(IEnumerable<Endpoint> endpoints, CancellationToken cancellationToken);
    }
}
