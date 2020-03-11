namespace Vera.Documents.Nodes
{
    public interface IThermalNode
    {
        void Accept(IThermalVisitor visitor);

        string Type { get; }
    }
}