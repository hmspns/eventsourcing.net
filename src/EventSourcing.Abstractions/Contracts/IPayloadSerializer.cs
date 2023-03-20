namespace EventSourcing.Abstractions
{
    /// <summary>
    /// Contract for payload serialization.
    /// </summary>
    public interface IPayloadSerializer
    {
        /// <summary>
        /// Serialize data.
        /// </summary>
        /// <param name="obj">Data to be serialized.</param>
        /// <param name="type">Type of serialized data.</param>
        /// <returns>Serialized data.</returns>
        byte[] Serialize(object obj, out string type);

        /// <summary>
        /// Deserialize data.
        /// </summary>
        /// <param name="data">Binary data.</param>
        /// <param name="type">Type of serialized data.</param>
        /// <returns>Deserialized object.</returns>
        object Deserialize(byte[] data, string type);
    }
}