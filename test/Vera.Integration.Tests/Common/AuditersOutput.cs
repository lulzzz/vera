using Google.Protobuf;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Vera.Grpc;

namespace Vera.Integration.Tests.Common
{
    public class AuditersOutput
    {
        private const string FolderName = "Auditers_Output";
        private const string AuditFolderName = "Audits";
        private const string ReceiptsFolderName = "Receipts";

        protected SetupClient _setupClient;
        protected HttpClient _httpClient;
        protected string _countryId;

        public AuditersOutput(SetupClient setupClient, HttpClient httpClient, string countryId)
        {
            _setupClient = setupClient;
            _httpClient = httpClient;
            _countryId = countryId;
        }

        public async Task StoreAuditFilesInAuditersOutput(string auditFileLocation, string testName, IEnumerable<string> invoiceNumbers)
        {
            await WriteAuditFileZipEntries(auditFileLocation, testName);

            var receiptFileNo = 0;

            foreach (var invoiceNumber in invoiceNumbers)
            {
                receiptFileNo++;
                await StoreReceiptFileInAuditersOutput(invoiceNumber, receiptFileNo, testName);
            }
        }

        private async Task WriteAuditFileZipEntries(string name, string testName)
        {
            var response = await _httpClient.GetAsync($"download/audit/{name}");
            var result = await response.Content.ReadAsStreamAsync();

            using var zipArchive = new ZipArchive(result);

            var auditFileNo = 0;
            foreach (var entry in zipArchive.Entries)
            {
                auditFileNo++;
                entry.ExtractToFile(string.Format("{0}_{1}.xml", Path.Join(SetUpFilePathInAuditersOutput(AuditFolderName),testName), auditFileNo), true);
            }
        }

        private async Task StoreReceiptFileInAuditersOutput(string invoiceNumber, int invoiceIndex, string testName)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return;

            var renderReceiptReply = await _setupClient.Receipt.RenderThermalAsync(new RenderThermalRequest
            {
                AccountId = _setupClient.AccountId,
                InvoiceNumber = invoiceNumber,
                Type = ReceiptOutputType.Text
            }, _setupClient.AuthorizedMetadata);

            var getInvoiceReply = _setupClient.Invoice.GetByNumber(new GetInvoiceByNumberRequest
            {
                AccountId = _setupClient.AccountId,
                Number = invoiceNumber
            }, _setupClient.AuthorizedMetadata);

            WriteReceiptFileInAuditersOutput(renderReceiptReply.Content, string.Format("{0}_{1}_{2}.txt", testName, invoiceIndex, getInvoiceReply.Remark), getInvoiceReply.Supplier.Name);
        }

        private void WriteReceiptFileInAuditersOutput(ByteString content, string fileName, string storeName)
        {
            File.WriteAllBytesAsync(Path.Join(SetUpFilePathInAuditersOutput(ReceiptsFolderName, storeName), fileName), content.ToByteArray());
        }

        private string SetUpFilePathInAuditersOutput(string fileTypeSubfolder, string storeSubfolder = null)
        {
            var folders = new List<string>
            {
                FolderName,
                _countryId,
                fileTypeSubfolder
            };

            if (storeSubfolder != null)
                folders.Add(storeSubfolder);

            var filePath = "";

            foreach (var folder in folders)
            {
                filePath = Path.Join(filePath, folder);
                Directory.CreateDirectory(filePath);
            }

            return filePath;
        }
    }
}
