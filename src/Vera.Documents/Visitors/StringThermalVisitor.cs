using System;
using System.IO;
using System.Text;
using Vera.Documents.Nodes;

namespace Vera.Documents.Visitors
{
    public class StringThermalVisitor : IThermalVisitor
    {
        private readonly TextWriter _tw;

        public StringThermalVisitor(TextWriter tw)
        {
            _tw = tw;
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
                _tw.WriteLine();
            }
        }

        public void Visit(LineThermalNode node)
        {
            Append("LINE", "--------");
        }

        private void Append(string tag, string value)
        {
            _tw.WriteLine($"#{tag} ");
            _tw.Write(value);
            _tw.WriteLine();
        }
    }
}