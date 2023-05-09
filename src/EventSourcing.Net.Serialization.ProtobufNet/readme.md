This package provides functionality to use protobuf-net as the serializer for EventSourcing.Net.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>
    {
        options.Serialization.UseProtobufNet();
    });
}
```

Check [EventSourcing.Net](https://github.com/hmspns/eventsourcing.net) to find full documentation.