using System.Collections.Generic;
using System.Threading;
using Transformalize.Extensions;
using Transformalize.Libs.Rhino.Etl;

namespace Transformalize.Operations.Transform {
    public class RightOperation : ShouldRunOperation {
        private readonly int _length;

        public RightOperation(string inKey, string outKey, int length)
            : base(inKey, outKey) {
            _length = length;
            Name = string.Format("RightOperation ({0})", outKey);
        }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows) {
            foreach (var row in rows) {
                if (ShouldRun(row)) {
                    var value = row[InKey].ToString();
                    if (value.Length > _length)
                        row[OutKey] = row[InKey].ToString().Right(_length);
                } else {
                    Interlocked.Increment(ref SkipCount);
                }

                yield return row;
            }
        }
    }
}