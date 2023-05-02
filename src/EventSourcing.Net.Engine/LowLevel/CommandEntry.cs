using System;

namespace EventSourcing.Net.Engine.LowLevel;

public sealed class CommandEntry
{
    internal CommandEntry()
    {
        
    }
    
    public Guid CommandId { get; set; }
    
    public Guid ParentCommandId { get; set; }
    
    public Guid CommandSequenceId { get; set; }
    
    public Guid TenantId { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string AggregateId { get; set; }
    
    public string PrincipalId { get; set; }
    
    public string CommandSource { get; set; }
    
    public string PayloadType { get; set; }
    
    public byte[] Payload { get; set; }
}