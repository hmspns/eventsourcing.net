using System;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Abstractions
{
    /// <summary>
    /// Envelope for the event.
    /// </summary>
    public interface IEventEnvelope
    {
        /// <summary>
        /// Get tenant id.
        /// </summary>
        public TenantId TenantId { get; }
        
        /// <summary>
        /// Get event id.
        /// </summary>
        public EventId EventId { get; }
        
        /// <summary>
        /// Get aggregate id.
        /// </summary>
        object AggregateId { get; }
        
        /// <summary>
        /// Get timestamp.
        /// </summary>
        public DateTime Timestamp { get; }
        
        /// <summary>
        /// Get event data.
        /// </summary>
        IEvent Payload { get; }
        
        /// <summary>
        /// Get command id.
        /// </summary>
        public CommandId CommandId { get; }
        
        /// <summary>
        /// Get command sequence id.
        /// </summary>
        public CommandSequenceId SequenceId { get; }
        
        /// <summary>
        /// Get id of principal.
        /// </summary>
        public PrincipalId PrincipalId { get; }
    }


    /// <inheritdoc />
    public interface IEventEnvelope<out TId, out TPayload> : IEventEnvelope where TPayload : IEvent 
    {
        public TId AggregateId { get; }
        
        public TPayload Payload { get; }
        
        object IEventEnvelope.AggregateId => AggregateId;
        
        IEvent IEventEnvelope.Payload => Payload;
    }
}