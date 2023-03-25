﻿using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Exceptions;
using EventSourcing.Core.Extensions;
using EventSourcing.Core.NoImplementation;

namespace EventSourcing.Core
{
    internal interface ICommandHandler
    {
        
    }
    
    /// <summary>
    /// Base class for command handler.
    /// </summary>
    /// <typeparam name="TId">Aggregate id type.</typeparam>
    /// <typeparam name="TAggregate">Aggregate type.</typeparam>
    public abstract class CommandHandler<TId, TAggregate> : ICommandHandler where TAggregate : IAggregate
    {
        private readonly Func<TId, TAggregate> _aggregateActivator;
        private readonly IEventSourcingEngine _engine;

        protected CommandHandler(Func<TId, TAggregate> activator, IEventSourcingEngine engine)
        {
            _engine = engine;
            _aggregateActivator = activator;
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
            
            int retryLimit = 5;
            for (int i = 1; i <= retryLimit; i++)
            {
                try
                {
                    if (cancellationToken.CancellationWasRequested(commandEnvelope, out ICommandExecutionResult<TId> cancelledResult))
                    {
                        return cancelledResult;
                    }
                    
                    AggregateUpdater<TId, TAggregate> updater = new AggregateUpdater<TId, TAggregate>(_engine, _aggregateActivator);
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
                throw new ArgumentNullException(nameof(cmd));
            }

            if (cmd.AggregateId == null)
            {
                throw new ArgumentException("Command.AggregateId mustn't be null");
            }

            if (cmd.Payload == null)
            {
                throw new ArgumentException("Command.Payload mustn't be null");
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
}