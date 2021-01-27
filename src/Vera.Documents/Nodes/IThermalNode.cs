using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public interface IThermalNode
    {
        void Accept(IThermalVisitor visitor);

        string Type { get; }
    }
}