namespace EventSourcing.Net.Samples.Rebuild.UserAggregate;

using Abstractions.Contracts;

public class UserRebuildProjector : UserProjector,
    IPublicationStartedHandler,
    IPublicationCompletionHandler
{
    public async Task PublicationStarting(BatchEvents matchedEvents)
    {
        IEnumerable<IEventEnvelope<Guid>> events = matchedEvents.GetTypedEvents<Guid>();
        
        // do some preload here
        
        Console.WriteLine("Preload");
    }
    
    public async Task PublicationDone()
    {
        // here we can call DbContext.SaveChangesAsync()
        Console.WriteLine("Done");
    }
}