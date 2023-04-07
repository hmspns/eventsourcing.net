using EventSourcing.Samples.Persistence.Data;
using EventSourcing.Samples.Persistence.Types;
using Microsoft.AspNetCore.Components;

namespace EventSourcing.Samples.Persistence.Pages;

public partial class CreateOperationComponent
{
    [Parameter]
    public Guid AccountId { get; set; }

    [Parameter]
    public EventCallback<CreateOperationArguments> OnCreateOperationCallback { get; set; }

    private decimal Amount { get; set; }
    
    private OperationType OperationType { get; set; }

    private Task OnCreateOperationClick()
    {
        CreateOperationArguments args = new CreateOperationArguments()
        {
            Amount = Amount,
            AccountId = AccountId,
            OperationType = OperationType
        };
        return OnCreateOperationCallback.InvokeAsync(args);
    }
}

