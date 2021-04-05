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
            var auditFiles = await GetAuditFileAsync(accountId, name);

            foreach (var file in auditFiles)
            {
                var auditInvoices = file.SourceDocuments.SalesInvoices.Invoice;
                if (auditInvoices != null)
                {
                    foreach (var invoice in auditInvoices)
                    {
                        var result = new InvoiceResult
                        {
                            InvoiceNumber = invoice.InvoiceNo,
                            ATCUD = invoice.ATCUD,
                            InvoiceType = invoice.InvoiceType,
                            GrossTotal = invoice.DocumentTotals.GrossTotal,
                            NetTotal = invoice.DocumentTotals.NetTotal,
                            InvoiceLinesCount = invoice.Line.Length
                            // TODO determinte PaymentType from audit and assert in test
                        };

                        _actualResults.Add(result.InvoiceNumber, result);
                    }
                }
            }
        }

        public async Task<IEnumerable<Models.Product>> LoadProductsFromAuditAsync(string accountId, string name)
        {
            var products = new List<Models.Product>();
            var auditFiles = await GetAuditFileAsync(accountId, name);

            foreach (var file in auditFiles)
            {
                var auditProducts = file.MasterFiles.Product;
                if (auditProducts != null)
                {
                    products.AddRange(
                        auditProducts.Select(p => new Models.Product
                        {
                            Code = p.ProductCode,
                            Description = p.ProductDescription
                        }));
                }
            }

            return products;
        }

        private async Task<IEnumerable<AuditFile>> GetAuditFileAsync(string accountId, string name)
        {
            var serializer = new XmlSerializer(typeof(AuditFile));
            var response = await _httpClient.GetAsync($"download/audit/{accountId}/{name}");
            var result = await response.Content.ReadAsStreamAsync();

            using var zipArchive = new ZipArchive(result);

            var files = new List<AuditFile>();
            foreach (var entry in zipArchive.Entries)
            {
                using var sr = new StreamReader(entry.Open());

                var file = (AuditFile)serializer.Deserialize(sr);
                files.Add(file);
            }

            return files;
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
