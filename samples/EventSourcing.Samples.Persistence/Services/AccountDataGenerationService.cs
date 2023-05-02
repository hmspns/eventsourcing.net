using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Aggregate;

namespace EventSourcing.Samples.Persistence.Services;

public sealed class AccountDataGenerationService
{
    private readonly IEventSourcingCommandBus _bus;

    public AccountDataGenerationService(IEventSourcingCommandBus bus)
    {
        _bus = bus;
    }

    public async Task<List<ICommandExecutionResult<Guid>>> CreateTestAccount()
    {
        List<ICommandExecutionResult<Guid>> results = new List<ICommandExecutionResult<Guid>>();
        Guid accountId = Guid.NewGuid();
        var result = await _bus.Send(accountId, new CreateAccountCommand("Owner for account " + accountId.ToString()));

        results.Add(result);
        if (result is { IsAccepted: true, HasChanges: true })
        {
            Random r = new Random();
            for (int i = 0; i < 20; i++)
            {
                ICommand command;
                if (r.Next(0, 100) % 2 == 0)
                {
                    command = new ReplenishAccountCommand(Guid.NewGuid(), r.Next(1, 100));
                }
                else
                {
                    command = new WithdrawAccountCommand(Guid.NewGuid(), r.Next(1, 50));
                }

                result = await _bus.Send(accountId, command);
                results.Add(result);
            }
        }

        return results;
    }
}