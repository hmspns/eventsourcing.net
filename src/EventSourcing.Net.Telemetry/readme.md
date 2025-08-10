
# Telemetry for EventSourcing.Net

This package enables telemetry for event publication and consumption in EventSourcing.Net–based applications. It registers required services and wraps event publishing/handling to collect execution time and throughput metrics.

## Quick start

1) Add the telemetry package to your project.
2) Enable telemetry during EventSourcing.Net configuration.

Example (e.g., in Program.cs or your DI composition root):

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Bootstrap
{
public static void RegisterEventSourcing(IServiceCollection services, IConfiguration configuration)
{
services.AddEventSourcing(options =>
{
            // 1) Configure the event bus BEFORE enabling telemetry.
            // Example: options.Bus = ...;
            // TODO: configure your event bus here.

            // 2) Enable telemetry.
            options.UseTelemetry();
        });
    }
}
```

## Customizing telemetry collection

You can provide your own telemetry collector by registering a custom implementation of IPublicationTelemetryService before calling `UseTelemetry()`:
```
csharp
using Microsoft.Extensions.DependencyInjection;
// using EventSourcing.Net.Telemetry.Contracts; // for IPublicationTelemetryService

public static class Bootstrap
{
public static void RegisterEventSourcing(IServiceCollection services)
{
// Register your implementation BEFORE enabling telemetry
services.AddSingleton<IPublicationTelemetryService, MyPublicationTelemetryService>();

        services.AddEventSourcing(options =>
        {
            // Configure the event bus
            // TODO: configure your event bus here.

            // Enable telemetry — your implementation will be used
            options.UseTelemetry();
        });
    }
}

// Example stub of a custom telemetry collector
public sealed class MyPublicationTelemetryService : IPublicationTelemetryService
{
public void AddTelemetry(Type eventType, Type publisherType, Type consumerType, TimeSpan duration)
{
// TODO: export metrics to your APM/logging/monitoring system
}
}
```
Tips:
- Providing your own IPublicationTelemetryService is useful if you need to export metrics to an existing observability stack.
- Always register your implementation before `options.UseTelemetry()` so it is picked up by the telemetry pipeline.

## Troubleshooting

- Ensure the event bus is configured before calling `UseTelemetry()`.
- If you don’t see metrics:
  - Verify your IPublicationTelemetryService implementation is registered in DI.
  - Add temporary logging inside your implementation to confirm it is being invoked.

## Compatibility

Designed for .NET 7, .NET 8, and .NET 9 projects, including ASP.NET Core and Worker services.

EventSourcing.Net documentation: https://github.com/hmspns/eventsourcing.net
