namespace EventSourcing.Net.Samples.Sagas.UserAggregate;

using Engine;
using Abstractions.Contracts;

public class UserStateMutator : StateMutator<UserState>
{
    /// <summary>
    /// Default state
    /// </summary>
    public override UserState DefaultState => new UserState()
    {
        IsCreated = false,
        IsDeleted = false
    };

    public UserStateMutator()
    {
        // here we register our handlers witch will be update the state
        // Guid is type of aggregate id
        // Second type param is type of event
        Register<Guid, UserCreatedEvent>(Handle);
        Register<Guid, UserNameChangedEvent>(Handle);
        Register<Guid, UserPhoneChangedEvent>(Handle);
        Register<Guid, UserDeletedEvent>(Handle);
    }
    
    private UserState Handle(IEventEnvelope<Guid, UserCreatedEvent> e, UserState state)
    {
        state.Name = e.Payload.Name;
        state.BirthDate = e.Payload.BirthDate;
        state.PhoneNumber = e.Payload.PhoneNumber;
        state.IsCreated = true; // mark that current user is created now

        return state;
    }

    private UserState Handle(IEventEnvelope<Guid, UserNameChangedEvent> e, UserState state)
    {
        state.Name = e.Payload.NewName;

        return state;
    }
    
    private UserState Handle(IEventEnvelope<Guid, UserPhoneChangedEvent> e, UserState state)
    {
        state.PhoneNumber = e.Payload.NewPhoneNumber;

        return state;
    }
    
    private UserState Handle(IEventEnvelope<Guid, UserDeletedEvent> e, UserState state)
    {
        state.IsDeleted = true; // mark that current user is deleted now

        return state;
    }
}