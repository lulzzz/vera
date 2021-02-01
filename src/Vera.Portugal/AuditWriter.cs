using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vera.Audits;
using Vera.Models;
using Vera.Portugal.Models;

namespace Vera.Portugal
{
    public class AuditWriter : IAuditWriter
    {
        public AuditWriter()
        {
        }

        public Task Write(AuditContext context, AuditCriteria criteria, Stream stream)
        {
            // TODO: pass correct parameters
            var creator = new AuditCreator(string.Empty);
            var model = creator.Create(context, criteria);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                // Windows-1252
                Encoding = Encoding.GetEncoding(1252),
                CloseOutput = false
            };

            using var writer = XmlWriter.Create(stream, settings);
            var serializer = new XmlSerializer(typeof(AuditFile));
            serializer.Serialize(writer, stream);

            return Task.CompletedTask;
        }
    }
}