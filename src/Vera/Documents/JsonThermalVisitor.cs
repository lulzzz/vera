using System;
using Newtonsoft.Json;
using Vera.Documents.Nodes;

namespace Vera.Documents
{
    public class JsonThermalVisitor : IThermalVisitor
    {
        private readonly JsonWriter _writer;

        public JsonThermalVisitor(JsonWriter writer)
        {
            _writer = writer;
        }

        public void Visit(DocumentThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("children");
            _writer.WriteStartArray();

            foreach (var n in node.Children)
            {
                n.Accept(this);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();
        }

        public void Visit(TextThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("value");
            _writer.WriteValue(node.Value);

            _writer.WriteEndObject();
        }

        public void Visit(QRCodeThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("value");
            _writer.WriteValue(node.Data);

            _writer.WriteEndObject();
        }

        public void Visit(ImageThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("mimeType");
            _writer.WriteValue(node.MimeType);

            _writer.WritePropertyName("value");
            _writer.WriteValue(Convert.ToBase64String(node.Data));

            _writer.WriteEndObject();
        }

        public void Visit(ScopeThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("align");
            _writer.WriteValue(node.Align);

            _writer.WritePropertyName("children");
            _writer.WriteStartArray();

            foreach (var n in node.Children)
            {
                n.Accept(this);
            }

            _writer.WriteEndArray();

            _writer.WriteEndObject();
        }

        public void Visit(BarcodeThermalNode node)
        {
            _writer.WriteStartObject();

            WriteNodeType(node);

            _writer.WritePropertyName("barcodeType");
            _writer.WriteValue(node.Type);

            _writer.WritePropertyName("value");
            _writer.WriteValue(node.Value);

            _writer.WriteEndObject();
        }

        private void WriteNodeType(IThermalNode node)
        {
            _writer.WritePropertyName("type");
            _writer.WriteValue(node.Type);
        }
    }
}