﻿#region license
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
using Autofac;
using Cfg.Net.Ext;
using Pipeline.Configuration;
using Pipeline.Context;
using Pipeline.Contracts;
using Pipeline.Extensions;
using Pipeline.Logging.NLog;
using Quartz.Spi;

namespace Pipeline.Command {

    public class ScheduleModule : Module {
        private readonly Options _options;

        public ScheduleModule(Options options) {
            _options = options;
        }

        protected override void Load(ContainerBuilder builder) {

            var name = Utility.Identifier(_options.Configuration, "-");
            builder.Register((ctx, p) => new NLogPipelineLogger(name, _options.LogLevel)).As<IPipelineLogger>().SingleInstance();
            builder.Register<IContext>(ctx => new PipelineContext(ctx.Resolve<IPipelineLogger>(), new Process { Name = "Scheduler", Key = "Scheduler" }.WithDefaults())).As<IContext>().SingleInstance();
            builder.Register((ctx, p) => new QuartzJobFactory(ctx.Resolve<IContext>())).As<IJobFactory>().SingleInstance();

            builder.Register<IScheduler>((ctx, p) => {
                var context = ctx.Resolve<IContext>();
                if (string.IsNullOrEmpty(_options.CronExpression) || _options.Mode.In("init", "meta")) {
                    return new QuartzNowScheduler(_options, context, ctx.Resolve<IJobFactory>());
                }
                return new QuartzCronScheduler(_options, context, ctx.Resolve<IJobFactory>());
            }).As<IScheduler>().SingleInstance();

        }

    }
}