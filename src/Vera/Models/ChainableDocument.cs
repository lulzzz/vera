using System;
using Newtonsoft.Json;

namespace Vera.Models
{
    public sealed class ChainableDocument<T> where T : class
    {
        // Required for deserialization
        public ChainableDocument() { }

        public ChainableDocument(T value, string partitionKey)
        {
            Id = Guid.NewGuid();
            Value = value ?? throw new NullReferenceException(nameof(value));
            PartitionKey = partitionKey ?? throw new NullReferenceException(nameof(partitionKey));
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        // Reference to the previous link
        public Guid? Previous { get; set; }

        // Reference to the next link
        public Guid? Next { get; set; }
        
        public T Value { get; set; }
        
        public string PartitionKey { get; set; }
    }
}