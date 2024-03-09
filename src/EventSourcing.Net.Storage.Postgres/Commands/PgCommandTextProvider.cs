using System.Text;
using System.Text.RegularExpressions;
using EventSourcing.Net.Abstractions.Types;

namespace EventSourcing.Net.Storage.Postgres;

public sealed class PgCommandTextProvider : IPgCommandTextProvider
{
    internal const string ID = "id";
    internal const string TENANT_ID = "tenant_id";
    internal const string STREAM_NAME = "stream_name";
    internal const string STREAM_POSITION = "stream_position";
    internal const string GLOBAL_POSITION = "global_position";
    internal const string TIMESTAMP = "timestamp";
    internal const string COMMAND_ID = "command_id";
    internal const string SEQUENCE_ID = "sequence_id";
    internal const string PRINCIPAL_ID = "principal_id";
    internal const string PAYLOAD_TYPE = "payload_type";
    internal const string PAYLOAD = "payload";
    internal const string PARENT_COMMAND_ID = "parent_command_id";
    internal const string AGGREGATE_ID = "aggregate_id";
    internal const string COMMAND_SOURCE = "command_source";
    internal const string TYPE_NAME = "type_name";
    internal const string AGGREGATE_ID_TYPE = "aggregate_id_type";

    private readonly PgStorageOptions _options;

    public string InsertEvent { get; private set; }

    public string InsertCommand { get; private set; }

    public string SelectStreamData { get; private set; }

    public string SelectStreamVersion { get; private set; }

    public string SelectEventCounts { get; private set; }

    public string SelectStorageExists { get; private set; }

    public string CreateDataStorage { get; private set; }

    public string SelectStreamIdsByPattern { get; private set; }
    
    public string CreateMappingsStorage { get; private set; }
    
    public string SelectTypeMappings { get; private set; }
    
    public string InsertTypeMapping { get; private set; }

    public PgCommandTextProvider(PgStorageOptions options)
    {
        _options = options;
        BuildInsertEvent();
        BuildInsertCommand();
        BuildSelectStreamVersion();
        BuildSelectStreamData();
        BuildSelectEventsCount();
        BuildSelectStorageExists();
        BuildSelectStorageExists();
        BuildCreateDataStorage();
        BuildSelectStreamIdsByPattern();
        BuildCreateMetadataStorage();
        BuildSelectTypeMappings();
        BuildInsertTypeMapping();
    }
    
    public string BuildReadAllStreamsCommandText(StreamReadOptions readOptions)
    {
        StringBuilder sb = new StringBuilder(2048);
        string tenantString = _options.StoreTenantId ? $"{TENANT_ID}," : string.Empty;
        string principalString = _options.StorePrincipal ? $"{PRINCIPAL_ID}," : string.Empty;
        switch (readOptions.ReadingVolume)
        {
            case StreamReadVolume.Data:
                sb.AppendLine(
                    $"SELECT {ID}, {tenantString} {AGGREGATE_ID_TYPE}, {STREAM_NAME}, {STREAM_POSITION}, \"{TIMESTAMP}\", {PAYLOAD_TYPE}, {PAYLOAD}");
                break;
            
            case StreamReadVolume.Meta:
                sb.AppendLine(
                    $"SELECT {ID}, {tenantString} {AGGREGATE_ID_TYPE}, {STREAM_NAME}, {STREAM_POSITION}, \"{TIMESTAMP}\", {COMMAND_ID}, {SEQUENCE_ID}, {principalString} {PAYLOAD_TYPE}");
                break;

            default:
                sb.AppendLine(
                    $"SELECT {ID}, {tenantString} {AGGREGATE_ID_TYPE}, {STREAM_NAME}, {STREAM_POSITION}, \"{TIMESTAMP}\", {COMMAND_ID}, {SEQUENCE_ID}, {principalString} {PAYLOAD_TYPE}, {PAYLOAD}");
                break;
        }

        sb.AppendLine("FROM \"{0}\".\"{1}\"");

        string like = readOptions.FilterType == AggregateStreamFilterType.Include ? "LIKE" : "NOT LIKE";
        string condition = readOptions.FilterType == AggregateStreamFilterType.Include ? "OR" : "AND";
        string where = readOptions.PrefixPattern switch
        {
            null => string.Empty,
            var p when p.Length == 1 => $"WHERE {STREAM_NAME} {like} '{p[0]}%'",
            var p when p.Length > 1 => "WHERE " +
                                       string.Join($" {condition} ", p.Select(x => $"{STREAM_NAME} {like} '{x}%'"))
        };
        sb.AppendLine(where);
        sb.AppendLine($"ORDER BY {GLOBAL_POSITION} " +
                      (readOptions.ReadDirection == StreamReadDirection.Forward ? "ASC" : "DESC"));
        sb.AppendLine("LIMIT $1 OFFSET $2");

        return sb.ToString();
    }

    private void BuildInsertEvent()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{0}"".""{1}""");
        sb.Append($"({ID}, {STREAM_NAME}, {AGGREGATE_ID_TYPE}, {STREAM_POSITION}, {TIMESTAMP}, {COMMAND_ID}, {SEQUENCE_ID}, {PAYLOAD_TYPE}, {PAYLOAD}");
        if (_options.StoreTenantId)
        {
            sb.Append($", {TENANT_ID}");
        }

        if (_options.StorePrincipal)
        {
            sb.Append($", {PRINCIPAL_ID}");
        }

        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.Append("($1, $2, $3, $4, $5, $6, $7, $8, $9");

        bool tenAdded = false;
        if (_options.StoreTenantId)
        {
            sb.Append(", $10");
            tenAdded = true;
        }

        if (_options.StorePrincipal && tenAdded)
        {
            sb.Append(", $11");
        }

        sb.Append(")");

        InsertEvent = Trim(sb.ToString());
    }

    private void BuildInsertCommand()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{0}"".""{1}""");
        sb.Append($"({ID}, {PARENT_COMMAND_ID}, {SEQUENCE_ID}, {TIMESTAMP}, {AGGREGATE_ID}, {PAYLOAD_TYPE}, {PAYLOAD}");

        if (_options.StoreTenantId)
        {
            sb.Append($", {TENANT_ID}");
        }

        if (_options.StorePrincipal)
        {
            sb.Append($", {PRINCIPAL_ID}");
        }

        if (_options.StoreCommandSource)
        {
            sb.Append($", {COMMAND_SOURCE}");
        }

        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.Append("($1, $2, $3, $4, $5, $6, $7");

        bool eightAdded = false;
        if (_options.StoreTenantId)
        {
            sb.Append(", $8");
            eightAdded = true;
        }

        bool nineAdded = false;
        if (_options.StorePrincipal && eightAdded)
        {
            sb.Append(", $9");
            nineAdded = true;
        }

        if (_options.StoreCommandSource && eightAdded && nineAdded)
        {
            sb.Append(", $10");
        }

        sb.Append(")");

        InsertCommand = Trim(sb.ToString());
    }

    private void BuildSelectStreamVersion()
    {
        SelectStreamVersion = $@"SELECT COALESCE(MAX({STREAM_POSITION}), 0) FROM ""{{0}}"".""{{1}}"" WHERE {STREAM_NAME} = $1";
    }

    private void BuildSelectStreamData()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"SELECT {ID}, {STREAM_POSITION}, \"{TIMESTAMP}\", {COMMAND_ID}, {SEQUENCE_ID}, {PAYLOAD_TYPE}, {PAYLOAD}");
        if (_options.StoreTenantId)
        {
            sb.Append($", {TENANT_ID}");
        }

        if (_options.StorePrincipal)
        {
            sb.Append($", {PRINCIPAL_ID}");
        }

        sb.AppendLine();

        sb.Append(
            $@"FROM ""{{0}}"".""{{1}}""
            WHERE {STREAM_NAME} = $1
            ORDER BY {STREAM_POSITION} ASC
            LIMIT $2 OFFSET $3;");

        SelectStreamData = Trim(sb.ToString());
    }

    private void BuildSelectEventsCount()
    {
        SelectEventCounts = @"SELECT COUNT(*) FROM ""{0}"".""{1}""";
    }

    private void BuildSelectStorageExists()
    {
        SelectStorageExists = @"
        SELECT EXISTS (
            SELECT FROM information_schema.tables 
            WHERE table_schema = '{0}' AND table_name = '{1}')";
    }

    private void BuildCreateDataStorage()
    {
        string binaryType = _options.BinaryDataPostgresType == BinaryDataPostgresType.JsonB ? "jsonb" : "bytea";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(
            $@"CREATE SCHEMA IF NOT EXISTS ""{{0}}"";
            CREATE TABLE IF NOT EXISTS ""{{0}}"".""{{1}}""
            (
                {ID}              uuid constraint ""{{1}}_pk"" primary key,"
        );
        if (_options.StoreTenantId)
        {
            sb.AppendLine($"{TENANT_ID}       uuid         not null,");
        }

        sb.AppendLine(
           $@"{STREAM_NAME}     varchar(255) not null,
              {AGGREGATE_ID_TYPE}         uuid         not null,
              {STREAM_POSITION} bigint       not null,
              {GLOBAL_POSITION} bigint GENERATED ALWAYS AS identity,
              {TIMESTAMP}       timestamptz  not null,
              {COMMAND_ID}      uuid         not null,
              {SEQUENCE_ID}     uuid         not null,");
        if (_options.StorePrincipal)
        {
            sb.AppendLine($"{PRINCIPAL_ID}    varchar(255) not null,");
        }

        sb.AppendLine(
            $@"{PAYLOAD_TYPE}    uuid not null,
                {PAYLOAD}         {binaryType}        not null
            );

            CREATE UNIQUE INDEX IF NOT EXISTS ""{{1}}__version""
                ON ""{{0}}"".""{{1}}"" ({STREAM_NAME}, {STREAM_POSITION});
        ");
        
        if (_options.StoreCommands)
        {
            sb.AppendLine($@"
            CREATE TABLE IF NOT EXISTS ""{{0}}"".""{{2}}""
            (
                {ID}                uuid         not null
                    constraint ""{{2}}_pk""
                        primary key,
                {PARENT_COMMAND_ID} uuid         null,
                {SEQUENCE_ID}       uuid         not null,"
            );

            if (_options.StoreTenantId)
            {
                sb.AppendLine($"{TENANT_ID}         uuid         not null,");
            }

            sb.AppendLine(
                $@"{TIMESTAMP}         timestamptz  not null,
            {AGGREGATE_ID}      varchar(255) not null,");

            if (_options.StorePrincipal)
            {
                sb.AppendLine($"{PRINCIPAL_ID}      varchar(255) not null,");
            }

            if (_options.StoreCommandSource)
            {
                sb.AppendLine($"{COMMAND_SOURCE}    varchar(255) not null,");
            }

            sb.AppendLine(
                $@"{PAYLOAD_TYPE}      uuid not null,
            {PAYLOAD}           {binaryType}        not null
        );");
        }

        CreateDataStorage = Trim(sb.ToString());
    }

    private void BuildSelectStreamIdsByPattern()
    {
        SelectStreamIdsByPattern = $@"SELECT {STREAM_NAME} FROM ""{{0}}"".""{{1}}"" WHERE {STREAM_NAME} LIKE $1";
    }

    private string Trim(string input)
    {
        Regex re = new Regex(@"[ \t]+", RegexOptions.Multiline);
        return re.Replace(input, " ");
    }

    private void BuildCreateMetadataStorage()
    {
        CreateMappingsStorage = $@"
CREATE SCHEMA IF NOT EXISTS ""{{0}}"";
CREATE TABLE IF NOT EXISTS ""{{0}}"".""{{1}}""
(
	{ID} uuid NOT NULL,
	{TYPE_NAME} TEXT NOT NULL,
	PRIMARY KEY ({ID}),
	CONSTRAINT type_mappings_pk
		UNIQUE ({TYPE_NAME})
);
";
    }

    private void BuildSelectTypeMappings()
    {
        SelectTypeMappings = $@"SELECT {ID}, {TYPE_NAME} FROM ""{{0}}"".""{{1}}""";
    }

    private void BuildInsertTypeMapping()
    {
        InsertTypeMapping = $@"
            INSERT INTO ""{{0}}"".""{{1}}""
            ({ID}, {TYPE_NAME})
            VALUES
            ($1, $2)
        ";
    }
}