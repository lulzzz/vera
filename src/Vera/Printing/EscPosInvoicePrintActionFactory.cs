using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vera.Documents.Visitors;
using Vera.Models;
using Vera.Stores;
using Vera.Thermal;

namespace Vera.Printing
{
    public class EscPosInvoicePrintActionFactory : IThermalInvoicePrintActionFactory
    {
        private readonly ILogger<EscPosInvoicePrintActionFactory> _logger;
        private readonly IThermalReceiptGenerator _thermalReceiptGenerator;
        private readonly IPrintAuditTrailStore _printAuditTrailStore;

        public EscPosInvoicePrintActionFactory(ILogger<EscPosInvoicePrintActionFactory> logger,
        IThermalReceiptGenerator thermalReceiptGenerator, IPrintAuditTrailStore printAuditTrailStore)
        {
            _logger = logger;
            _thermalReceiptGenerator = thermalReceiptGenerator;
            _printAuditTrailStore = printAuditTrailStore;
        }

        public async Task<IPrintAction> Create(Account account, Invoice invoice)
        {
            await using var ms = new MemoryStream(8192);
            await using var sw = new StreamWriter(ms, Encoding.UTF8);

            var receiptContextFactory = new ThermalReceiptContextFactory(_printAuditTrailStore);

            var context = await receiptContextFactory.Create(account, invoice);

            var node = _thermalReceiptGenerator.Generate(context);

            var visitor = new EscPosVisitor(ms);
            node.Accept(visitor);
            await ms.FlushAsync();
            ms.Position = 0;
            var payload = ms.ToArray();

            _logger.Log(LogLevel.Debug, "It works");

            return new EscPosPrintAction(payload);
        }
    }

    public class EscPosPrintAction : IPrintAction
    {
        private readonly byte[] _payload;

        public EscPosPrintAction(byte[] payload)
        {
            _payload = payload;
        }

        public Task<PrintActionResult> Generate()
        {
            var actionResult = new PrintActionResult
            {
                Action = ClientAction.Write,
                Payload = _payload,
                NextAction = new DonePrintAction()
            };
            return Task.FromResult(actionResult);
        }

        public Task<PrintActionResult> Process(ReadOnlySpan<byte> payload)
        {
            var actionResult = new PrintActionResult
            {
                Action = ClientAction.Done,
                Payload = Array.Empty<byte>(),
                NextAction = new DonePrintAction()
            };
            return Task.FromResult(actionResult);
        }
    }
}
