using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public sealed class UnknownNode : IThermalNode
    {
        public UnknownNode(string type)
        {
            Type = type;
        }

        public void Accept(IThermalVisitor visitor)
        {
        }

        public string Type { get; }
    }
}