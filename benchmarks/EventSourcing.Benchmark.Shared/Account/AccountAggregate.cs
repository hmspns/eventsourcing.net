using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;

namespace EventSourcing.Benchmark.Shared.Account;

public sealed class AccountAggregate : Aggregate<Guid, AccountState, AccountStateMutator>
{
    public AccountAggregate(Guid aggregateId) : base(aggregateId, new AccountStateMutator())
    {
    }

    public ICommandExecutionResult<Guid> CreateAccount(ICommandEnvelope<Guid, CreateAccountCommand> cmd)
    {
        if (!State.IsCreated)
        {
            Apply(cmd, new AccountCreatedEvent(cmd.Payload.OwnerName));
        }

        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public ICommandExecutionResult<Guid> ReplenishAccount(ICommandEnvelope<Guid, ReplenishAccountCommand> cmd)
    {
        if (State is { IsCreated: true, IsClosed: false } &&
            cmd.Payload.Amount > 0 &&
            State.LastOperationTimestamp < cmd.Timestamp)
        {
            // if we don't process this operation yet
            Apply(cmd, new AccountReplenishedEvent(cmd.Payload.OperationId, cmd.Payload.Amount));
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public ICommandExecutionResult<Guid> WithdrawAccount(ICommandEnvelope<Guid, WithdrawAccountCommand> cmd)
    {
        if (State is { IsCreated: true, IsClosed: false })
        {
            if (State.Amount < cmd.Payload.Amount)
            {
                return CommandExecutionResult<Guid>.Error(cmd, "Not enough money to provide withdrawn");
            }
            
            if (cmd.Payload.Amount > 0 && State.LastOperationTimestamp < cmd.Timestamp)
            {
                // if we don't process this operation yet
                Apply(cmd, new AccountWithdrawnEvent(cmd.Payload.OperationId, cmd.Payload.Amount));
            }
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public ICommandExecutionResult<Guid> CloseAccount(ICommandEnvelope<Guid, CloseAccountCommand> cmd)
    {
        if (State is { IsCreated: true, IsClosed: false })
        {
            if (State.Amount > 0)
            {
                return CommandExecutionResult<Guid>.Error(cmd, "Non-zero account can't be closed");
            }
            
            Apply(cmd, new AccountClosedEvent());
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }
}