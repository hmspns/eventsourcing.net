namespace EventSourcing.Net.Storage.SQLite;

using Abstractions.Contracts;
using Abstractions.Identities;
using Engine.Exceptions;

/// <inheritdoc />
public class SqliteEventSourcingStorage : IEventSourcingStorage
{
    private readonly IResolveAppender _appender;
    private readonly EventSourcingEngineStarter _starter;

    public SqliteEventSourcingStorage(IResolveAppender appender, EventSourcingEngineStarter starter)
    {
        _starter = starter;
        _appender = appender;
    }
    
    public Task Initialize(TenantId tenantId)
    {
        if (!_starter.IsStarted)
        {
            Thrown.InvalidOperationException("Event sourcing not started. You should start it by call extension method 'StartEventSourcingEngine' on instance of the IServiceProvider");
        }
        return _appender.Get(tenantId).Initialize();
    }
}