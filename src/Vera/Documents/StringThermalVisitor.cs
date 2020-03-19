using System;
using System.Text;
using Vera.Documents.Nodes;

namespace Vera.Documents
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
            _builder.Append(node.Value).AppendLine();
        }

        public void Visit(QRCodeThermalNode node)
        {
            _builder
                .Append(">QR ")
                .Append(node.Data)
                .AppendLine();
        }

        public void Visit(ImageThermalNode node)
        {
            _builder
                .Append("> IMG ")
                .Append(Convert.ToBase64String(node.Data))
                .AppendLine();
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
            _builder
                .Append(">BARCODE ")
                .Append(node.BarcodeType)
                .Append(" ")
                .Append(node.Value)
                .AppendLine();
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
            _builder.Append("--------").AppendLine();
        }
    }
}