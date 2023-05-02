using System.Collections.Concurrent;
using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;

namespace EventSourcing.Net.Engine.InMemory;

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