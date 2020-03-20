using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vera.Audit;
using Vera.Portugal.Models;

namespace Vera.Portugal
{
    public class AuditArchive : IAuditArchive<AuditFile>
    {
        public Task Archive(AuditCriteria criteria, AuditFile result)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.GetEncoding(1252),
                CloseOutput = false
            };

            // TODO(kevin): pass stream as parameter and write to it or perform storing here?
            // using (var writer = XmlWriter.Create(stream, settings))
            // {
            //     var serializer = new XmlSerializer(typeof(AuditFile));
            //     serializer.Serialize(writer, archive);
            // }

            return Task.CompletedTask;
        }

        public Task<ICollection<AuditFile>> Get(AuditCriteria criteria)
        {
            throw new System.NotImplementedException();
        }
    }
}