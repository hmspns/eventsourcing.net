using System.Collections.Concurrent;
using EventSourcing.Abstractions.Identities;
using EventSourcing.Core.Contracts;

namespace EventSourcing.Core.InMemory;

/// <inheritdoc />
public class InMemoryResolveAppender : IResolveAppender
{
    private readonly ConcurrentDictionary<TenantId, InMemoryAppender> _appenders = new();

    public IAppendOnly Get(TenantId tenantId)
    {
        if (!_appenders.TryGetValue(tenantId, out InMemoryAppender appender))
        {
            appender = new InMemoryAppender();
            if (!_appenders.TryAdd(tenantId, appender)) // already added in another thread.
            {
                appender = _appenders[tenantId];
            }
        }

        return appender;
    }
}