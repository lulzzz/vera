using System.Collections.Generic;

namespace Vera.Documents.Nodes
{
    public abstract class CompoundThermalNode : IThermalNode
    {
        protected CompoundThermalNode(IEnumerable<IThermalNode> children)
        {
            Children = children;
        }

        public abstract void Accept(IThermalVisitor visitor);

        public abstract string Type { get; }

        public IEnumerable<IThermalNode> Children { get; }
    }
}