using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Vera.Audits;
using Vera.Portugal.Models;

namespace Vera.Portugal
{
    public class AuditWriter : IAuditWriter
    {
        private readonly string _productCompanyTaxId;
        private readonly string _certificateName;
        private readonly string _certificateNumber;

        public AuditWriter(string productCompanyTaxId, string certificateName, string certificateNumber)
        {
            _productCompanyTaxId = productCompanyTaxId;
            _certificateName = certificateName;
            _certificateNumber = certificateNumber;
        }

        public Task<string> ResolveName(AuditCriteria criteria, int sequence, int total)
        {
            return Task.FromResult($"{criteria.SupplierSystemId}-{DateTime.UtcNow:yyyyMMdd}-{sequence}_{total}.xml");
        }

        public Task Write(AuditContext context, AuditCriteria criteria, Stream stream)
        {
            var creator = new AuditCreator(_productCompanyTaxId, _certificateName, _certificateNumber);
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
            serializer.Serialize(writer, model);

            return Task.CompletedTask;
        }
    }
}