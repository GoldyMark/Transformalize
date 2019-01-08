#region license
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
using Transformalize.Contracts;

namespace Transformalize {
    public class SlaveRow : BaseRow, IRow {

        public SlaveRow(int capacity) : base(capacity) { }

        public object this[IField field] {
            get => Storage[field.Index];
            set => Storage[field.Index] = value;
        }

        public override object GetValue(IField field) {
            return Storage[field.Index];
        }

        public override void SetValue(IField field, object value) {
            Storage[field.Index] = value;
        }
    }
}