using System;
using Newtonsoft.Json;

namespace Vera.Stores.Cosmos
{
    /// <summary>
    /// Generic wrapper for models so Cosmos' partition is abstracted away from the client.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Document<T> where T : class
    {
        // Constructor for deserialization
        public Document() { }

        public Document(Func<T, Guid> id, Func<T, string> partitionKey, T value)
        {
            Id = id(value);
            PartitionKey = partitionKey(value);
            Value = value;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }
        public string PartitionKey { get; set; }
        public T Value { get; set; }
    }
}