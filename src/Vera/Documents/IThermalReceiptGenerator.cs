using Vera.Documents.Nodes;
using Vera.Models;

namespace Vera.Documents
{
    public interface IThermalReceiptGenerator
    {
        IThermalNode Generate(Invoice model);
    }
}