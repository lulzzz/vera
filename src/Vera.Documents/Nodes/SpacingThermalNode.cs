using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public class SpacingThermalNode : IThermalNode
    {
        public SpacingThermalNode(int value)
        {
            Value = value;
        }

        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "spacing";

        public int Value { get; }
    }
}