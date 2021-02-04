using System;

namespace Vera.Stores.Cosmos
{
    /// <summary>
    /// Specialized version of the document class which add a next and previous property
    /// to make this act like a linked list. Used for invoices for example to make sure that
    /// they follow each other in-order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ChainedDocument<T> : Document<T> where T : class
    {
        // Required for deserialization
        public ChainedDocument() { }

        public ChainedDocument(T value, string partitionKey)
        {
            Id = Guid.NewGuid();
            Value = value ?? throw new NullReferenceException(nameof(value));
            PartitionKey = partitionKey ?? throw new NullReferenceException(nameof(partitionKey));
        }

        /// <summary>
        /// Reference to the previous document' id.
        /// </summary>
        public Guid? Previous { get; set; }

        /// <summary>
        /// Reference to the next document' id.
        /// </summary>
        public Guid? Next { get; set; }
    }
}