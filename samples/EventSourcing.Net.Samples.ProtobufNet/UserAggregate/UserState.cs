using ProtoBuf;

namespace EventSourcing.Net.Samples.ProtobufNet.UserAggregate;

[ProtoContract]
public record UserState
{
    /// <summary>
    /// Property to indicate that current user is exists. Exists means UserCreatedEvent was handled.
    /// </summary>
    [ProtoMember(1)]
    public bool IsCreated { get; set; }
    
    /// <summary>
    /// Property to indicate that current user is deleted. Deleted means UserDeletedEvent was handled.
    /// </summary>
    [ProtoMember(2)]
    public bool IsDeleted { get; set; }
    
    [ProtoMember(3)]
    public string Name { get; set; }
    
    [ProtoMember(4)]
    public DateTime BirthDate { get; set; }
    
    [ProtoMember(5)]
    public string PhoneNumber { get; set; }
}