using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine;

namespace EventSourcing.Net.Samples.ProtobufNet.UserAggregate;

public class UserAggregate : Aggregate<Guid, UserState, UserStateMutator>
{
    public UserAggregate(Guid id) : base(id, new UserStateMutator())
    {
    }

    public CommandExecutionResult<Guid> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd)
    {
        if (!State.IsCreated)
        {
            Apply(cmd, new UserCreatedEvent(cmd.Payload.Name, cmd.Payload.BirthDate, cmd.Payload.PhoneNumber));
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public CommandExecutionResult<Guid> UpdateUser(ICommandEnvelope<Guid, UpdateUserCommand> cmd)
    {
        if (!State.IsCreated || State.IsDeleted)
        {
            return CommandExecutionResult<Guid>.NotExists(cmd);
        }

        if (!string.Equals(State.Name, cmd.Payload.Name, StringComparison.Ordinal))
        {
            Apply(cmd, new UserNameChangedEvent(State.Name, cmd.Payload.Name));
        }

        if (!string.Equals(State.PhoneNumber, cmd.Payload.PhoneNumber, StringComparison.Ordinal))
        {
            Apply(cmd, new UserPhoneChangedEvent(State.PhoneNumber, cmd.Payload.PhoneNumber));
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public CommandExecutionResult<Guid> DeleteUser(ICommandEnvelope<Guid, DeleteUserCommand> cmd)
    {
        if (!State.IsCreated)
        {
            return CommandExecutionResult<Guid>.NotExists(cmd);
        }

        if (!State.IsDeleted) // don't apply duplicate event
        {
            Apply(cmd, new UserDeletedEvent());
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }
}