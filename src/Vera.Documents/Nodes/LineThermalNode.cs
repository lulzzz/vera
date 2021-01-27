using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public class LineThermalNode : IThermalNode
    {
        public void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public string Type => "line";
    }
}