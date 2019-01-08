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
using Transformalize.Contracts;

namespace Transformalize.Impl {

    public class DefaultRowReader : IRead {

        private readonly IRowFactory _rowFactory;
        private readonly IContext _context;

        public DefaultRowReader(IContext context, IRowFactory rowFactory) {
            _context = context;
            _rowFactory = rowFactory;
        }
        public IEnumerable<IRow> Read() {
            var types = Constants.TypeDefaults();
            var row = _rowFactory.Create();
            foreach (var field in _context.Entity.GetAllFields()) {
                row[field] = field.Convert(field.Default == Constants.DefaultSetting ? types[field.Type] : field.Convert(field.Default));
            }
            yield return row;
        }
    }
}
