using System;
using System.IO;
using Vera.Documents.Nodes;

namespace Vera.Documents.Visitors
{
    public class TextThermalVisitor : IThermalVisitor
    {
        private readonly TextWriter _tw;
        private readonly int _width;

        public TextThermalVisitor(TextWriter tw, int width = 48)
        {
            _tw = tw;
            _width = width;
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
            Append(node.Value);
        }

        public void Visit(QRCodeThermalNode node)
        {
            Append(node.Data);
        }

        public void Visit(ImageThermalNode node)
        {
            Append(Convert.ToBase64String(node.Data));
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
            Append($"{node.BarcodeType} {node.Value}");
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
            Append(new string('=', _width));
        }

        private void Append(string value)
        {
            if (value == null)
            {
                _tw.Write("NULL");
                _tw.WriteLine();
                
                return;
            }
            
            var left = value.Length;
            
            for (var i = 0; i < value.Length; i += _width)
            {
                var line = value.Substring(i, Math.Min(left, _width));
                
                _tw.Write(line);
                _tw.WriteLine();

                left -= line.Length;
            }
        }
    }
}