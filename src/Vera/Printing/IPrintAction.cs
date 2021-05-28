using System;
using System.Threading.Tasks;

namespace Vera.Printing
{
    public interface IPrintAction
    {
        Task<PrintActionResult> Generate();

        Task<PrintActionResult> Process(ReadOnlySpan<byte> payload);
    }
}