using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vera.Germany.Fiskaly;
using Vera.Models;
using Vera.Signing;

namespace Vera.Germany.Invoices
{
    public class InvoiceSigner : IInvoiceSigner
    {
        private readonly IFiskalyClient _fiskalyClient;

        public InvoiceSigner(IFiskalyClient fiskalyClient)
        {
            _fiskalyClient = fiskalyClient;
        }

        public Task<Signature> Sign(Invoice invoice, Signature previousSignature)
        {
            var clientId = invoice.RegisterSystemId;

            var tss = _fiskalyClient.GetTss(invoice.Supplier.Id.ToString());
            var client = _fiskalyClient.GetClient(clientId, tss._id);

            var taxes = invoice.Lines
              .Where(il => il.Settlements.Any())
              .GroupBy(il => FiskalyHelper.MapTax(il.Taxes.Category))
              .Select(g => (taxCategory: g.Key, grossTotal: g.Sum(il => il.Gross)))
              .ToList();

            var payments = invoice.Payments
              .GroupBy(p => p.Category == PaymentCategory.Cash)
              .Select(g => (isCash: g.Key, amount: g.Sum(p => p.Amount)))
              .ToList();

            var createTransactionModel = new CreateTransactionModel
            {
                TssId = tss._id,
                ClientId = clientId,
                TxId = Guid.NewGuid().ToString(),
                Taxes = taxes,
                Payments = payments,
                Currency = invoice.CurrencyCode
            };
            var transactionModel = _fiskalyClient.CreateTransaction(createTransactionModel);

            var signatureOutput = EncodingHelper.Encode(transactionModel.Signature.Value);
            var signatureInput = JsonConvert.SerializeObject(createTransactionModel);

            return Task.FromResult(new Signature
            {
                Input = signatureInput,
                Output = signatureOutput
            });
        }
    }
}
