using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Core.Exceptions;
using EventSourcing.Net;
using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Storage.Postgres;

/// <inheritdoc />
public class PgEventSourcingStorage : IEventSourcingStorage
{
    private readonly IResolveAppender _appender;
    private readonly EventSourcingEngineStarter _starter;

    public PgEventSourcingStorage(IResolveAppender appender, EventSourcingEngineStarter starter)
    {
        _starter = starter;
        _appender = appender;
    }
    
    public Task Initialize(TenantId tenantId)
    {
        if (!_starter.IsStarted)
        {
            Thrown.InvalidOperationException("Event sourcing not started. You should start it by call extension method 'StartEventSourcingEngine' on instance of IServiceProvider");
        }
        return _appender.Get(tenantId).Initialize();
    }
}