using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vera.Documents.Nodes;

namespace Vera.Documents
{
    public class JsonThermalNodeParser
    {
        private readonly JsonReader _reader;

        public JsonThermalNodeParser(JsonReader reader)
        {
            _reader = reader;
        }

        public IThermalNode Parse()
        {
            var s = new JsonSerializer();
            var o = s.Deserialize<JObject>(_reader);

            return ParseNodes(o);
        }

        private IThermalNode ParseNodes(JObject o)
        {
            var nodeType = o.Value<string>("type");

            return nodeType switch
            {
                "document" => ParseDocument(o),
                "scope" => ParseScope(o),
                "text" => ParseText(o),
                "image" => ParseImage(o),
                "qr" => ParseQR(o),
                _ => new UnknownNode(nodeType)
            };
        }

        private IThermalNode ParseDocument(JToken o)
        {
            return new DocumentThermalNode(ParseChildren(o));
        }

        private IThermalNode ParseScope(JToken o)
        {
            return new ScopeThermalNode(ParseChildren(o), o.Value<string>("align"));
        }

        private IThermalNode ParseText(JToken o)
        {
            return new TextThermalNode(o.Value<string>("value"));
        }

        private IThermalNode ParseImage(JToken o)
        {
            return new ImageThermalNode(
                o.Value<string>("mimeType"),
                Convert.FromBase64String(o.Value<string>("value"))
            );
        }

        private IThermalNode ParseQR(JToken o)
        {
            return new QRCodeThermalNode(o.Value<string>("value"));
        }

        private IEnumerable<IThermalNode> ParseChildren(JToken o)
        {
            return o.Value<JArray>("children")
                .Values<JObject>()
                .Select(ParseNodes)
                .ToList();
        }
    }
}