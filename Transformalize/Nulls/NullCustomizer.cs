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
using Cfg.Net.Contracts;

namespace Transformalize.Nulls {
    /// <summary>
    /// The validator that doesn't do anything.
    /// </summary>
    public class NullCustomizer : ICustomizer {
        public void Customize(string parent, INode node, IDictionary<string, string> parameters, ILogger logger){}
        public void Customize(INode root, IDictionary<string, string> parameters, ILogger logger) { }
       
    }
}
