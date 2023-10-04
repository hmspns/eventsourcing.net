namespace EventSourcing.Net.Samples.CustomIdTypes;

using System.Text;
using Abstractions.Contracts;

public record AddUserCommand(string Name) : ICommand;

public record UserAddedEvent(string Name) : IEvent;

public record UserState
{
    public List<string> Users { get; set; } = new List<string>();

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Users: ");
        foreach (string user in Users)
        {
            builder.AppendLine(user);
        }

        return true;
    }
}