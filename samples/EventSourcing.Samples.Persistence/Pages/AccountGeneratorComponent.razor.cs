using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class AccountGeneratorComponent
{
    [Inject]
    private IEventSourcingCommandBus Bus { get; set; }

    private static List<ICommandExecutionResult<Guid>> Results { get; set; } = new List<ICommandExecutionResult<Guid>>();

    private async Task CreateAccount()
    {
        Results.Clear();
        AccountDataGenerationService service = new AccountDataGenerationService(Bus);
        Results.AddRange(await service.CreateTestAccount());
    }
}