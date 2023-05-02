using System;
using System.Collections.Generic;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using MassTransit;

namespace EventSourcing.Bus.MassTransit;

/// <summary>
/// State for saga.
/// </summary>
/// <seealso cref="https://masstransit-project.com/usage/sagas/automatonymous.html#state"/>
public sealed class CommandSequenceSagaState : SagaStateMachineInstance
{
    private readonly HashSet<CommandId> _commands = new HashSet<CommandId>();

    public int CurrentState { get; set; }

    public Guid CorrelationId { get; set; }

    public HashSet<CommandId> Commands => _commands;
}

/// <summary>
/// Saga to control command sequence flow.
/// </summary>
/// <seealso cref="https://masstransit-project.com/usage/sagas/automatonymous.html#state-machine"/>
public sealed class CommandsSequenceSaga : MassTransitStateMachine<CommandSequenceSagaState>
{
    public State InProcess { get; private set; }
    public State Done { get; private set; }
        
    public Event<IEsCommandPublicationStarted> CommandStarted { get; set; }

    public Event<IEsCommandPublicationCompleted> CommandCompleted { get; set; }

    public CommandsSequenceSaga()
    {
        InstanceState(x => x.CurrentState, InProcess, Done);

        Event(() => CommandStarted, x => x.CorrelateById(context => context.Message.SequenceId.Id));
        Event(() => CommandCompleted, x => x.CorrelateById(context => context.Message.SequenceId.Id));
            
        Initially(When(CommandStarted)
            .Then(x => x.Instance.Commands.Add(x.Data.CommandId))
            .TransitionTo(InProcess));
        During(InProcess, When(CommandStarted)
            .Then(x => x.Instance.Commands.Add(x.Data.CommandId)));
        During(InProcess, When(CommandCompleted)
            .Then(x => x.Instance.Commands.Remove(x.Data.CommandId))
            .If(x => x.Instance.Commands.Count == 0, x =>
                x.PublishAsync(async x => new EsSequenceCompleted(x.Instance.CorrelationId))
                    .TransitionTo(Done)
                    .Finalize()
            )
        );
            
        SetCompletedWhenFinalized();
    }
}