using System;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Abstractions.Contracts;

/// <summary>
/// Provide functionality to set properties to command envelope.
/// </summary>
internal interface IInitializableCommandEnvelope
{
    public TenantId TenantId { set; }
    public CommandSequenceId SequenceId { set; }
    public CommandId CommandId { set; }
    public CommandId ParentCommandId { set; }
    public DateTime Timestamp { set; }
    public object AggregateId { set; }
    public object Payload { set; }
    public PrincipalId PrincipalId { set; }
    public string Source { set; }
}