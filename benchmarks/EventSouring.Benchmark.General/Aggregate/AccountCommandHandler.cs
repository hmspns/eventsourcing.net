using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;

namespace EventSouring.Benchmark.General.Aggregate;

public class AccountCommandHandler : CommandHandler<Guid, AccountAggregate>
{
    public AccountCommandHandler(IEventSourcingEngine engine) 
        : base(id => new AccountAggregate(id), engine)
    {
    }

    public Task<ICommandExecutionResult<Guid>> CreateAccount(ICommandEnvelope<Guid, CreateAccountCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.CreateAccount(cmd));
    }

    public Task<ICommandExecutionResult<Guid>> ReplenishAccount(ICommandEnvelope<Guid, ReplenishAccountCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.ReplenishAccount(cmd));
    }

    public Task<ICommandExecutionResult<Guid>> WithdrawAccount(ICommandEnvelope<Guid, WithdrawAccountCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.WithdrawAccount(cmd));
    }

    public Task<ICommandExecutionResult<Guid>> CloseAccount(ICommandEnvelope<Guid, CloseAccountCommand> cmd)
    {
        return Update(cmd, aggregate => aggregate.CloseAccount(cmd));
    }
}