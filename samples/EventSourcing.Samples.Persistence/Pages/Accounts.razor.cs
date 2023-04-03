using EventSourcing.Samples.Persistence.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class Accounts
{
    [Inject]
    private ApplicationDbContext DbContext { get; set; }
    
    private AccountDb[] Data { get; set; }

    protected override async Task OnInitializedAsync()
    {
        Data = await DbContext.Accounts.OrderBy(x => x.CreationDate).ToArrayAsync();
    }

    private async Task LoadAccount()
    {
        throw new NotImplementedException();
    }
}