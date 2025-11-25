using ActiveMQ.Artemis.Client;
using Axanndar.Consumer.Factory.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Factory
{
    public class ArtemisClientConnectionFactory : ConnectionFactory, IArtemisClientConnectionFactory
    {
        public string IdEndpoint { get; private set; }

        public ArtemisClientConnectionFactory(string idEndpoint) : base() => IdEndpoint = idEndpoint;

    }
}
