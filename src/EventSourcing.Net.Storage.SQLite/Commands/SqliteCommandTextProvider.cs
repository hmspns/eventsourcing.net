namespace EventSourcing.Net.Storage.SQLite.Commands;

using System.Text;
using System.Text.RegularExpressions;
using Abstractions.Types;

public sealed class SqliteCommandTextProvider : ISqliteCommandTextProvider
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

    private readonly SqliteStorageOptions _options;

    public SqliteCommandTextProvider(SqliteStorageOptions options)
    {
        _options = options;
        BuildInsertEvent();
        BuildInsertCommand();
        BuildSelectStreamVersion();
        BuildSelectStreamData();
        BuildSelectEventsCount();
        BuildSelectStorageExists();
        BuildCreateDataStorage();
        BuildSelectStreamIdsByPattern();
        BuildCreateMetadataStorage();
        BuildSelectTypeMappings();
        BuildInsertTypeMapping();
    }

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

        sb.AppendLine("FROM \"{1}\""); // Убрана схема

        string like = readOptions.FilterType == AggregateStreamFilterType.Include ? "LIKE" : "NOT LIKE";
        string condition = readOptions.FilterType == AggregateStreamFilterType.Include ? "OR" : "AND";
        string where = readOptions.PrefixPattern switch
        {
            null                     => string.Empty,
            var p when p.Length == 1 => $"WHERE {STREAM_NAME} {like} '{p[0]}%'",
            var p when p.Length > 1 => "WHERE " +
                string.Join($" {condition} ", p.Select(x => $"{STREAM_NAME} {like} '{x}%'"))
        };
        sb.AppendLine(where);
        sb.AppendLine($"ORDER BY {GLOBAL_POSITION} " +
            (readOptions.ReadDirection == StreamReadDirection.Forward ? "ASC" : "DESC"));
        sb.AppendLine("LIMIT ? OFFSET ?"); // Замена параметров

        return sb.ToString();
    }

    private void BuildInsertEvent()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{1}"""); // Убрана схема
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
        sb.Append("(?, ?, ?, ?, ?, ?, ?, ?, ?"); // Замена параметров
        int paramCount = 9;
        if (_options.StoreTenantId)
        {
            sb.Append(", ?");
        }

        if (_options.StorePrincipal)
        {
            sb.Append(", ?");
        }

        sb.Append(")");
        InsertEvent = Trim(sb.ToString());
    }

    private void BuildInsertCommand()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{2}"""); // Убрана схема
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
        sb.Append("(?, ?, ?, ?, ?, ?, ?"); // Замена параметров
        int paramCount = 7;
        if (_options.StoreTenantId)
        {
            sb.Append(", ?");
        }

        if (_options.StorePrincipal)
        {
            sb.Append(", ?");
        }

        if (_options.StoreCommandSource)
        {
            sb.Append(", ?");
        }

        sb.Append(")");
        InsertCommand = Trim(sb.ToString());
    }

    private void BuildSelectStreamVersion()
    {
        SelectStreamVersion = $@"SELECT COALESCE(MAX({STREAM_POSITION}), 0) FROM ""{{1}}"" WHERE {STREAM_NAME} = ?";
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
        sb.Append($@"FROM ""{{1}}""
            WHERE {STREAM_NAME} = ?
            ORDER BY {STREAM_POSITION} ASC
            LIMIT ? OFFSET ?;");
        SelectStreamData = Trim(sb.ToString());
    }

    private void BuildSelectEventsCount()
    {
        SelectEventCounts = @"SELECT COUNT(*) FROM ""{1}""";
    }

    private void BuildSelectStorageExists()
    {
        SelectStorageExists = @"SELECT EXISTS (SELECT 1 FROM sqlite_master WHERE type='table' AND name='{1}')";
    }

    private void BuildCreateDataStorage()
    {
        string binaryType = "TEXT";
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($@"CREATE TABLE IF NOT EXISTS ""{{1}}""
            (
                {ID}              TEXT PRIMARY KEY,
                {STREAM_NAME}     TEXT NOT NULL,
                {AGGREGATE_ID_TYPE} TEXT NOT NULL,
                {STREAM_POSITION} INTEGER NOT NULL,
                {GLOBAL_POSITION} INTEGER,
                {TIMESTAMP}       TEXT NOT NULL,
                {COMMAND_ID}      TEXT NOT NULL,
                {SEQUENCE_ID}     TEXT NOT NULL,
                {PAYLOAD_TYPE}    TEXT NOT NULL,
                {PAYLOAD}         {binaryType} NOT NULL");
        if (_options.StoreTenantId)
        {
            sb.Append($",{TENANT_ID} TEXT NOT NULL");
        }

        if (_options.StorePrincipal)
        {
            sb.Append($",{PRINCIPAL_ID} TEXT NOT NULL");
        }

        sb.AppendLine(");");
        sb.AppendLine($@"CREATE UNIQUE INDEX IF NOT EXISTS ""{{1}}__version"" ON ""{{1}}"" ({STREAM_NAME}, {STREAM_POSITION});");

        if (_options.StoreCommands)
        {
            sb.AppendLine($@"CREATE TABLE IF NOT EXISTS ""{{2}}""
                (
                    {ID} TEXT PRIMARY KEY,
                    {PARENT_COMMAND_ID} TEXT,
                    {SEQUENCE_ID} TEXT NOT NULL,
                    {TIMESTAMP} TEXT NOT NULL,
                    {AGGREGATE_ID} TEXT NOT NULL,
                    {PAYLOAD_TYPE} TEXT NOT NULL,
                    {PAYLOAD} {binaryType} NOT NULL");
            if (_options.StoreTenantId)
            {
                sb.Append(", TENANT_ID TEXT NOT NULL");
            }

            if (_options.StorePrincipal)
            {
                sb.Append(", PRINCIPAL_ID TEXT NOT NULL");
            }

            if (_options.StoreCommandSource)
            {
                sb.Append(", COMMAND_SOURCE TEXT NOT NULL");
            }

            sb.AppendLine(");");
        }

        CreateDataStorage = Trim(sb.ToString());
    }

    private void BuildSelectStreamIdsByPattern()
    {
        SelectStreamIdsByPattern = $@"SELECT {STREAM_NAME} FROM ""{{1}}"" WHERE {STREAM_NAME} LIKE ?";
    }

    private void BuildCreateMetadataStorage()
    {
        CreateMappingsStorage = $@"
CREATE TABLE IF NOT EXISTS ""{{1}}""
(
    {ID} TEXT PRIMARY KEY,
    {TYPE_NAME} TEXT NOT NULL UNIQUE
);";
    }

    private void BuildSelectTypeMappings()
    {
        SelectTypeMappings = $@"SELECT {ID}, {TYPE_NAME} FROM ""{{1}}""";
    }

    private void BuildInsertTypeMapping()
    {
        InsertTypeMapping = $@"INSERT INTO ""{{1}}"" ({ID}, {TYPE_NAME}) VALUES (?, ?)";
    }

    private string Trim(string input)
    {
        return Regex.Replace(input, @"[ \t]+", " ");
    }
}