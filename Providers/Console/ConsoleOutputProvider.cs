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
using System;
using System.Collections.Generic;
using Transformalize.Context;
using Transformalize.Contracts;

namespace Transformalize.Providers.Console {
    public class ConsoleOutputProvider : IOutputProvider {

        private readonly OutputContext _outputContext;
        private readonly IWrite _writer;

        public ConsoleOutputProvider(OutputContext context, IWrite writer) {
            _writer = writer;
            _outputContext = context;
        }
        public void Delete() {
        }

        public void Dispose() {
        }

        public void End() {
        }

        public int GetMaxTflKey() {
            return 0;
        }

        public object GetMaxVersion() {
            return null;
        }

        public int GetNextTflBatchId() {
            return _outputContext.Entity.Index;
        }

        public void Initialize() {
        }

        public IEnumerable<IRow> Match(IEnumerable<IRow> rows) {
            throw new NotImplementedException();
        }

        public IEnumerable<IRow> ReadKeys() {
            throw new NotImplementedException();
        }

        public void Start() {
        }

        public void Write(IEnumerable<IRow> rows) {
            _writer.Write(rows);
        }
    }
}
