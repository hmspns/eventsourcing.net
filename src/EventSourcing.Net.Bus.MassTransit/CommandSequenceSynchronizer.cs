using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using MassTransit;

namespace EventSourcing.Net.Bus.MassTransit;

/// <summary>
/// Listening notification about completion of sequence of commands and notify <see cref="EventSourcing.Net.Abstractions.Contracts.IPublicationAwaiter"/> about it.
/// </summary>
/// <see cref="CommandsSequenceSaga"/>
public sealed class CommandSequenceSynchronizer : IConsumer<IEsSequenceCompleted>
{
    private readonly IPublicationAwaiter _awaiter;

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="awaiter">Awaiter to be notified.</param>
    public CommandSequenceSynchronizer(IPublicationAwaiter awaiter)
    {
        _awaiter = awaiter;
    }
        
    public Task Consume(ConsumeContext<IEsSequenceCompleted> context)
    {
        _awaiter.MarkAsReady(context.Message.SequenceId);
        return Task.CompletedTask;
    }
}
    
/// <inheritdoc />
public sealed class MediatorPublicationAwaiter : IPublicationAwaiter
{
    void IPublicationAwaiter.MarkAsReady(CommandSequenceId sequenceId)
    {
        // in memory mass transit synchronize it by itself
    }

    public Task WaitForPublication(CommandSequenceId sequenceId, TimeSpan timeout = default)
    {
        return Task.CompletedTask;
    }
}

/// <inheritdoc />
public sealed class PublicationAwaiter : IPublicationAwaiter
{
    private readonly ConcurrentDictionary<CommandSequenceId, TaskCompletionSource> _data = new ConcurrentDictionary<CommandSequenceId, TaskCompletionSource>();

    void IPublicationAwaiter.MarkAsReady(CommandSequenceId sequenceId)
    {
        if(_data.TryRemove(sequenceId, out TaskCompletionSource completionSource))
        {
            completionSource.TrySetResult();
        }
    }

    public Task WaitForPublication(CommandSequenceId sequenceId, TimeSpan timeout = default)
    {
        TaskCompletionSource source = new TaskCompletionSource();
        _data.TryAdd(sequenceId, source);

        CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        tokenSource.Token.Register(() =>
        {
            _data.TryRemove(sequenceId, out TaskCompletionSource? innerToken);
            innerToken.TrySetCanceled();
        });
            
        source.SetCanceled(CancellationToken.None);
        return source.Task;
    }
}