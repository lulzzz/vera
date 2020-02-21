using System;
using System.Threading.Tasks;

namespace Vera
{
    public interface IInvoiceLocker
    {
        Task<IDisposable> Lock(string resource);
    }
}