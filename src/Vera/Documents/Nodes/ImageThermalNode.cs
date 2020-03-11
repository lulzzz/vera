namespace Vera.Documents.Nodes
{
    public class ImageThermalNode : IThermalNode
    {
        public ImageThermalNode(string mimeType, byte[] data)
        {
            MimeType = mimeType;
            Data = data;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "image";

        public string MimeType { get; }

        public byte[] Data { get; }
    }
}