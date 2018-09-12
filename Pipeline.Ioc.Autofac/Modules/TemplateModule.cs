#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
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
using System.Linq;
using Autofac;
using Cfg.Net.Contracts;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Nulls;
using Transformalize.Transforms.Velocity;

namespace Transformalize.Ioc.Autofac.Modules {
    public class TemplateModule : Module {
        private readonly Process _process;

        public TemplateModule() { }

        public TemplateModule(Process process) {
            _process = process;
        }

        protected override void Load(ContainerBuilder builder) {

            if (_process == null)
                return;

            // razor moved to plugin, velocity too soon...
            foreach (var t in _process.Templates.Where(t => t.Enabled && t.Engine != "razor")) {
                var template = t;
                builder.Register<ITemplateEngine>(ctx => {
                    var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), _process);
                    context.Debug(() => $"Registering {template.Engine} Engine for {t.Key}");
                    switch (template.Engine) {
                        case "velocity":
                            return new VelocityTemplateEngine(context, template, ctx.Resolve<IReader>());
                        default:
                            return new NullTemplateEngine();
                    }
                }).Named<ITemplateEngine>(t.Key);
            }

        }
    }
}