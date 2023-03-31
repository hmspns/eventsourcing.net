namespace EventSourcing.Samples.Simple.UserAggregate;

public record UserState
{
    /// <summary>
    /// Property to indicate that current user exists. Exists means UserCreatedEvent was handled.
    /// </summary>
    public bool IsCreated { get; set; }
    
    /// <summary>
    /// Property to indicate that current user is deleted. Deleted means UserDeletedEvent was handled.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    public string Name { get; set; }
    
    public DateTime BirthDate { get; set; }
    
    public string PhoneNumber { get; set; }
}