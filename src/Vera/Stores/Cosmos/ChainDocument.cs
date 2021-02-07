using System;
using Newtonsoft.Json;
using Vera.Models;

namespace Vera.Stores.Cosmos
{
    // TODO(kevin): inherit from Document<T>?
    public class ChainDocument
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        public Guid? Previous { get; set; }
        public Guid? Next { get; set; }
        public int Sequence { get; set; }
        public Signature Signature { get; set; }
        public string PartitionKey { get; set; }
    }
}