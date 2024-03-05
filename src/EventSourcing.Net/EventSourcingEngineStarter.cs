using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Abstractions.Identities;
using EventSourcing.Net.Engine.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.Net;

using Abstractions;

/// <summary>
/// Start event sourcing engine.
/// </summary>
public sealed class EventSourcingEngineStarter
{
    private IServiceProvider _provider;
    private readonly IEventSourcingStatus _status;

    internal bool IsStarted { get; private set; }

    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="provider">Service provider.</param>
    public EventSourcingEngineStarter(IServiceProvider provider, IEventSourcingStatus status)
    {
        _status = status;
        _provider = provider;
    }

    /// <summary>
    /// Start event sourcing engine.
    /// </summary>
    /// <param name="initializeStorageForDefaultTenant">Should storage be initialized for default tenant (TenantId.Empty).</param>
    /// <remarks>For multitenancy environment you should manually initialize storage per tenant.</remarks>
    public async Task Start(bool initializeStorageForDefaultTenant = true)
    {
        if (IsStarted)
        {
            return;
        }

        ITypeMappingStorageProvider storageTypeMappingProvider = _provider.GetRequiredService<ITypeMappingStorageProvider>();
        await storageTypeMappingProvider.Initialize().ConfigureAwait(false);

        ITypeMappingHandler typeMappingHandler = _provider.GetRequiredService<ITypeMappingHandler>();
        await typeMappingHandler.SynchronizeAppTypesWithStorageTypes().ConfigureAwait(false);

        IEventSourcingEngine engine = _provider.GetRequiredService<IEventSourcingEngine>();
        EventSourcingEngine.Instance = engine;
        
        IsStarted = true;

        EventSourcingStatus castedStatus = (EventSourcingStatus)_status;
        castedStatus.IsStarted = true;

        if (initializeStorageForDefaultTenant)
        {
            IEventSourcingStorage resolveEventStore = _provider.GetRequiredService<IEventSourcingStorage>();
            await resolveEventStore.Initialize().ConfigureAwait(false);
        }
        
        _provider = null; // don't handle reference to the IServiceProvider.
    }
}