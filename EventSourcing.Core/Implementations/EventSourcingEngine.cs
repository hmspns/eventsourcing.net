using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.Implementations
{
    /// <inheritdoc />
    public sealed class EventSourcingEngine : IEventSourcingEngine
    {
        private readonly IResolveEventStore _eventStoreResolver;
        private readonly IResolveSnapshotStore _snapshotStoreResolver;
        private readonly IResolveEventPublisher _publisherResolver;

        public EventSourcingEngine(
            IResolveEventStore eventStoreResolver,
            IResolveSnapshotStore snapshotStoreResolver,
            IResolveEventPublisher publisherResolver)
        {
            _eventStoreResolver = eventStoreResolver;
            _snapshotStoreResolver = snapshotStoreResolver;
            _publisherResolver = publisherResolver;
        }

        public IResolveEventStore EventStoreResolver => _eventStoreResolver;

        public IResolveSnapshotStore SnapshotStoreResolver => _snapshotStoreResolver;

        public IResolveEventPublisher PublisherResolver => _publisherResolver;
    }
}