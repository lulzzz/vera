using System;
using Newtonsoft.Json;

namespace Vera.Stores.Cosmos
{
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

    public class DocumentWithType<T> : Document<T> where T : class
    {
        // Constructor for deserialization
        public DocumentWithType() { }

        public DocumentWithType(
            Func<T, Guid> id,
            Func<T, string> partitionKey,
            T value,
            string type
        ) : base(id, partitionKey, value)
        {
            Type = type;
        }

        public string Type { get; set; }
    }
}