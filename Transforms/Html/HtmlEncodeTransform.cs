﻿#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2019 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
using System.Collections.Generic;
using System.Web;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms.Html {

    public class HtmlEncodeTransform : StringTransform {
        private readonly Field _input;

        public HtmlEncodeTransform(IContext context = null) : base(context, "string") {
            if (IsMissingContext()) {
                return;
            }
            if (IsNotReceiving("string")) {
                return;
            }
            _input = SingleInput();
        }

        public override IRow Operate(IRow row) {
            row[Context.Field] = HttpUtility.HtmlEncode(GetString(row, _input));
            
            return row;
        }

        public override IEnumerable<OperationSignature> GetSignatures() {
            return new[] {
                new OperationSignature("htmlencode"),
                new OperationSignature("xmlencode")
            };
        }
    }
}
