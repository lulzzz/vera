using System.Collections.Generic;
using Vera.Documents.Visitors;

namespace Vera.Documents.Nodes
{
    public class DocumentThermalNode : CompoundThermalNode
    {
        public DocumentThermalNode(IEnumerable<IThermalNode> children) : base(children)
        {
        }

        public override void Accept(IThermalVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string Type => "document";
    }
}