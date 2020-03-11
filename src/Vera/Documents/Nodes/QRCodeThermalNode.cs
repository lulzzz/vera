namespace Vera.Documents.Nodes
{
    public class QRCodeThermalNode : IThermalNode
    {
        public QRCodeThermalNode(string data)
        {
            Data = data;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "qr";

        public string Data { get; }
    }
}