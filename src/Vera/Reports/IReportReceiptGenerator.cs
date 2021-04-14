using Vera.Documents.Nodes;

namespace Vera.Reports
{
    public interface IReportReceiptGenerator
    {
        IThermalNode Generate(ReceiptReportContext context);
    }
}
