namespace Foundation.Redis.Serialization
{
    /// <summary>
    /// Contract for Serializer implementation
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serializes the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns>Return the serialized payload</returns>
        byte[] Serialize<T>(T payload);

        /// <summary>
        /// Deserializes the specified bytes.
        /// </summary>
        /// <typeparam name="T">The type of the expected object.</typeparam>
        /// <param name="serializedPayload">The serialized payload.</param>
        /// <returns>
        /// The instance of the specified Payload
        /// </returns>
        T Deserialize<T>(byte[] serializedPayload);
    }
}
