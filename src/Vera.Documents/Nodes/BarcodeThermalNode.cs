using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public enum BarcodeTypes
    {
        Code39 = 1
    }

    public class BarcodeThermalNode : IThermalNode
    {
        public BarcodeThermalNode(BarcodeTypes type, string value)
        {
            BarcodeType = type;
            Value = value;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "barcode";
        public BarcodeTypes BarcodeType { get; }
        public string Value { get; }
    }
}