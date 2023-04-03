using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Postgres;

/// <summary>
/// Configuration for postgres events storage.
/// </summary>
public sealed class PgStorageOptions
{
    /// <summary>
    /// Store information about tenants.
    /// </summary>
    public bool UseMultitenancy { get; set; } = true;

    /// <summary>
    /// Store information about source of command.
    /// </summary>
    public bool StoreCommandSource { get; set; } = true;

    /// <summary>
    /// Store information about principal who raise the command.
    /// </summary>
    public bool StorePrincipal { get; set; } = true;

    /// <summary>
    /// Store information about sent commands.
    /// </summary>
    /// <remarks>Commands will be stored in the additional table.</remarks>
    public bool StoreCommands { get; set; } = true;

    /// <summary>
    /// Name of events table.
    /// </summary>
    public string EventsTableName { get; set; } = "events";

    /// <summary>
    /// Name of commands table.
    /// </summary>
    public string CommandsTableName { get; set; } = "commands";

    /// <summary>
    /// Schema that will be using when multitenancy switched off.
    /// </summary>
    public string NonMultitenancySchemaName { get; set; } = "public";

    /// <summary>
    /// Handler to get schema name. This name will be using when multitenancy switched on.
    /// </summary>
    public Func<TenantId, string> MultitenancySchemaName { get; set; } = (x) => x.ToString();
}