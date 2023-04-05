using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Postgres;

/// <summary>
/// Configuration for postgres events storage.
/// </summary>
public sealed class PgStorageOptions
{
    /// <summary>
    /// Does storage support multitenancy.
    /// If true every tenant will have own schema that can be configured by MultitenancySchemaName property.
    /// Otherwise all data will be stored in the same schema that can be configured by NonMultitenancySchemaName property.
    /// </summary>
    public bool UseMultitenancy { get; set; } = false;
    
    /// <summary>
    /// Store tenant id in tables.
    /// </summary>
    public bool StoreTenantId { get; set; } = true;

    /// <summary>
    /// Store information about source of command.
    /// This configuration applying to the commands table, so if property StoreCommands == false, command source won't be stored.
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

    /// <summary>
    /// Postgres type to store binary data.
    /// </summary>
    public BinaryDataPostgresType BinaryDataPostgresType { get; set; } = BinaryDataPostgresType.JsonB;
}

public enum BinaryDataPostgresType
{
    JsonB,
    ByteA
}