using System.IO;
using System.Text;
using Newtonsoft.Json;
using Vera.Documents;
using Vera.Documents.Nodes;
using Vera.Documents.Parsers;
using Vera.Documents.Visitors;
using Xunit;

namespace Vera.Tests
{
    public class Lala
    {
        [Fact]
        public void Lala1()
        {
            var node = new DocumentThermalNode(new IThermalNode[]
            {
                new TextThermalNode("FFFFFFU"),
                new ImageThermalNode("image/png", new byte[128]),
                new QRCodeThermalNode("hello world"),
                new ScopeThermalNode(new IThermalNode[]
                    {
                        new TextThermalNode("FFFFFFU"),
                        new TextThermalNode("FFFFFFU"),
                        new ScopeThermalNode(new IThermalNode[]
                            {
                                new TextThermalNode("FFFFFFU"),
                                new TextThermalNode("FFFFFFU"),
                                new TextThermalNode("FFFFFFU"),
                            },
                            "left"
                        ),
                        new TextThermalNode("FFFFFFU"),
                    },
                    "center"
                ),
            });

            var sw = new StringWriter();
            using var jw = new JsonTextWriter(sw);

            var visitor1 = new JsonThermalVisitor(jw);
            node.Accept(visitor1);

            var x = sw.ToString();

            var parser = new JsonThermalNodeParser(new JsonTextReader(new StringReader(x)));
            var node1 = parser.Parse();

            var sw1 = new StringWriter();
            using var jw1 = new JsonTextWriter(sw1);

            var visitor2 = new JsonThermalVisitor(jw1);
            node1.Accept(visitor2);

            Assert.Equal(x, sw1.ToString());
        }
    }
}