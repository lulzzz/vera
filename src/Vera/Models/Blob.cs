using System.IO;

namespace Vera.Models
{
    public class Blob
    {
        public string MimeType { get; set; }
        public Stream Content { get; set; }
    }
}