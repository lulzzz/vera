using Vera.Documents.Nodes;

namespace Vera.Documents
{
    public interface IThermalVisitor
    {
        void Visit(DocumentThermalNode node);
        void Visit(TextThermalNode node);
        void Visit(QRCodeThermalNode node);
        void Visit(ImageThermalNode node);
        void Visit(ScopeThermalNode node);
        void Visit(BarcodeThermalNode node);
    }
}