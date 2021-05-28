using System;
using System.Threading.Tasks;

namespace Vera.Printing
{
    /// <summary>
    /// Default empty implementation of an action.
    /// </summary>
    public sealed class DonePrintAction: IPrintAction
    {
        public Task<PrintActionResult> Generate()
        {
            return Task.FromResult(new PrintActionResult
            {
                NextAction = null,
                Payload = Array.Empty<byte>(),
                Action = ClientAction.Done
            });
        }

        public Task<PrintActionResult> Process(ReadOnlySpan<byte> payload)
        {
            return Task.FromResult(new PrintActionResult
            {
                NextAction = null,
                Payload = Array.Empty<byte>(),
                Action = ClientAction.Done
            });
        }
    }
}