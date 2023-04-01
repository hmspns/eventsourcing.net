using EventSourcing.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class Accounts
{
    [Inject]
    private IEventSourcingCommandBus Bus { get; set; }
    
    private async Task CreateAccount()
    {
        AccountDataGenerationService service = new AccountDataGenerationService(Bus);
        await service.CreateTestAccount();
    }
}