using Vera.Documents.Visitors;

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

        public FontSize FontSize { get; set; }
        public bool Bold { get; set; }
    }

    public enum FontSize
    {
        Small = 1,
        Medium = 2,
        Large = 3
    }
}