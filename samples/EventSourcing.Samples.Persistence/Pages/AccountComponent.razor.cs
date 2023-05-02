using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Samples.Persistence.Aggregate;
using EventSourcing.Samples.Persistence.Data;
using EventSourcing.Samples.Persistence.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class AccountComponent
{
    [Inject]
    public ApplicationDbContext DbContext { get; set; }
    
    [Inject]
    public IEventSourcingCommandBus Bus { get; set; }
    
    [Parameter]
    public Guid Id { get; set; }
    
    private bool IsNewOperationMode { get; set; }
    
    private ICommandExecutionResult<Guid> CommandExecutionResult { get; set; }

    public AccountDb Account { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadAccount();
    }

    private async Task LoadAccount()
    {
        Account = await DbContext.Accounts.Include(x => x.Operations).FirstOrDefaultAsync(x => x.Id == Id);
    }

    private async Task CreateOperation(CreateOperationArguments arg)
    {
        if (arg.OperationType == OperationType.Replenishment)
        {
            CommandExecutionResult = await Bus.Send(arg.AccountId, new ReplenishAccountCommand(Guid.NewGuid(), arg.Amount));
        }
        else
        {
            CommandExecutionResult = await Bus.Send(arg.AccountId, new WithdrawAccountCommand(Guid.NewGuid(), arg.Amount));
        }

        await LoadAccount();
        IsNewOperationMode = false;
    }

    private void SwitchToNewOperationMode()
    {
        IsNewOperationMode = true;
        CommandExecutionResult = null;
    }
}