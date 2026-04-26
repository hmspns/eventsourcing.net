namespace EventSourcing.Net.Samples.Rebuild;

using Abstractions.Contracts;
using Abstractions.Identities;
using Abstractions.Types;
using Engine.InMemory;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// This class is used to simulate storage. It's not necessary for real usage.
/// </summary>
public class VirtualStorage
{
    private readonly IResolveAppender _resolver;
    private readonly ITypeMappingStorageProvider _typeMappingStorageProvider;
    private readonly TypeMappingId _guidMappingId;
    
    public VirtualStorage()
    {
        _guidMappingId = TypeMappingId.New();
        _resolver = new VirtualAppenderResolver(_guidMappingId);
        _typeMappingStorageProvider = new InMemoryTypeMappingStorageProvider();
    }

    public async Task Initialize()
    {
        await _typeMappingStorageProvider.AddMappings([new TypeMapping(_guidMappingId, typeof(Guid).FullName)]);
    }

    public void AddServices(IServiceCollection services)
    {
        services.AddScoped<IResolveAppender>(x => _resolver);
        services.AddSingleton(_typeMappingStorageProvider);
    }
}