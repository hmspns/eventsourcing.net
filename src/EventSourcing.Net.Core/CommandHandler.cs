using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Core.Exceptions;
using EventSourcing.Net.Core.Extensions;
using EventSourcing.Net.Core.Implementations;

namespace EventSourcing.Net.Core;

internal interface ICommandHandler
{
    internal IEventSourcingEngine Engine { get; set; }
}
    
/// <summary>
/// Base class for command handler.
/// </summary>
/// <typeparam name="TId">Aggregate id type.</typeparam>
/// <typeparam name="TAggregate">Aggregate type.</typeparam>
public abstract class CommandHandler<TId, TAggregate> : ICommandHandler where TAggregate : IAggregate
{
    private readonly Func<TId, TAggregate> _aggregateActivator;
    private IEventSourcingEngine? _engine;

    IEventSourcingEngine ICommandHandler.Engine
    {
        get => _engine;
        set => Interlocked.CompareExchange(ref _engine, value, null);
    }

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="activator">Function to create new aggregate.</param>
    /// <param name="engine">Optional instance of event sourcing engine.</param>
    protected CommandHandler(Func<TId, TAggregate> activator, IEventSourcingEngine? engine = null)
    {
        _aggregateActivator = activator;
        _engine = engine; // ?? EventSourcingEngine.Instance;
    }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="commandEnvelope">Command data.</param>
    /// <param name="handler">Command handler.</param>
    /// <returns>Result of command execution.</returns>
    /// <exception cref="ArgumentNullException">Command envelope is null.</exception>
    /// <exception cref="ArgumentException">Command envelope contains null for required properties</exception>
    /// <exception cref="CommandExecutionException">Command couldn't executed because of aggregate exception.</exception>
    protected async Task<ICommandExecutionResult<TId>> Update(
        ICommandEnvelope<TId> commandEnvelope,
        Func<TAggregate, ICommandExecutionResult<TId>> handler,
        CancellationToken cancellationToken = default
    )
    {
        CommandMessageValidator validator = new CommandMessageValidator();
        validator.Validate(commandEnvelope);
            
        int retryLimit = 15;
        for (int i = 1; i <= retryLimit; i++)
        {
            try
            {
                if (cancellationToken.CancellationWasRequested(commandEnvelope, out ICommandExecutionResult<TId> cancelledResult))
                {
                    return cancelledResult;
                }
                    
                Trace.WriteLine($"Processing command {commandEnvelope.Payload.GetType().Name} on iteration {i.ToString()}");
                    
                AggregateUpdater<TId, TAggregate> updater = new AggregateUpdater<TId, TAggregate>(_engine ?? EventSourcingEngine.Instance, _aggregateActivator);
                ICommandExecutionResult<TId> result = await updater.Execute(commandEnvelope, handler, cancellationToken);
                return result;
            }
            catch (AggregateConcurrencyException<TId> e)
            {
                Trace.WriteLine(nameof(AggregateConcurrencyException<TId>) + $" occurs on iteration {i.ToString()} occurs." +
                                $" Expected version {e.ExpectedVersion.ToString()}. Actual version {e.ActualVersion.ToString()}." +
                                "It means concurrent thread update an aggregate with the same AggregateId during execution of current aggregate." +
                                $"Will try execute command again {(retryLimit - i).ToString()} times");
            }
        }
        Trace.WriteLine($"It's impossible to execute command after {retryLimit.ToString()} tries");

        throw new CommandExecutionException("Couldn't execute command", commandEnvelope);
    }
}

internal sealed class CommandMessageValidator
{
    internal void Validate<TId>(ICommandEnvelope<TId> cmd)
    {
        if (cmd == null)
        {
            Thrown.ArgumentNullException(nameof(cmd));
        }

        if (cmd.AggregateId == null)
        {
            Thrown.ArgumentException("Command.AggregateId shouldn't be null");
        }

        if (cmd.Payload == null)
        {
            Thrown.ArgumentException("Command.Payload mustn't be null");
        }

        // if (string.IsNullOrEmpty(cmd.Source))
        // {
        //     throw new ArgumentException("Command.Source mustn't be null nor empty string");
        // }

        if (cmd.Timestamp == default || cmd.Timestamp.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("Command.Timestamp must be valid UTC value");
        }

        if (cmd.CommandId == CommandId.Empty)
        {
            throw new ArgumentException("Command.CommandId mustn't be CommandId.Empty");
        }

        // if (cmd.PrincipalId == PrincipalId.Empty)
        // {
        //     throw new ArgumentException("Command.PrincipalId mustn't be PrincipalId.Empty");
        // }

        if (cmd.SequenceId == CommandSequenceId.Empty)
        {
            throw new ArgumentException("Command.SequenceId mustn't be CommandSequenceId.Empty");
        }

        // if (cmd.TenantId == TenantId.Empty)
        // {
        //     throw new ArgumentException("Command.TenantId mustn't be TenantId.Empty");
        // }
    }
}