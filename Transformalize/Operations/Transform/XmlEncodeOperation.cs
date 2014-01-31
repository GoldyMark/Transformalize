using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Transformalize.Libs.Rhino.Etl;

namespace Transformalize.Operations.Transform
{
    public class XmlEncodeOperation : TflOperation {
        public XmlEncodeOperation(string inKey, string outKey) : base(inKey, outKey) { }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows) {
            foreach (var row in rows) {
                if (ShouldRun(row)) {
                    row[OutKey] = new XText(SanitizeXmlString(row[InKey].ToString())).ToString();
                }
                yield return row;
            }
        }

        public string SanitizeXmlString(string xml) {
            var buffer = new StringBuilder(xml.Length);
            foreach (var c in xml.Where(c => IsLegalXmlChar(c))) {
                buffer.Append(c);
            }
            return buffer.ToString();
        }

        /// <summary>
        /// Whether a given character is allowed by XML 1.0.
        /// </summary>
        public bool IsLegalXmlChar(int character) {
            return (
                character == 0x9 /* == '\t' == 9   */          ||
                character == 0xA /* == '\n' == 10  */          ||
                character == 0xD /* == '\r' == 13  */          ||
                (character >= 0x20 && character <= 0xD7FF) ||
                (character >= 0xE000 && character <= 0xFFFD) ||
                (character >= 0x10000 && character <= 0x10FFFF)
                );
        }
    }
}