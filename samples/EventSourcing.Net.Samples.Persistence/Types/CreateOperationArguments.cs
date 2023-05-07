namespace EventSourcing.Net.Samples.Persistence.Types;

public class CreateOperationArguments
{
    public OperationType OperationType { get; set; }
    
    public Guid AccountId { get; set; }
    
    public decimal Amount { get; set; }
}