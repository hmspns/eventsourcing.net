namespace EventSourcing.Net.Abstractions.Types;

using System;

/// <summary>
/// Delegate for views rebuild event.
/// </summary>
public delegate void ViewsBatchRebuiltEventHandler(object sender, ViewsBatchRebuiltEventArgs args);

/// <summary>
/// Arguments related to views rebuild event.
/// </summary>
public sealed class ViewsBatchRebuiltEventArgs : EventArgs
{
    /// <summary>
    /// Current batch size.
    /// </summary>
    public int BatchSize { get; init; }
    
    /// <summary>
    /// How many events were processed during rebuild of current batch.
    /// </summary>
    public int EventsProcessed { get; init; }
    
    /// <summary>
    /// Position where rebuild of current batch started.
    /// </summary>
    public StreamPosition StartPosition { get; init; }
    
    /// <summary>
    /// Position where rebuild of current batch ended.
    /// </summary>
    public StreamPosition EndPosition { get; init; }
}