using EventSourcing.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.AccountAggregate;

namespace EventSourcing.Samples.Persistence.Services;

public sealed class AccountDataGenerationService
{
    private readonly IEventSourcingCommandBus _bus;

    public AccountDataGenerationService(IEventSourcingCommandBus bus)
    {
        _bus = bus;
    }

    public async Task CreateTestAccount()
    {
        Guid accountId = Guid.NewGuid();
        await _bus.Send(accountId, new CreateAccountCommand("Owner for account " + accountId.ToString()));
    }
}