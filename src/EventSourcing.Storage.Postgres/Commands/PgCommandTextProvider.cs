using System.Text;

namespace EventSourcing.Storage.Postgres;

public class PgCommandTextProvider : IPgCommandTextProvider
{
    private PgStorageOptions _options;
    public string InsertEvent { get; private set; }
    
    public string InsertCommand { get; private set; }
    
    public string GetStreamDataText { get; private set; }
    
    public string SelectStreamVersion { get; private set; }

    public virtual void Initialize(PgStorageOptions options)
    {
        _options = options;
        BuildInsertEvent();
        BuildInsertCommand();
        BuildSelectStreamVersion();
        BuildSelectStreamData();
    }
    
    protected virtual void BuildInsertEvent()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{0}"".""{1}""");
        sb.Append("(id, stream_name, stream_position, timestamp, command_id, sequence_id, payload_type, payload");
        if (_options.UseMultitenancy)
        {
            sb.Append(", tenant_id");
        }

        if (_options.StorePrincipal)
        {
            sb.Append(", principal_id");
        }

        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.Append("($1, $2, $3, $4, $5, $6, $7, $8");

        bool nineAdded = false;
        if (_options.UseMultitenancy)
        {
            sb.Append(", $9");
            nineAdded = true;
        }

        if (_options.StorePrincipal && nineAdded)
        {
            sb.Append(", $10");
        }

        sb.Append(")");
        
        InsertEvent = sb.ToString();
    }

    protected virtual void BuildInsertCommand()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"INSERT INTO ""{0}"".""{1}""");
        sb.Append("(id, parent_command_id, sequence_id, timestamp, aggregate_id, payload_type, payload");
        
        if (_options.UseMultitenancy)
        {
            sb.Append(", tenant_id");
        }
        if (_options.StorePrincipal)
        {
            sb.Append(", principal_id");
        }
        if (_options.StoreCommandSource)
        {
            sb.Append(", command_source");
        }
        
        sb.AppendLine(")");
        sb.AppendLine("VALUES");
        sb.Append("($1, $2, $3, $4, $5, $6, $7");
        
        bool eightAdded = false;
        if (_options.UseMultitenancy)
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
        
        InsertCommand = sb.ToString();
    }

    protected virtual void BuildSelectStreamVersion()
    {
        SelectStreamVersion = @"SELECT COALESCE(MAX(stream_position), 0) FROM ""{0}"".""{1}"" WHERE stream_name = $1";
    }

    protected virtual void BuildSelectStreamData()
    {
        

        const string GET_STREAM_DATA =
            @"
SELECT id, tenant_id, stream_position, ""timestamp"", command_id, sequence_id, principal_id, payload_type, payload
FROM ""{0}"".""{1}""
WHERE stream_name = $1
ORDER BY stream_position ASC
LIMIT $2 OFFSET $3;";

        StringBuilder sb = new StringBuilder();
        
    }
}