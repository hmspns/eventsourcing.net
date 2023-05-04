This package provides functionality to use Redis as the snapshot store.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>  
    {  
        options.UseRedisSnapshotStore(configuration.GetConnectionString("Redis"));  
    }); 
}
```

Check [EventSourcing.Net](https://www.nuget.org/packages/EventSourcing.Net/) to find documentation.