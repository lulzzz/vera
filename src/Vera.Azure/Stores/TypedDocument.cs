using System;

namespace Vera.Azure.Stores
{
    public class TypedDocument<T> : Document<T> where T : class
    {
        // Constructor for deserialization
        public TypedDocument() { }

        public TypedDocument(
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