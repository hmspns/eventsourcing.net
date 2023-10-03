namespace EventSourcing.Net.Samples.Sagas.UsersListAggregate;

using System.Text;

public record UsersListState
{
    public Dictionary<Guid, string> Users { get; set; } = new Dictionary<Guid, string>();

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.AppendLine("Users: ");
        foreach (KeyValuePair<Guid,string> pair in Users)
        {
            builder.AppendLine($"Id: {pair.Key}, Name: {pair.Value}");
        }

        return true;
    }
}