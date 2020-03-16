using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Documents
{
    public sealed class ThermalReceiptContext
    {
        public Account Account { get; set; }
    }

    public interface IThermalReceiptGenerator
    {
        IThermalNode Generate(ThermalReceiptContext context, Invoice model);
    }
}