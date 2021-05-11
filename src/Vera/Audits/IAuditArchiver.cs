using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Vera.Models;
using Vera.Stores;

namespace Vera.Audits
{
    public interface IAuditArchiver
    {
        Task Archive(Account account, Audit audit);
    }

    public class AuditArchiver : IAuditArchiver
    {
        private readonly IInvoiceStore _invoiceStore;
        private readonly IBlobStore _blobStore;
        private readonly IAuditStore _auditStore;
        private readonly IAuditWriter _auditWriter;

        public AuditArchiver(
            IInvoiceStore invoiceStore,
            IBlobStore blobStore,
            IAuditStore auditStore,
            IAuditWriter auditWriter)
        {
            _invoiceStore = invoiceStore;
            _blobStore = blobStore;
            _auditStore = auditStore;
            _auditWriter = auditWriter;
        }

        public async Task Archive(Account account, Audit audit)
        {
            if (audit.Start > audit.End)
            {
                throw new InvalidOperationException("audit.Start cannot exceed audit.End");
            }

            // TODO: extract "archiver" interface? to make it more testable and don't depend on filesystem
            await using var streamToZip = File.Create(Path.GetTempFileName());

            using (var archive = new ZipArchive(streamToZip, ZipArchiveMode.Create, true))
            {
                await FillArchive(account, audit, archive);
            }

            // Flush and rewind before the contents are stored
            await streamToZip.FlushAsync();
            streamToZip.Position = 0;

            var location = await _blobStore.Store(account.Id, new Blob
            {
                Content = streamToZip,
                MimeType = "application/zip"
            });

            audit.Location = location;

            await _auditStore.Update(audit);
        }

        private async Task FillArchive(Account account, Audit audit, ZipArchive archive)
        {
            var sequence = 1;
            var ranges = GetDateRanges(audit).ToList();

            var criteria = new AuditCriteria
            {
                AccountId = account.Id,
                SupplierSystemId = audit.SupplierSystemId,
            };

            var context = new AuditContext
            {
                Account = account
            };

            foreach (var (start, end) in ranges)
            {
                criteria.StartDate = start;
                criteria.EndDate = end;

                context.Invoices = await _invoiceStore.List(criteria);

                // TODO(kevin): fetch print trail?

                var entryName = await _auditWriter.ResolveName(criteria, sequence++, ranges.Count);

                await using var stream = archive.CreateEntry(entryName, CompressionLevel.Fastest).Open();
                await _auditWriter.Write(context, criteria, stream);
            }
        }

        /// <summary>
        /// Returns all of the date ranges (start of month till end of month) based on the start/end of the given audit.
        /// </summary>
        /// <param name="audit"></param>
        /// <returns></returns>
        private static IEnumerable<(DateTime start, DateTime end)> GetDateRanges(Audit audit)
        {
            static DateTime Min(DateTime a, DateTime b)
            {
                return a > b ? b : a;
            }

            var offset = audit.Start;
            var lastDayOfMonth = DateTime.DaysInMonth(offset.Year, offset.Month);
            var end = new DateTime(offset.Year, offset.Month, lastDayOfMonth);

            end = Min(end, audit.End);

            while (offset < audit.End)
            {
                yield return (offset, end);

                // Set offset to end and move to the next day after end
                offset = end;
                end = end.AddDays(1);

                // set end to last day of end month
                lastDayOfMonth = DateTime.DaysInMonth(end.Year, end.Month);
                end = new DateTime(end.Year, end.Month, lastDayOfMonth);

                // Ensure end does not pass that of the audit
                end = Min(end, audit.End);
            }
        }
    }
}