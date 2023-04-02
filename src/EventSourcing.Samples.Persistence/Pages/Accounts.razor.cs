using EventSourcing.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class Accounts
{
    [Inject]
    private IEventSourcingCommandBus Bus { get; set; }

    private List<ICommandExecutionResult<Guid>> Results { get; set; } = new List<ICommandExecutionResult<Guid>>();

    private async Task CreateAccount()
    {
        AccountDataGenerationService service = new AccountDataGenerationService(Bus);
        await service.CreateTestAccount();
    }
}