This package provides functionality to use Postgres DB as the events store.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>  
    {  
        options.Storage.UsePostgresEventStore(configuration.GetConnectionString("EventsDb"));  
    });
}
```

EventSourcing.Net documentation available [here](https://github.com/hmspns/eventsourcing.net).

### Notes

1. If you use non base type (such as int, long, Guid) for the type of aggregate id, you should create TypeConverter for it, to provide conversion from string to the type.