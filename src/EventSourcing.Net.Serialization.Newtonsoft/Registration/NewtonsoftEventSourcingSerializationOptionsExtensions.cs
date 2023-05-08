using EventSourcing.Net.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace EventSourcing.Net.Serialization.Newtonsoft;

/// <summary>
/// Provide extensions for registration.
/// </summary>
public static class NewtonsoftEventSourcingSerializationOptionsExtensions
{
    /// <summary>
    /// Configure EventSourcing.Net to use System.Text.Json serialization.
    /// </summary>
    /// <param name="options">Serialization options.</param>
    /// <param name="configurator">Callback to configure serialization.</param>
    public static void UseNewtonsoftJson(this EventSourcingSerializationOptions options, Action<NewtonsoftJsonSerializerOptions>? configurator = null)
    {
        NewtonsoftJsonSerializerOptions o = new NewtonsoftJsonSerializerOptions();
        configurator?.Invoke(o);
        options.EventSourcingOptions.Services.Replace<IPayloadSerializerFactory>(services =>
        {
            services.AddSingleton<IPayloadSerializerFactory>(x => new NewtonsoftJsonPayloadSerializerFactory(o.PayloadSerializationOptions));
        });
        options.EventSourcingOptions.Services.Replace<ISnapshotSerializerFactory>(services =>
        {
            services.AddSingleton<ISnapshotSerializerFactory>(x => new NewtonsoftJsonSnapshotSerializerFactory(o.SnapshotSerializationOptions));
        });
    }
}

/// <summary>
/// Options to configure Newtonsoft.Json serialization.
/// </summary>
public sealed class NewtonsoftJsonSerializerOptions
{
    /// <summary>
    /// Get serialization options for snapshots.
    /// </summary>
    public JsonSerializerSettings? SnapshotSerializationOptions { internal get; set; }
    
    /// <summary>
    /// Get serialization options for events and commands.
    /// </summary>
    public JsonSerializerSettings? PayloadSerializationOptions { internal get; set; }
}