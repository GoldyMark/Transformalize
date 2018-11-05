using System;
using System.Collections.Generic;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms {



    public class SplitTransform : StringTransform {

        private readonly Field _input;
        private readonly char[] _separator;

        public SplitTransform(IContext context = null) : base(context, "object") {
            if (IsMissingContext()) {
                return;
            }

            if (IsMissing(Context.Operation.Separator)) {
                return;
            }

            _input = SingleInput();
            _separator = Context.Operation.Separator.ToCharArray();

        }

        public override IRow Operate(IRow row) {
            row[Context.Field] = GetString(row, _input).Split(_separator, StringSplitOptions.None);
            
            return row;
        }

        public override IEnumerable<OperationSignature> GetSignatures() {
            yield return new OperationSignature("split") {
                Parameters = new List<OperationParameter>(1) { new OperationParameter("separator", ",") }
            };
        }
    }
}