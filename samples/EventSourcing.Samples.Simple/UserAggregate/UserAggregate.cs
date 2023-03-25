using EventSourcing.Abstractions.Contracts;
using EventSourcing.Core;

namespace EventSourcing.Samples.Simple.UserAggregate;

public class UserAggregate : Aggregate<Guid, UserState, UserStateMutator>
{
    public UserAggregate(Guid id) : base(id, new UserStateMutator())
    {
    }

    public CommandExecutionResult<Guid> CreateUser(ICommandEnvelope<Guid, CreateUserCommand> cmd)
    {
        if (!State.Current.IsCreated)
        {
            Apply(cmd, new UserCreatedEvent(cmd.Payload.Name, cmd.Payload.BirthDate, cmd.Payload.PhoneNumber));
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public CommandExecutionResult<Guid> UpdateUser(ICommandEnvelope<Guid, UpdateUserCommand> cmd)
    {
        if (!State.Current.IsCreated || State.Current.IsDeleted)
        {
            return CommandExecutionResult<Guid>.NotExists(cmd);
        }

        if (!string.Equals(State.Current.Name, cmd.Payload.Name, StringComparison.Ordinal))
        {
            Apply(cmd, new UserNameChangedEvent(State.Current.Name, cmd.Payload.Name));
        }

        if (!string.Equals(State.Current.PhoneNumber, cmd.Payload.PhoneNumber, StringComparison.Ordinal))
        {
            Apply(cmd, new UserPhoneChangedEvent(State.Current.PhoneNumber, cmd.Payload.PhoneNumber));
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }

    public CommandExecutionResult<Guid> DeleteUser(ICommandEnvelope<Guid, DeleteUserCommand> cmd)
    {
        if (!State.Current.IsCreated)
        {
            return CommandExecutionResult<Guid>.NotExists(cmd);
        }

        if (!State.Current.IsDeleted) // don't apply duplicate event
        {
            Apply(cmd, new UserDeletedEvent());
        }
        
        return CommandExecutionResult<Guid>.OkIfChanges(this, cmd);
    }
}