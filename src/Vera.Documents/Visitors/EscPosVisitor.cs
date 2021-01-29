using System;
using System.IO;
using System.Linq;
using System.Text;
using Vera.Documents.Nodes;

namespace Vera.Documents.Visitors
{
    // TODO: copy EscPos printer from EVA, but let it write to a given stream instead of opening a socket
    public class EscPosVisitor : IThermalVisitor
    {
        /// <summary>
        /// Windows-1252 character encoding. (ANSI Latin 1; Western European (Windows).
        /// </summary>
        private const byte CodePageWPC1252 = 0x10;

        /// <summary>
        /// IBM00858. (	OEM Multilingual Latin 1 + Euro symbol).
        /// </summary>
        private const byte CodePagePC858 = 0x13;

        private readonly Stream _stream;
        private readonly Encoding _encoding;

        public EscPosVisitor(Stream stream)
        {
            _stream = stream;
            _encoding = Encoding.GetEncoding("IBM00858");
        }

        public void Visit(DocumentThermalNode node)
        {
            _stream.Write(new byte[]
            {
                // Align left
                0x1b, 0x61, 0x30,

                // Set font to FontA
                0x1B, 0x21, 0x00,

                // Set tab sizes to 0
                0x1b, 0x44, 0x00, 0x00,

                // Set the code page
                0x1b, 0x74, CodePagePC858,

                // Set font size to 0
                0x1d, 0x21, 0x00,

                // Line spacing to 0
                0x1b, 0x32
            });

            // Actual content of the document
            foreach (var n in node.Children)
            {
                n.Accept(this);
            }

            // End of document
            _stream.Write(new byte[]
            {
                0x1b, 0x0c, // end of page
                0x1b, 0x64, 0x00, 0x1d, 0x56, 0x42, 0x04 // cut paper
            });
        }

        public void Visit(TextThermalNode node)
        {
            if (string.IsNullOrEmpty(node.Value)) return;

            // TODO: apply styling
            _stream.Write(_encoding.GetBytes(node.Value));
            _stream.WriteByte(0x0a); // new line
        }

        public void Visit(QRCodeThermalNode node)
        {
            var size = 4; // between 1 - 16

            var chars = node.Data.ToCharArray().Select(c => (byte) c).ToArray();
            var pl = chars.Length + 3;

            var lsb = pl % 256;
            var msb = pl / 256;


            _stream.Write(new byte[]
            {
                0x1d, 0x28, 0x6b, 0x03, 0x0, 0x31, 0x43, (byte) size,
                0x1d, 0x28, 0x6b, (byte) lsb, (byte) msb, 0x31, 0x50, 0x30
            });

            _stream.Write(chars);

            _stream.Write(new byte[]
            {
                0x1d, 0x28, 0x6b, 0x03, 0x0, 0x31, 0x51, 0x30
            });
        }

        public void Visit(ImageThermalNode node)
        {
            // TODO: assume image is in correct format?
        }

        public void Visit(ScopeThermalNode node)
        {
            // TODO: apply scope styling

            foreach (var n in node.Children)
            {
                n.Accept(this);
            }

            // TODO: revert scope styling
        }

        public void Visit(BarcodeThermalNode node)
        {
            var type = node.BarcodeType switch
            {
                BarcodeTypes.Code39 => 0x04,
                _ => throw new ArgumentOutOfRangeException()
            };

            var width = 3;
            var height = 50;

            _stream.Write(new byte[]
            {
                0x1b, 0x64, 0x00, 0x1d, 0x66, 0x21, // 0x21 => FontA
                0x1d, 0x48, (byte) 0x00, // none, above or below text
                0x1d, 0x68, (byte) height,
                0x1d, 0x77, (byte) width,
                0x1d, 0x6b, (byte) type
            });

            // TODO: original code did .ToCharArray()
            _stream.Write(_encoding.GetBytes(node.Value));

            _stream.Write(new byte[] { 0x00, 0x1b, 0x64, 0x00 });
        }

        public void Visit(SpacingThermalNode node)
        {
            _stream.Write(new byte[] { 0x1b, 0x4a, (byte)node.Value });
        }

        public void Visit(LineThermalNode node)
        {
            // TODO: use actual bitmap line instead
            _stream.Write(_encoding.GetBytes("-----"));
            _stream.WriteByte(0x0a); // new line
        }
    }
}