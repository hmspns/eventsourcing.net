using System;

namespace EventSourcing.Net.Engine.LowLevel;

public sealed class EventEntry
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }
    
    public string StreamName { get; set; }
    
    public long StreamPosition { get; set; }
    
    public long GlobalPosition { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public Guid CommandId { get; set; }
    
    public Guid SequenceId { get; set; }
    
    public string PrincipalId { get; set; }
    
    public string PayloadType { get; set; }
    
    public byte[] Payload { get; set; }
}