namespace EventSourcing.Core.Contracts
{
    /// <summary>
    /// Event sourcing engine.
    /// </summary>
    public interface IEventSourcingEngine
    {
        /// <summary>
        /// Return factory to resolve tenant based event store.
        /// </summary>
        public IResolveEventStore EventStoreResolver { get; }
        
        /// <summary>
        /// Return factory to resolve snapshot store for specific stream.
        /// </summary>
        public IResolveSnapshotStore SnapshotStoreResolver { get; }
        
        /// <summary>
        /// Return factory to resolve event publisher.
        /// </summary>
        public IResolveEventPublisher PublisherResolver { get; }
    }
}