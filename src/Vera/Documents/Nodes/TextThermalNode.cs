namespace Vera.Documents.Nodes
{
    public class TextThermalNode : IThermalNode
    {
        public TextThermalNode(string value)
        {
            Value = value;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "text";

        public string Value { get; }
    }
}