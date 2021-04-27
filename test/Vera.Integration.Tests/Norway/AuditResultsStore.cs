using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vera.Norway;

namespace Vera.Integration.Tests.Norway
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
                var auditInvoices = file.Company.Location
                    .SelectMany(l => l.Cashregister)
                    .SelectMany(c => c.Cashtransaction)
                    .Select(c => new InvoiceResult
                    {
                        InvoiceNumber = c.InvoiceID,
                        NetTotal = c.TransAmntIn,
                        GrossTotal = c.TransAmntEx,
                        InvoiceLinesCount = c.CtLine.Count
                    });
                if (auditInvoices != null)
                {
                    foreach (var invoice in auditInvoices)
                    {
                        _actualResults.Add(invoice.InvoiceNumber, invoice);
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
                var auditProducts = file.Company.Articles;
                if (auditProducts != null)
                {
                    products.AddRange(
                        auditProducts.Select(p => new Models.Product
                        {
                            SystemId = p.ArtID,
                            Description = p.ArtDesc
                        }));
                }
            }

            return products;
        }

        private async Task<IEnumerable<Auditfile>> GetAuditFileAsync(string accountId, string name)
        {
            var serializer = new XmlSerializer(typeof(Auditfile));
            var response = await _httpClient.GetAsync($"download/audit/{accountId}/{name}");
            var result = await response.Content.ReadAsStreamAsync();

            using var zipArchive = new ZipArchive(result);

            var files = new List<Auditfile>();
            foreach (var entry in zipArchive.Entries)
            {
                using var sr = new StreamReader(entry.Open());

                var file = (Auditfile)serializer.Deserialize(sr);
                files.Add(file);
            }

            return files;
        }

        public IEnumerable<InvoiceResult> ExpectedResults => _expectedResults;
    }

    public class InvoiceResult
    {
        public string InvoiceNumber { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal NetTotal { get; set; }
        public int InvoiceLinesCount { get; set; }

        public Models.Invoice Invoice { get; set; }
    }
}
