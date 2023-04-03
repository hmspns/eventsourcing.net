using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;
using Npgsql;

namespace EventSourcing.Storage.Postgres;

/// <inheritdoc />
public sealed class PgAppenderResolver : IResolveAppender
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IPayloadSerializer _serializer;

    public PgAppenderResolver(string connectionString, IPayloadSerializer serializer)
    {
        _serializer = serializer;

        NpgsqlDataSourceBuilder builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNodaTime();
        _dataSource = builder.Build();
    }
        
    public IAppendOnly Get(TenantId tenantId)
    {
        return new PgSqlAppender(_serializer, _dataSource, tenantId.ToString());
    }
}