using System;

namespace EventSourcing.Net.Abstractions.Contracts;

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