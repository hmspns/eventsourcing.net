using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Benchmark.Shared.Account;
using EventSourcing.Core.Implementations;

namespace EventSouring.Benchmark.General;

public sealed class AccountCommandsExecutor
{
    private readonly IEventSourcingCommandBus _bus;
    private readonly IEventSourcingEngine _engine;

    public AccountCommandsExecutor(IEventSourcingCommandBus bus, IEventSourcingEngine engine)
    {
        _engine = engine;
        _bus = bus;
    }

    public async Task CreateTestAccount(TenantId tenantId, int addCount = 30, int restCount = 20)
    {
        Guid accountId = Guid.NewGuid();
        await _bus.Send(tenantId, PrincipalId.Empty, nameof(CreateTestAccount), accountId,
            new CreateAccountCommand("Owner for account " + accountId.ToString()));

        for (int i = 0; i < addCount; i++)
        {
            ReplenishAccountCommand command = new ReplenishAccountCommand(Guid.NewGuid(), 199);
            await _bus.Send(tenantId, PrincipalId.Empty, nameof(CreateTestAccount), accountId, command);
        }

        for (int i = 0; i < restCount; i++)
        {
            WithdrawAccountCommand command = new WithdrawAccountCommand(Guid.NewGuid(), 49);
            await _bus.Send(tenantId, PrincipalId.Empty, nameof(CreateTestAccount), accountId, command);
        }

        AggregateStateLoader<Guid, AccountAggregate, AccountState> loader = 
            new AggregateStateLoader<Guid, AccountAggregate, AccountState>(_engine, () => new AccountStateMutator());
        AccountState state = await loader.GetState(tenantId, accountId);
        
        WithdrawAccountCommand withdrawAllCmd = new WithdrawAccountCommand(Guid.NewGuid(), state.Amount);
        await _bus.Send(tenantId, PrincipalId.Empty, nameof(CreateTestAccount), accountId, withdrawAllCmd);

        CloseAccountCommand closeCmd = new CloseAccountCommand();
        await _bus.Send(tenantId, PrincipalId.Empty, nameof(CreateTestAccount), accountId, closeCmd);
    }
}