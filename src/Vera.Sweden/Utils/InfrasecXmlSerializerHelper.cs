using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Vera.Sweden.Utils
{
    public static class InfrasecXmlSerializerHelper
    {
        public static string SerializeXml(object obj)
        {
            var xmlContent = "";
            var xmlWriterSettings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8, Indent = true, OmitXmlDeclaration = true
            };

            var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings);
            var serializer = new XmlSerializer(obj.GetType());

            // do not add any xml specific test, the API hates that, will not parse the request and return
            // a 412 precondition fail auth error since it can't read the organization number and register ID
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            serializer.Serialize(xmlWriter, obj, namespaces);

            xmlContent = stringWriter.ToString();

            return xmlContent;
        }
    }
}
