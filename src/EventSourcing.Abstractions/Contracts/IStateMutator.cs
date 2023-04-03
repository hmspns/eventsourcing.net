namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Mutator of the state.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
public interface IStateMutator<out TState> where TState : class
{
    /// <summary>
    /// Default state value.
    /// </summary>
    TState DefaultState { get; }

    /// <summary>
    /// Apply event to the state.
    /// </summary>
    /// <param name="eventEnvelope">Event that should be applied.</param>
    /// <returns>State after changes.</returns>
    TState Transition(IEventEnvelope eventEnvelope);
        
    /// <summary>
    /// Replace state with new state.
    /// </summary>
    /// <param name="state">New state.</param>
    /// <returns>New state.</returns>
    internal TState Transition(object state);

    /// <summary>
    /// Return current state.
    /// </summary>
    TState Current { get; }
}