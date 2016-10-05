#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
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
using System.Diagnostics;
using System.Linq;
using Autofac;
using Humanizer;
using Humanizer.Bytes;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Ioc.Autofac.Modules;
using Pipeline.Logging.NLog;
using Quartz;
using Process = Pipeline.Configuration.Process;

namespace Pipeline.Command {

    [DisallowConcurrentExecution]
    public class RunTimeExecutor : IRunTimeExecute, IJob, IDisposable {

        public void Execute(Process process) {

            var logger = new NLogPipelineLogger(process.Name);

            if (!string.IsNullOrEmpty(process.MaxMemory)) {
                var context = new PipelineContext(logger, process);

                var timer = new Stopwatch();
                timer.Start();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                timer.Stop();
                context.Info($"Collected free memory. Time taken: {timer.Elapsed}.");

                var currentBytes = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64.Bytes();
                var maxMemory = ByteSize.Parse(process.MaxMemory);

                if (maxMemory.CompareTo(currentBytes) < 0) {
                    context.Error($"Process exceeded {maxMemory.Megabytes.ToString("#.0")} Mb. Current memory is {currentBytes.Megabytes.ToString("#.0")} Mb!");
                    Environment.Exit(1);
                } else {
                    context.Info($"The process is using {currentBytes.Megabytes.ToString("#.0")} Mb of it's max {maxMemory.Megabytes.ToString("#.0")} Mb allowed.");
                }
            }

            var builder = new ContainerBuilder();
            builder.RegisterInstance(logger).As<IPipelineLogger>().SingleInstance();
            builder.RegisterCallback(new RootModule(process.Shorthand).Configure);
            builder.RegisterCallback(new ContextModule(process).Configure);

            // providers
            builder.RegisterCallback(new AdoModule(process).Configure);
            builder.RegisterCallback(new LuceneModule(process).Configure);
            builder.RegisterCallback(new SolrModule(process).Configure);
            builder.RegisterCallback(new ElasticModule(process).Configure);
            builder.RegisterCallback(new InternalModule(process).Configure);
            builder.RegisterCallback(new FileModule(process).Configure);
            builder.RegisterCallback(new FolderModule(process).Configure);
            builder.RegisterCallback(new DirectoryModule(process).Configure);
            builder.RegisterCallback(new ExcelModule(process).Configure);
            builder.RegisterCallback(new WebModule(process).Configure);

            builder.RegisterCallback(new MapModule(process).Configure);
            builder.RegisterCallback(new TemplateModule(process).Configure);
            builder.RegisterCallback(new ActionModule(process).Configure);

            builder.RegisterCallback(new EntityPipelineModule(process).Configure);
            builder.RegisterCallback(new ProcessPipelineModule(process).Configure);
            builder.RegisterCallback(new ProcessControlModule(process).Configure);

            using (var scope = builder.Build().BeginLifetimeScope()) {
                try {
                    scope.Resolve<IProcessController>().Execute();
                } catch (Exception ex) {
                    scope.Resolve<IContext>().Error(ex.Message);
                }
            }
        }

        public void Execute(string cfg, string shorthand, Dictionary<string, string> parameters) {

            var builder = new ContainerBuilder();
            builder.RegisterModule(new RootModule(shorthand));
            builder.Register<IPipelineLogger>(c => new NLogPipelineLogger(cfg)).As<IPipelineLogger>().SingleInstance();
            builder.Register<IContext>(c => new PipelineContext(c.Resolve<IPipelineLogger>())).As<IContext>();

            using (var scope = builder.Build().BeginLifetimeScope()) {
                var context = scope.Resolve<IContext>();
                var process = scope.Resolve<Process>(
                    new NamedParameter("cfg", cfg),
                    new NamedParameter("parameters", parameters)
                );
                foreach (var warning in process.Warnings()) {
                    context.Warn(warning);
                }

                if (process.Errors().Any()) {
                    foreach (var error in process.Errors()) {
                        context.Error(error);
                    }
                    context.Error("The configuration errors must be fixed before this job will run.");
                    return;
                }

                if (parameters.ContainsKey("Mode")) {
                    process.Mode = parameters["Mode"];
                }

                // Since we're in a Console app, and honor output format
                if (process.Output().IsInternal()) {
                    process.Output().Provider = "console";
                    if (parameters.ContainsKey("Output")) {
                        process.Output().Format = parameters["Output"];
                    }
                }

                Execute(process);
            }
        }

        public void Execute(IJobExecutionContext context) {
            var cfg = context.MergedJobDataMap.Get("Cfg") as string;
            var shorthand = context.MergedJobDataMap.Get("Shorthand") as string;
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "Mode", context.MergedJobDataMap.Get("Mode") as string },
                { "Output", context.MergedJobDataMap.Get("Output") as string }
            };
            Execute(cfg, shorthand, parameters);
        }

        public void Dispose() {
            // shouldn't be anything to dispose
        }
    }
}