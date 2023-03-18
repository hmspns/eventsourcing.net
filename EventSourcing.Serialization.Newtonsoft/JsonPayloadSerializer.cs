using System.Text;
using EventSourcing.Abstractions;
using Newtonsoft.Json;

namespace EventSourcing.Serialization.Newtonsoft;

/// <inheritdoc />
public class JsonPayloadSerializer : IPayloadSerializer
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        Formatting = Formatting.Indented,
        Converters = new List<JsonConverter>()
        {
            new IdentityConverter()
        }
    };

    private static readonly Dictionary<string, Type> _mappings = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(x => x.GetTypes())
        .Where(x => x.IsAssignableTo(typeof(IEvent))).ToDictionary(x => x.FullName, x => x);
    
    public byte[] Serialize(object obj, out string type)
    {
        type = obj.GetType().FullName;
        string raw = JsonConvert.SerializeObject(obj, _settings);
        return Encoding.UTF8.GetBytes(raw);
    }

    public object Deserialize(byte[] data, string type)
    {
        if (!_mappings.TryGetValue(type, out Type t))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "Couldn't proper type for Event");
        }
        string raw = Encoding.UTF8.GetString(data);
        object result = JsonConvert.DeserializeObject(raw, t, _settings);
        return result;
    }
}