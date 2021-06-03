using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vera.Audits;
using Vera.Dependencies;
using Vera.Stores;

namespace Vera.Norway.Audit
{
    public class AuditWriter : IAuditWriter
    {
        private readonly IDateProvider _dateProvider;
        private readonly IReportStore _reportStore;

        public AuditWriter(IDateProvider dateProvider, IReportStore reportStore)
        {
            _dateProvider = dateProvider;
            _reportStore = reportStore;
        }

        public Task<string> ResolveName(string supplierSystemId, int sequence, int total)
        {
            // Currently only support generating one file at a time

            const string type = "SAF-T Cash Register";
            
            var creationTime = _dateProvider.Now.ToString("yyyyMMddHHmmss");

            // Format as defined in the "Naming of the SAF-T data file"
            return Task.FromResult($"{type}_{supplierSystemId}_{creationTime}_{sequence}_{total}.xml");
        }

        public async Task Write(AuditContext context, AuditCriteria criteria, Stream stream)
        {
            var creator = new AuditCreator(_reportStore);
            var file = await creator.CreateAsync(context, criteria);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                CloseOutput = false
            };

            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(Auditfile));
                serializer.Serialize(writer, file);
            }
        }

        public bool WillOutput => true;
    }
}