namespace EventSourcing.Net.Abstractions.Contracts;

using System.Threading.Tasks;
using Identities;
using Types;

public interface IViewsRebuilder
{
    /// <summary>
    /// Raised when rebuild of batch done.
    /// </summary>
    event ViewsBatchRebuiltEventHandler OnBatchRebuilt;

    /// <summary>
    /// Rebuild views for default tenant.
    /// </summary>
    /// <param name="batchSize">Batch size for iteration.</param>
    Task Rebuild(int batchSize = 1000);
    
    /// <summary>
    /// Rebuild views by custom read options.
    /// </summary>
    /// <param name="tenantId">Id of tenant to rebuild.</param>
    /// <param name="readOptions">Options how to read events.</param>
    Task Rebuild(TenantId tenantId, StreamReadOptions readOptions);
    
    /// <summary>
    /// Rebuild views for tenant.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <param name="batchSize">Batch size for iteration.</param>
    Task RebuildTenant(TenantId tenantId, int batchSize = 1000);
}