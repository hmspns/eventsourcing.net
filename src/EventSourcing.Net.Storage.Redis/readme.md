This package provides functionality to use Redis as the snapshot store.

### Configuration example

```csharp
public void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)  
{  
    services.AddEventSourcing(options =>  
    {  
        options.Storage.UseRedisSnapshotStore(configuration.GetConnectionString("Redis"));  
    }); 
}
```

Check [EventSourcing.Net](https://github.com/hmspns/eventsourcing.net) to find full documentation.