using System.Threading.Tasks;
using Vera.Models;

namespace Vera
{
    public interface IInvoiceSequenceGenerator
    {
        string Generate(Invoice invoice);
    }
}