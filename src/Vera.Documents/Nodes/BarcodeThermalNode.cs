namespace Vera.Documents.Nodes
{
    public class BarcodeThermalNode : IThermalNode
    {
        public BarcodeThermalNode(string barcodeType, string value)
        {
            BarcodeType = barcodeType;
            Value = value;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "barcode";
        public string BarcodeType { get; }
        public string Value { get; }
    }
}