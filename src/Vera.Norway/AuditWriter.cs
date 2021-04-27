using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vera.Audits;
using Vera.Dependencies;

namespace Vera.Norway
{
    public class AuditWriter : IAuditWriter
    {
        private readonly IDateProvider _dateProvider;

        public AuditWriter(IDateProvider dateProvider)
        {
            _dateProvider = dateProvider;
        }

        public Task<string> ResolveName(AuditCriteria criteria, int sequence, int total)
        {
            // Currently only support generating one file at a time

            const string type = "SAF-T Cash Register";
            
            var creationTime = _dateProvider.Now.ToString("yyyyMMddHHmmss");

            // Format as defined in the "Naming of the SAF-T data file"
            return Task.FromResult($"{type}_{criteria.SupplierSystemId}_{creationTime}_{sequence}_{total}.xml");
        }

        public Task Write(AuditContext context, AuditCriteria criteria, Stream stream)
        {
            var creator = new AuditCreator();
            var file = creator.Create(context, criteria);

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

            return Task.CompletedTask;
        }

        public bool WillOutput => true;
    }
}