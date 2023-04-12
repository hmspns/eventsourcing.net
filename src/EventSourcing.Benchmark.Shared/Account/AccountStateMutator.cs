using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;

namespace EventSourcing.Benchmark.Shared.Account;

public class AccountStateMutator : StateMutator<AccountState>
{
    public override AccountState DefaultState => new AccountState()
    {
        OwnerName = null,
        Amount = 0,
        IsClosed = false,
        IsCreated = false,
        LastOperationTimestamp = DateTime.MinValue
    };

    public AccountStateMutator()
    {
        Register<Guid, AccountCreatedEvent>(Handle);
        Register<Guid, AccountReplenishedEvent>(Handle);
        Register<Guid, AccountWithdrawnEvent>(Handle);
        Register<Guid, AccountClosedEvent>(Handle);
    }

    private AccountState Handle(IEventEnvelope<Guid, AccountCreatedEvent> e, AccountState state)
    {
        state.IsCreated = true;
        state.OwnerName = e.Payload.OwnerName;
        state.LastOperationTimestamp = e.Timestamp;

        return state;
    }
    
    private AccountState Handle(IEventEnvelope<Guid, AccountReplenishedEvent> e, AccountState state)
    {
        state.Amount += e.Payload.Amount;
        state.LastOperationTimestamp = e.Timestamp;
        
        return state;
    }
    
    private AccountState Handle(IEventEnvelope<Guid, AccountWithdrawnEvent> e, AccountState state)
    {
        state.Amount -= e.Payload.Amount;
        state.LastOperationTimestamp = e.Timestamp;

        return state;
    }

    private AccountState Handle(IEventEnvelope<Guid, AccountClosedEvent> e, AccountState state)
    {
        state.IsClosed = true;
        state.LastOperationTimestamp = e.Timestamp;

        return state;
    }
}