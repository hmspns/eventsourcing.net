using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class EventSourcingEngineStarter
{
    private IServiceProvider _provider;
    private bool _isStarted = false;

    public EventSourcingEngineStarter(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Start event sourcing engine.
    /// </summary>
    public async Task Start()
    {
        if (_isStarted)
        {
            return;
        }

        ITypeMappingStorageProvider storageTypeMappingProvider = _provider.GetRequiredService<ITypeMappingStorageProvider>();
        await storageTypeMappingProvider.Initialize().ConfigureAwait(false);

        ITypeMappingHandler typeMappingHandler = _provider.GetRequiredService<ITypeMappingHandler>();
        await typeMappingHandler.SynchronizeAppTypesWithStorageTypes();

        IEventSourcingEngine engine = _provider.GetRequiredService<IEventSourcingEngine>();
        EventSourcingEngine.Instance = engine;

        _provider = null;
        _isStarted = true;
    }
    
    
}