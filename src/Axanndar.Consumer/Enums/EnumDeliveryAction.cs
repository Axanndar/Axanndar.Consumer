using System;
using System.Collections.Generic;
using System.Text;

namespace Axanndar.Consumer.Enums
{
    public enum EnumDeliveryAction
    {
        /// <summary>
        /// To Accept received message.
        /// </summary>
        Accept = 1,
        /// <summary>
        /// To Reject received message (malformed message)
        /// </summary>
        Reject = 2,
        /// <summary>
        /// To Release received message (message that cannot be processed at the moment, but can be retried later) (No update)
        /// </summary>
        Release = 3,
        /// <summary>
        /// To Retry received message (message that cannot be processed at the moment, but can be retried later) (Update retry count and retry time)
        /// </summary>
        Retry = 4
    }
}
