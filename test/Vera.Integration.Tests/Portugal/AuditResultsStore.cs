using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vera.Grpc;
using Vera.Portugal.Models;
using Invoice = Vera.Models.Invoice;

namespace Vera.Integration.Tests.Portugal
{
    public class AuditResultsStore
    {
        private readonly HttpClient _httpClient;
        private readonly ICollection<InvoiceResult> _expectedResults;
        private readonly Dictionary<string, InvoiceResult> _actualResults;

        public AuditResultsStore(HttpClient httpClient)
        {
            _expectedResults = new List<InvoiceResult>();
            _actualResults = new Dictionary<string, InvoiceResult>();
            _httpClient = httpClient;
        }

        public void AddExpectedEntry(InvoiceResult invoiceResult)
        {
            _expectedResults.Add(invoiceResult);
        }

        public InvoiceResult GetAuditEntry(string invoiceNumber)
        {
            return _actualResults.TryGetValue(invoiceNumber, out var invoice) ? invoice : null;
        }

        public async Task LoadInvoicesFromAuditAsync(string accountId, string name)
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

        public async Task<IEnumerable<Models.Product>> LoadProductsFromAuditAsync(string accountId, string name)
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
        
        public IEnumerable<InvoiceResult> ExpectedResults => _expectedResults;
    }

    public class InvoiceResult
    {
        public string InvoiceNumber { get; set; }
        public InvoiceType InvoiceType { get; set; }
        public string ATCUD { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal NetTotal { get; set; }
        public int InvoiceLinesCount { get; set; }
        public int ProductsCount { get; set; }
        
        public Invoice Invoice { get; set; }
        public CreateInvoiceReply Reply { get; set; }
    }
}
