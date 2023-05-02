using BlazorBootstrap;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Aggregate;
using EventSourcing.Samples.Persistence.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class AccountListComponent
{
    [Inject]
    private ApplicationDbContext DbContext { get; set; }
    
    [Inject]
    private IEventSourcingCommandBus Bus { get; set; }
    
    private AccountDb[] Data { get; set; }
    
    private Modal Modal { get; set; }
    
    private string OwnerName { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await RefreshData();
    }

    private async Task RefreshData()
    {
        Data = await DbContext.Accounts.OrderBy(x => x.CreationDate).ToArrayAsync();
    }

    private async Task OnCreateAccount()
    {
        await Modal.ShowAsync();
    }

    private async Task OnCreateAccountClick()
    {
        await Bus.Send(Guid.NewGuid(), new CreateAccountCommand(OwnerName));
        await Modal.HideAsync();
        await RefreshData();
        OwnerName = null;
    }
}