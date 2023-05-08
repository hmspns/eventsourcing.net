using EventSourcing.Net.Abstractions.Contracts;

namespace EventSourcing.Net;

/// <summary>
/// Options to configure serialization.
/// </summary>
public sealed class EventSourcingSerializationOptions
{
    private readonly EventSourcingOptions _options;

    internal EventSourcingOptions EventSourcingOptions => _options;

    public EventSourcingSerializationOptions(EventSourcingOptions options)
    {
        _options = options;
    }

    /// <summary>
    /// Configure EventSourcing.Net to use System.Text.Json serialization.
    /// </summary>
    /// <param name="configurator">Callback to configure serialization.</param>
    /// <remarks>It's not necessary to call this method if you don't have a plan to use custom JsonSerializerOptions options.</remarks>
    public void UseSystemTextJson(Action<SystemTextJsonSerializationOptions> configurator = null)
    {
        SystemTextJsonSerializationOptions options = new SystemTextJsonSerializationOptions();
        configurator?.Invoke(options);
        
        _options.Services.RegisterEventsSerialization(options.PayloadSerializationOptions);
        _options.Services.RegisterSnapshotSerialization(options.SnapshotSerializationOptions);
    }
}