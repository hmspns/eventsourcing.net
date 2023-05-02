using System.Diagnostics.CodeAnalysis;
using System.Threading;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Core.Exceptions;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Core.Implementations;

/// <inheritdoc />
public sealed class EventSourcingEngine : IEventSourcingEngine
{
    private static IEventSourcingEngine _instance;
    
    public EventSourcingEngine(
        IResolveEventStore eventStoreResolver,
        IResolveSnapshotStore snapshotStoreResolver,
        IResolveEventPublisher publisherResolver)
    {
        EventStoreResolver = eventStoreResolver;
        SnapshotStoreResolver = snapshotStoreResolver;
        PublisherResolver = publisherResolver;
    }

    [MemberNotNull(nameof(_instance))]
    internal static IEventSourcingEngine Instance
    {
        get
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (_instance == null)
            {
                Thrown.InvalidOperationException("EventSourcingEngine not set");
            }

            return _instance;
        }
        set => Interlocked.Exchange(ref _instance, value);
    }

    public IResolveEventStore EventStoreResolver { get; }

    public IResolveSnapshotStore SnapshotStoreResolver { get; }

    public IResolveEventPublisher PublisherResolver { get; }
}