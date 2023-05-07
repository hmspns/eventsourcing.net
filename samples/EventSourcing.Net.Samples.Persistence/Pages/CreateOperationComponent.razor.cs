using BlazorBootstrap;
using EventSourcing.Net.Samples.Persistence.Types;
using Microsoft.AspNetCore.Components;

namespace EventSourcing.Net.Samples.Persistence.Pages;

public partial class CreateOperationComponent
{
    [Parameter]
    public Guid AccountId { get; set; }

    [Parameter]
    public EventCallback<CreateOperationArguments> OnCreateOperationCallback { get; set; }

    private Modal Modal { get; set; }

    private decimal Amount { get; set; }
    
    private OperationType OperationType { get; set; }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        return Modal.ShowAsync();
    }

    private Task OnCreateOperationClick()
    {
        CreateOperationArguments args = new CreateOperationArguments()
        {
            Amount = Amount,
            AccountId = AccountId,
            OperationType = OperationType
        };
        Amount = 0;
        OperationType = OperationType.Replenishment;
        return OnCreateOperationCallback.InvokeAsync(args);
    }
}

