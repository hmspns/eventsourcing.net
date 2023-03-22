using System;

namespace EventSourcing.Abstractions.Contracts;

/// <summary>
/// Will add execution time measurement for operation.
/// </summary>
public interface ITimedOperation : IDisposable
{
    /// <summary>
    /// Complete operation.
    /// </summary>
    public void Complete();
}