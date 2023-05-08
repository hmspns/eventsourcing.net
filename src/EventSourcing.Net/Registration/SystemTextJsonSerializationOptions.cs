using System.Text.Json;

namespace EventSourcing.Net;

/// <summary>
/// Options to configure System.Text.Json serialization.
/// </summary>
public sealed class SystemTextJsonSerializationOptions
{
    /// <summary>
    /// Get serialization options for snapshots.
    /// </summary>
    public JsonSerializerOptions? SnapshotSerializationOptions { internal get; set; }
    
    /// <summary>
    /// Get serialization options for events and commands.
    /// </summary>
    public JsonSerializerOptions? PayloadSerializationOptions { internal get; set; }
}