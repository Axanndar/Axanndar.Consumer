using System;

namespace Axanndar.Consumer.Logger.Interfaces
{
    public interface ICorrelationIdProvider
    {
        string? CorrelationId { get; }
        void NewCorrelationId();
    }
}
