using System.Threading;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net.Engine.Extensions;

internal static class CancellationExtensions
{
    internal static bool CancellationWasRequested<TId>(
        this CancellationToken cancellationToken,
        ICommandEnvelope<TId> cmd,
        out ICommandExecutionResult<TId> result)
    {
        result = null;
        if (cancellationToken.IsCancellationRequested)
        {
            result = new CommandExecutionResult<TId>(cmd, false, false, "Cancellation was requested");
            return true;
        }

        return false;
    }
}