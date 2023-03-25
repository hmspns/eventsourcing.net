namespace EventSourcing.Net.Internal;

internal sealed class EventConsumers
{
    private readonly Dictionary<Type, List<EventConsumerActivation>> _hash = new();

    internal void Add(Type eventType, EventConsumerActivation activation)
    {
        if (!_hash.TryGetValue(eventType, out List<EventConsumerActivation> values))
        {
            values = new List<EventConsumerActivation>();
            _hash.Add(eventType, values);
        }
        values.Add(activation);
    }

    internal Dictionary<Type, EventConsumerActivation[]> GetResults()
    {
        Dictionary<Type, EventConsumerActivation[]> buffer = _hash.ToDictionary(x => x.Key, x => x.Value.ToArray());
        buffer.TrimExcess();
        return buffer;
    }
}