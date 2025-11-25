[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
<!-- [![Nuget](https://img.shields.io/nuget/v/Axanndar.Consumer?style=plastic)](https://www.nuget.org/packages/Axanndar.Consumer) -->
<!-- ![NuGet Downloads](https://img.shields.io/nuget/dt/Axanndar.Consumer) -->
[![issues - Axanndar.Consumer](https://img.shields.io/github/issues/Axanndar/Axanndar.Consumer)](https://github.com/Axanndar/Axanndar.Consumer/issues) 

# Axanndar.Consumer
This .NET library adds an abstraction layer over ActiveMQ Artemis, handling connection creation, consumer instantiation, and automatic re-creation in case of failures. It provides tools to easily implement new consumers by extending a base class (`BaseConsumer`), and facilitates dependency injection of consumers via configuration (`appsettings.json`) or code.

## Create new Consumer

```csharp
    public class FooConsumer : BaseConsumer
    {
        public FooConsumer(Axanndar.Consumer.Models.ConsumerConfiguration consumerConfiguration, IArtemisClientConnectionFactory connectionFactory) : base(consumerConfiguration, connectionFactory)
        {
        }

        public override async Task ReceiveMessage(Message message)
        {
            Console.WriteLine($"FooConsumer - Received message with ID: {message.MessageId}");
            await Accept(message);
        }
    }

    public class DummyConsumer : BaseConsumer
    {
        public DummyConsumer(Axanndar.Consumer.Models.ConsumerConfiguration consumerConfiguration, IArtemisClientConnectionFactory connectionFactory) : base(consumerConfiguration, connectionFactory)
        {
        }

        public override async Task ReceiveMessage(Message message)
        {
            Console.WriteLine($"FooConsumer - Received message with ID: {message.MessageId}");
            await Reject(message);
        }
    }
```

## Service registration

### `appsettings.json`

```json
{
  "Amqp": {
    "FooConsumer": {
      "RetryTime": 5000,
      "IsActive": true,
      "Address": "my-address",
      "Queue": "my-queue",
      "RoutingType": 0,
      "Credit": 200,
      "Durable": false,
      "FilterExpression": "",
      "NoLocalFilter": false,
      "Shared": false,
      "Endpoints": [
        {
          "Host": "localhost",
          "Port": 5672,
          "User": "user",
          "Password": "password"
        }
      ]
    }
  }
}
```

---

You can register the Consume service simply with dependency injection:

```csharp
    // Via options configuration
    IConfiguration config = ...;
    services
        .AddConsumerBackgroundService<FooConsumer>("FooConsumer", config);

    // Via code configuration
    services
        .AddConsumerBackgroundService<DummyConsumer>(new Axanndar.Consumer.Models.ConsumerConfiguration
        {
            IdEndpoint = "DummyConsumer",
            Address = "DummyConsumer.Address",
            Queue = "DummyConsumer.Queue",
            Endpoints = new List<Axanndar.Consumer.Models.ConsumerConfigurationEndpoint> {
                new Axanndar.Consumer.Models.ConsumerConfigurationEndpoint
                {
                 Host = "localhost",
                 Port = 61616,
                 Password = "password",
                 User = "user"
                }
            },            
            IsActive = true,
            RetryTime = 5,
            RoutingType = (int)RoutingType.Anycast,
        });
```

## Licensee
Repository source code is available under MIT License, see license in the source.

## Contributing
Thank you for considering to help out with the source code!
If you'd like to contribute, please fork, fix, commit and send a pull request for the maintainers to review and merge into the main code base.