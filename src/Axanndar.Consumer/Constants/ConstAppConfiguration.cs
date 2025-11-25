using ActiveMQ.Artemis.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Constants
{
    /// <summary>
    /// Provides application-wide constant values for configuration properties and default values related to AMQP consumers and endpoints.
    /// </summary>
    public static class ConstAppConfiguration
    {
        /// <summary>
        /// Contains property name constants for AMQP configuration.
        /// </summary>
        public static class PropertyName 
        {
            /// <summary>
            /// The root property name for AMQP configuration.
            /// </summary>
            public const string AMQP = "Amqp";
            /// <summary>
            /// Contains property name constants for AMQP consumer configuration.
            /// </summary>
            public static class Amqp
            {
                /// <summary>
                /// The property name for consumer configuration.
                /// </summary>
                public const string CONSUMER = "Consumer";
                /// <summary>
                /// Contains property name constants for individual consumer settings.
                /// </summary>
                public static class Consumer
                {
                    /// <summary>
                    /// The property name for the ID endpoint.
                    /// </summary>
                    public const string ID_ENDPOINT = "IdEndpoint";
                    /// <summary>
                    /// The property name for the retry time.
                    /// </summary>
                    public const string RETRY_TIME = "RetryTime";
                    /// <summary>
                    /// The property name indicating if the consumer is active.
                    /// </summary>
                    public const string IS_ACTIVE = "IsActive";
                    /// <summary>
                    /// The property name for the address.
                    /// </summary>
                    public const string ADDRESS = "Address";
                    /// <summary>
                    /// The property name for the routing type.
                    /// </summary>
                    public const string ROUTING_TYPE = "RoutingType";
                    /// <summary>
                    /// The property name for the credit.
                    /// </summary>
                    public const string CREDIT = "Credit";
                    /// <summary>
                    /// The property name indicating if the setting is durable.
                    /// </summary>
                    public const string DURABLE = "Durable";
                    /// <summary>
                    /// The property name for the filter expression.
                    /// </summary>
                    public const string FILTER_EXPRESSION = "FilterExpression";
                    /// <summary>
                    /// The property name indicating if local queue consumption is disallowed.
                    /// </summary>
                    public const string NO_LOCAL_FILTER = "NoLocalFilter";
                    /// <summary>
                    /// The property name indicating if the queue is shared.
                    /// </summary>
                    public const string SHARED = "Shared";

                    /// <summary>
                    /// The property name for the list of endpoints.
                    /// </summary>
                    public const string ENDPOINTS = "Endpoints";
                    /// <summary>
                    /// Contains property name constants for endpoint settings.
                    /// </summary>
                    public static class Endpoint
                    {
                        /// <summary>
                        /// The property name for the host.
                        /// </summary>
                        public const string HOST = "Host";
                        /// <summary>
                        /// The property name for the port.
                        /// </summary>
                        public const string PORT = "Port";
                        /// <summary>
                        /// The property name for the username.
                        /// </summary>
                        public const string USER = "User";
                        /// <summary>
                        /// The property name for the password.
                        /// </summary>
                        public const string PASSWORD = "Password";
                    }
                }
            }
        }
        
        /// <summary>
        /// Contains default values for AMQP consumer configuration.
        /// </summary>
        public static class DefaultValue
        {
            /// <summary>
            /// Contains default values for AMQP consumer and endpoint settings.
            /// </summary>
            public static class Amqp
            {
                /// <summary>
                /// The default retry time in milliseconds.
                /// </summary>
                public const int RETRY_TIME = 5000;
                /// <summary>
                /// Indicates if the consumer is active by default.
                /// </summary>
                public const bool IS_ACTIVE = true;
                /// <summary>
                /// The default routing type as Anycast.
                /// </summary>
                public const int ROUTING_TYPE = (int)RoutingType.Anycast;   
                /// <summary>
                /// The default credit value.
                /// </summary>
                public const int CREDIT = 200;
                /// <summary>
                /// Indicates if the setting is durable by default.
                /// </summary>
                public const bool DURABLE = false;
                /// <summary>
                /// Indicates if local queue consumption is disallowed by default.
                /// </summary>
                public const bool NO_LOCAL_FILTER = false;
                /// <summary>
                /// Indicates if the queue is shared by default.
                /// </summary>
                public const bool SHARED = false;
            }
        }
    }
}
