using EventSourcing.Net.Abstractions.Contracts;
using EventSourcing.Net.Engine.Serialization;

namespace EventSourcing.Net;

/// <summary>
/// Options to configure serialization.
/// </summary>
public sealed class EventSourcingSerializationOptions : SpecificOptions
{
    /// <summary>
    /// Initialize new object.
    /// </summary>
    /// <param name="options">Event sourcing options.</param>
    public EventSourcingSerializationOptions(EventSourcingOptions options) : base(options.Services)
    {
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

        ReplaceSingleton<IPayloadSerializerFactory>(x => new SystemTextJsonPayloadSerializerFactory(options.PayloadSerializationOptions));
        ReplaceSingleton<ISnapshotSerializerFactory>(x => new SystemTextJsonSnapshotSerializerFactory(options.SnapshotSerializationOptions));
    }
}