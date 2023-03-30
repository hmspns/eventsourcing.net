using EventSourcing.Abstractions;
using EventSourcing.Abstractions.Contracts;
using EventSourcing.Abstractions.Identities;

namespace EventSourcing.Storage.Postgres
{
    /// <inheritdoc />
    public sealed class PgAppenderResolver : IResolveAppender
    {
        private readonly string _connectionString;
        private readonly IPayloadSerializer _serializer;

        public PgAppenderResolver(string connectionString, IPayloadSerializer serializer)
        {
            _serializer = serializer;
            _connectionString = connectionString;
        }
        
        public IAppendOnly Get(TenantId tenantId)
        {
            return new PgSqlAppender(_serializer, _connectionString, tenantId.ToString());
        }
    }
}