namespace EventSourcing.Net;

using Abstractions.Contracts;
using Abstractions.Identities;
using Abstractions.Types;

internal static class CommandEnvelopeBuilder
{
    internal static ICommandEnvelope<TId> ToEnvelope<TId, TPayload>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        TPayload commandPayload)
        where TPayload : ICommand
    {
        ICommandEnvelope<TId> data;
        if (typeof(TPayload) != typeof(ICommand))
        {
            data = GetDataFast(tenantId, principalId, source, aggregateId, commandPayload);
        }
        else
        {
            data = GetDataSlow(tenantId, principalId, source, aggregateId, commandPayload);
        }

        return data;
    }

    private static ICommandEnvelope<TId> GetDataFast<TId, TPayload>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        TPayload commandPayload)
        where TPayload : ICommand
    {
        ICommandEnvelope<TId, TPayload> command = new CommandEnvelope<TId, TPayload>()
        {
            Payload = commandPayload,
            Timestamp = DateTime.UtcNow,
            AggregateId = aggregateId,
            CommandId = CommandId.New(),
            SequenceId = CommandSequenceId.New(),
            ParentCommandId = CommandId.Empty,
            TenantId = tenantId,
            Source = source,
            PrincipalId = principalId
        };

        return command;
    }

    /// <summary>
    /// Create command envelope in case when TPayload is ICommand. We have to use reflection to create proper envelop.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="principalId">Principal id.</param>
    /// <param name="source">Command source.</param>
    /// <param name="aggregateId">Aggregate id.</param>
    /// <param name="commandPayload">Command payload.</param>
    /// <typeparam name="TId">Type of aggregate id.</typeparam>
    /// <returns></returns>
    private static ICommandEnvelope<TId> GetDataSlow<TId>(
        TenantId tenantId,
        PrincipalId principalId,
        string source,
        TId aggregateId,
        ICommand commandPayload)
    {
        Type envelopeType = typeof(CommandEnvelope<,>).MakeGenericType(typeof(TId), commandPayload.GetType());
        object command = Activator.CreateInstance(envelopeType)!;

        IInitializableCommandEnvelope initializable = (IInitializableCommandEnvelope)command;
        initializable.Payload = commandPayload;
        initializable.Timestamp = DateTime.UtcNow;
        initializable.AggregateId = aggregateId;
        initializable.CommandId = CommandId.New();
        initializable.SequenceId = CommandSequenceId.New();
        initializable.ParentCommandId = CommandId.Empty;
        initializable.TenantId = tenantId;
        initializable.Source = source;
        initializable.PrincipalId = principalId;

        return (ICommandEnvelope<TId>)command;
    }
}