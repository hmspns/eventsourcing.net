using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Core.Implementations;
using EventSourcing.Net.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

public sealed class EventSourcingEngineStarter
{
    private IServiceProvider _provider;
    
    internal bool IsStarted { get; private set; }

    public EventSourcingEngineStarter(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    /// Start event sourcing engine.
    /// </summary>
    public async Task Start()
    {
        if (IsStarted)
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
        IsStarted = true;
    }
}