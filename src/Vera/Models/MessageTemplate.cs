using System;
using System.Collections.Generic;

namespace Vera.Models
{
    public class MessageTemplate
    {
        public MessageTemplate()
        {
            Id = Guid.NewGuid();
        }
        
        public Guid Id { get; set; }
        
        public string AccountId { get; set; }
        
        public ICollection<string> Footer { get; set; }
        
        public byte[] Logo { get; set; }
    }
}