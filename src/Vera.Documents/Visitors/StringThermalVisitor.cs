using System;
using System.Text;
using Vera.Documents.Nodes;

namespace Vera.Documents.Visitors
{
    public class StringThermalVisitor : IThermalVisitor
    {
        private readonly StringBuilder _builder;

        public StringThermalVisitor(StringBuilder builder)
        {
            _builder = builder;
        }

        public void Visit(DocumentThermalNode node)
        {
            foreach (var c in node.Children)
            {
                c.Accept(this);
            }
        }

        public void Visit(TextThermalNode node)
        {
            Append("TEXT", node.Value);
        }

        public void Visit(QRCodeThermalNode node)
        {
            Append("QR", node.Data);
        }

        public void Visit(ImageThermalNode node)
        {
            Append("IMG", Convert.ToBase64String(node.Data));
        }

        public void Visit(ScopeThermalNode node)
        {
            foreach (var c in node.Children) 
            {
                c.Accept(this);
            }
        }

        public void Visit(BarcodeThermalNode node)
        {
            Append("BARCODE", $"{node.BarcodeType} {node.Value}");
        }

        public void Visit(SpacingThermalNode node)
        {
            for (var i = 0; i < node.Value; i++)
            {
                _builder.AppendLine();
            }
        }

        public void Visit(LineThermalNode node)
        {
            Append("LINE", "--------");
        }

        private void Append(string tag, string value)
        {
            _builder
                .Append($"#{tag} ")
                .Append(value)
                .AppendLine();
        }
    }
}