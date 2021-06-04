using System;
using System.IO;
using System.Text;

namespace Vera.Helpers
{
  public class EncodedStringWriter : StringWriter
  {
    public override Encoding Encoding { get; }

    public static EncodedStringWriter ASCII => new EncodedStringWriter(Encoding.ASCII);
    public static EncodedStringWriter UTF8 => new EncodedStringWriter(Encoding.UTF8);
    public static EncodedStringWriter Windows1252 => new EncodedStringWriter(Encoding.GetEncoding(1252));

    public EncodedStringWriter(Encoding encoding) : base()
    {
      Encoding = encoding;
    }

    public EncodedStringWriter(Encoding encoding, StringBuilder sb) : base(sb)
    {
      Encoding = encoding;
    }

    public EncodedStringWriter(Encoding encoding, IFormatProvider formatProvider) : base(formatProvider)
    {
      Encoding = encoding;
    }

    public EncodedStringWriter(Encoding encoding, StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider)
    {
      Encoding = encoding;
    }
  }
}