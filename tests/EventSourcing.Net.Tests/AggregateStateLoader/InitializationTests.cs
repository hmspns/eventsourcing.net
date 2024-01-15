namespace EventSourcing.Net.Tests.AggregateStateLoader;

using Abstractions.Contracts;
using Engine.Implementations;
using Engine.InMemory;
using Engine.NoImplementation;
using FluentAssertions;
using SequenceAggregate;
using Shared;

public sealed class InitializationTests : IDisposable
{
    private readonly SequenceStateMutator _sequenceStateMutator = new SequenceStateMutator();
    
    [Fact]
    public void DefaultConstructor()
    {
        Wrapper wrapper = new Wrapper(new AggregateStateLoader<int, SequenceAggregate, SequenceState>());
        EventSourcingEngine.Instance = Create();
        
        wrapper.MutatorActivator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeSameAs(_sequenceStateMutator);
        
        wrapper.EventSourcingEngineHandler.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().BeSameAs(EventSourcingEngine.Instance);
    }

    [Fact]
    public void OnlyActivator()
    {
        Wrapper wrapper = new Wrapper(new AggregateStateLoader<int, SequenceAggregate, SequenceState>(() => _sequenceStateMutator));
        EventSourcingEngine.Instance = Create();
        
        wrapper.MutatorActivator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeNull();
        wrapper.Mutator.Should().BeSameAs(_sequenceStateMutator);
       
        wrapper.EventSourcingEngineHandler.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().BeSameAs(EventSourcingEngine.Instance);
    }

    [Fact]
    public void OnlyEventSourcingEngine()
    {
        Wrapper wrapper = new Wrapper(new AggregateStateLoader<int, SequenceAggregate, SequenceState>(Create()));
        EventSourcingEngine.Instance = Create();
        
        wrapper.MutatorActivator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeSameAs(_sequenceStateMutator);
        
        wrapper.EventSourcingEngineHandler.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeSameAs(EventSourcingEngine.Instance);
    }

    [Fact]
    public void BothOfActivatorAndEventSourcingEngine()
    {
        Wrapper wrapper = new Wrapper(new AggregateStateLoader<int, SequenceAggregate, SequenceState>(Create(), () => _sequenceStateMutator));
        EventSourcingEngine.Instance = Create();
        
        wrapper.MutatorActivator.Should().NotBeNull();
        wrapper.Mutator.Should().NotBeNull();
        wrapper.Mutator.Should().BeSameAs(_sequenceStateMutator);
        
        wrapper.EventSourcingEngineHandler.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeNull();
        wrapper.EventSourcingEngine.Should().NotBeSameAs(EventSourcingEngine.Instance);
    }

    [Fact]
    public void NreTests()
    {
        Action a = () => new AggregateStateLoader<int, SequenceAggregate, SequenceState>((IEventSourcingEngine)null);
        Action b = () => new AggregateStateLoader<int, SequenceAggregate, SequenceState>((Func<IStateMutator<SequenceState>>)null);
        Action c = () => new AggregateStateLoader<int, SequenceAggregate, SequenceState>(null, (Func<IStateMutator<SequenceState>>)null);
        Action d = () => new AggregateStateLoader<int, SequenceAggregate, SequenceState>(null, () => new SequenceStateMutator());
        Action e = () => new AggregateStateLoader<int, SequenceAggregate, SequenceState>(Create(), (Func<IStateMutator<SequenceState>>)null);

        a.Should().Throw<ArgumentNullException>();
        b.Should().Throw<ArgumentNullException>();
        c.Should().Throw<ArgumentNullException>();
        d.Should().Throw<ArgumentNullException>();
        e.Should().Throw<ArgumentNullException>();
    }

    private EventSourcingEngine Create() => new EventSourcingEngine(new EventStoreResolver(new InMemoryResolveAppender()), new NoSnapshotStoreResolver(), new NoEventPublisherResolver());

    public void Dispose()
    {
        EventSourcingEngine.Instance = null;
    }
    
    private sealed class Wrapper
    {
        private readonly AggregateStateLoader<int, SequenceAggregate, SequenceState> _loader;

        public Wrapper(AggregateStateLoader<int, SequenceAggregate, SequenceState> loader)
        {
            _loader = loader;
        }

        public Func<IStateMutator<SequenceState>> MutatorActivator => _loader.GetPrivateField<Func<IStateMutator<SequenceState>>>("_stateMutatorActivator");

        public IStateMutator<SequenceState> Mutator => MutatorActivator();

        public Func<IEventSourcingEngine> EventSourcingEngineHandler => _loader.GetPrivateField<Func<IEventSourcingEngine>>("_engineHandler");

        public IEventSourcingEngine EventSourcingEngine => EventSourcingEngineHandler();
    }
}