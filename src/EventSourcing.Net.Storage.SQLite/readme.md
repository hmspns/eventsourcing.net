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

1. If you use non primitive type (such as int, long, Guid) for the type of aggregate id, you should create TypeConverter for it, to provide conversion from string to the type.
2. By default Postgres using `JsonB` as the data type for payload. If you would like binary serialization (with `EventSourcing.Net.Serialization.ProtobufNet` for example), you should configure storage provider to use `ByteA`:
```csharp
    services.AddEventSourcing(options =>  
    {  
        options.Storage.UsePostgresEventStore(configuration.GetConnectionString("EventsDb"), config => {
            config.BinaryDataPostgresType = BinaryDataPostgresType.ByteA;
        });  
    });
```