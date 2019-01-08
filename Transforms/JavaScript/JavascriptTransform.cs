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
using System.Linq;
using Cfg.Net.Contracts;
using JavaScriptEngineSwitcher.Core;
using Transformalize.Configuration;
using Transformalize.Contracts;

namespace Transformalize.Transforms.JavaScript {

    public class JavascriptTransform : BaseTransform {

        private readonly Field[] _input;
        private readonly Dictionary<int, string> _errors = new Dictionary<int, string>();
        private readonly IJsEngine _engine;

        public JavascriptTransform(IJsEngineFactory factory, IReader reader, IContext context = null) : base(context, "object") {

            if (IsMissingContext()) {
                return;
            }

            if (IsMissing(Context.Operation.Script)) {
                return;
            }

            _engine = factory.CreateEngine();
            _input = Context.Entity.GetFieldMatches(Context.Operation.Script).ToArray();

            // load any global scripts
            foreach (var sc in Context.Process.Scripts.Where(s => s.Global)) {
                ProcessScript(context, reader, Context.Process.Scripts.First(s => s.Name == sc.Name));
            }

            // load any specified scripts
            if (Context.Operation.Scripts.Any()) {
                foreach (var sc in Context.Operation.Scripts) {
                    ProcessScript(context, reader, Context.Process.Scripts.First(s => s.Name == sc.Name));
                }
            }

            Context.Debug(() => $"Script in {Context.Field.Alias} : {Context.Operation.Script.Replace("{", "{{").Replace("}", "}}")}");
        }

        private void ProcessScript(IContext context, IReader reader, Script script) {
            script.Content = ReadScript(context, reader, script);
            _engine.Execute(script.Content);
        }

        /// <summary>
        /// Read the script.  The script could be from the content attribute, 
        /// from a file referenced in the file attribute, or a combination.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reader"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        private static string ReadScript(IContext context, IReader reader, Script script) {
            var content = string.Empty;

            if (script.Content != string.Empty)
                content += script.Content + "\r\n";

            if (script.File != string.Empty) {
                var p = new Dictionary<string, string>();
                var l = new Cfg.Net.Loggers.MemoryLogger();
                var response = reader.Read(script.File, p, l);
                if (l.Errors().Any()) {
                    foreach (var error in l.Errors()) {
                        context.Error(error);
                    }
                    context.Error($"Could not load {script.File}.");
                } else {
                    content += response + "\r\n";
                }

            }

            return content;
        }

        public override IRow Operate(IRow row) {
            foreach (var field in _input) {
                _engine.SetVariableValue(field.Alias, row[field]);
            }
            try {
                var value = Context.Field.Convert(_engine.Evaluate(Context.Operation.Script));
                if (value == null && !_errors.ContainsKey(0)) {
                    Context.Error($"{_engine.Name} transform in {Context.Field.Alias} returns null!");
                    _errors[0] = $"{_engine.Name} transform in {Context.Field.Alias} returns null!";
                } else {
                    row[Context.Field] = value;
                }
            } catch (JsRuntimeException jse) {
                if (!_errors.ContainsKey(jse.LineNumber)) {
                    Context.Error("Script: " + Context.Operation.Script.Replace("{", "{{").Replace("}", "}}"));
                    Context.Error(jse, "Error Message: " + jse.Message);
                    Context.Error("Variables:");
                    foreach (var field in _input) {
                        Context.Error($"{field.Alias}:{row[field]}");
                    }
                    _errors[jse.LineNumber] = jse.Message;
                }
            }


            return row;
        }

        public override void Dispose() {
            if (_engine.SupportsGarbageCollection) {
                try {
                    _engine.CollectGarbage();
                } catch (Exception) {
                    Context.Debug(() => "Error collecting js garbage");
                }
            }
            _engine.Dispose();
        }

        public override IEnumerable<OperationSignature> GetSignatures() {
            yield return new OperationSignature("js") { Parameters = new List<OperationParameter>(1) { new OperationParameter("script") } };
            yield return new OperationSignature("javascript") { Parameters = new List<OperationParameter>(1) { new OperationParameter("script") } };
            yield return new OperationSignature("chakra") { Parameters = new List<OperationParameter>(1) { new OperationParameter("script") } };
        }
    }
}
