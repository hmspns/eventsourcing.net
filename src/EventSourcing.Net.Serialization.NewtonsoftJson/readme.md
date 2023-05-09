This package provides functionality to use Newtonsoft.Json as the serializer for EventSourcing.Net.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>
    {
        options.Serialization.UseNewtonsoftJson();
    });
}
```

Check [EventSourcing.Net](https://github.com/hmspns/eventsourcing.net) to find full documentation.