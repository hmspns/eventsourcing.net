using System;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Engine.Extensions;

using Abstractions.Types;

/// <summary>
/// Extension methods for events.
/// </summary>
public static class EventSourcingExtensions
{
    /// <summary>
    /// Convert event to new command bounded to same sequence.
    /// </summary>
    /// <param name="payloadEvent"></param>
    /// <param name="id">Aggregate id.</param>
    /// <param name="payload">Command payload.</param>
    /// <param name="source">Command source.</param>
    /// 
    public static ICommandEnvelope<TId, TPayload> ToCommand<TId, TPayload>(
        this IEventEnvelope payloadEvent,
        TId id,
        TPayload payload,
        string source)
        where TPayload : ICommand
    {
        return new CommandEnvelope<TId, TPayload>()
        {
            Payload = payload,
            Timestamp = DateTime.UtcNow,
            AggregateId = id,
            CommandId = CommandId.New(),
            SequenceId = payloadEvent.SequenceId,
            ParentCommandId = payloadEvent.CommandId,
            TenantId = payloadEvent.TenantId,
            Source = source,
            PrincipalId = payloadEvent.PrincipalId
        };
    }
}