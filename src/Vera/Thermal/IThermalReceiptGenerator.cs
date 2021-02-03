using Vera.Documents.Nodes;

namespace Vera.Thermal
{
    public interface IThermalReceiptGenerator
    {
        IThermalNode Generate(ThermalReceiptContext context);
    }
}