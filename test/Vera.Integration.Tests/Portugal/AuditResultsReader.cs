using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vera.Models;
using Vera.Portugal.Models;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditResultsReader
    {
        private readonly HttpClient _httpClient;
        private List<InvoiceResult> _expectedResults;
        private Dictionary<string, InvoiceResult> _actualResults;

        public IEnumerable<InvoiceResult> ExpectedResults => _expectedResults;

        public AuditResultsReader(HttpClient httpClient)
        {
            _expectedResults = new List<InvoiceResult>();
            _actualResults = new Dictionary<string, InvoiceResult>();
            _httpClient = httpClient;
        }

        public void Add(InvoiceResult invoiceResult)
        {
            _expectedResults.Add(invoiceResult);
        }

        public InvoiceResult GetActualResult(string invoiceNumber)
        {
            if (_actualResults.TryGetValue(invoiceNumber, out var invoice))
            {
                return invoice;
            }
            return null;
        }

        public async Task LoadInvoiceResultsAsync(string accountId, string name)
        {
            var auditFile = await GetAuditFileAsync(accountId, name);

            foreach (var auditInvoice in auditFile.SourceDocuments.SalesInvoices.Invoice)
            {
                var invoiceResult = new InvoiceResult
                {
                    InvoiceNumber = auditInvoice.InvoiceNo,
                    ATCUD = auditInvoice.ATCUD,
                    InvoiceType = auditInvoice.InvoiceType,
                    GrossTotal = auditInvoice.DocumentTotals.GrossTotal,
                    NetTotal = auditInvoice.DocumentTotals.NetTotal,
                    InvoiceLinesCount = auditInvoice.Line.Length
                    // TODO determinte PaymentType from audit and assert in test
                };
                _actualResults.Add(auditInvoice.InvoiceNo, invoiceResult);
            }
        }

        public async Task<IEnumerable<Models.Product>> GetProductsAsync(string accountId, string name)
        {
            var auditFile = await GetAuditFileAsync(accountId, name);

            return auditFile.MasterFiles.Product.Select(p => new Models.Product
            {
                Code = p.ProductCode,
                Description = p.ProductDescription
            });
        }

        private async Task<AuditFile> GetAuditFileAsync(string accountId, string name)
        {
            var serializer = new XmlSerializer(typeof(AuditFile));
            var response = await _httpClient.GetAsync($"download/audit/{accountId}/{name}");
            var result = await response.Content.ReadAsStreamAsync();

            using var zipArchive = new ZipArchive(result);
            var entry = zipArchive.Entries.First();
            using var sr = new StreamReader(entry.Open());

            return (AuditFile)serializer.Deserialize(sr);
        }
    }

    public class InvoiceResult
    {
        public string InvoiceNumber { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public string ATCUD { get; set; }
        public string PaymentType { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal NetTotal { get; set; }
        public int InvoiceLinesCount { get; set; }
        public int ProductsCount { get; set; }

        public static string GetPaymentType(PaymentCategory paymentCategory)
        {
            return paymentCategory switch
            {
                PaymentCategory.Cash => "NU",
                PaymentCategory.Debit => "CD",
                PaymentCategory.Credit => "CC",
                PaymentCategory.Voucher => "CO",
                PaymentCategory.Other => throw new NotImplementedException(),
                PaymentCategory.Change => throw new NotImplementedException(),
                PaymentCategory.Online => throw new NotImplementedException(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
