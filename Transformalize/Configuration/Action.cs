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
using System.Collections.Generic;
using System.Linq;
using Cfg.Net;
using Transformalize.Contracts;

namespace Transformalize.Configuration {

    /// <summary>
    /// An action is performed <see cref="Before"/> or <see cref="After"/> a pipeline is run.
    /// 
    /// Current actions are: `copy`, `web`, `tfl`, `run`, and `open`.
    /// 
    /// ---
    /// 
    /// __Copy__: Copies a file <see cref="From"/> somewhere, <see cref="To"/> somewhere.
    /// 
    /// [!code-html[copy](../../api/actions.xml?range=3-3 "copy")]
    /// 
    /// ---
    /// 
    /// __Web__: Executes a GET or POST (as determined by <see cref="Method"/>) to a specified <see cref="Url"/>.  
    /// 
    /// * If *get*, the web request result is stored in <see cref="Body"/>, and is forwarded to the next action's <see cref="Body"/>. 
    /// * If *post*, <see cref="Body"/> is posted to the <see cref="Url"/>.
    /// 
    /// [!code-html[web](../../api/actions.xml?range=4-5 "web")]
    /// 
    /// ---
    ///
    /// __Tfl__: Executes another pipeline as determined by either <see cref="Url"/>, <see cref="File"/>, or <see cref="Body"/>. 
    /// 
    /// [!code-html[tfl](../../api/actions.xml?range=6-7 "tfl")]
    /// 
    /// ---
    /// 
    /// __Run__: Runs a <see cref="Command"/> against a <see cref="Connection"/>.
    /// 
    /// [!code-html[run](../../api/actions.xml?range=8-10 "run")]
    /// 
    /// ---
    /// 
    /// __Open__: Opens a <see cref="File"/>.
    /// TODO: Add support to open Url when in web context and this will perform a redirect (after) the action completes
    /// 
    /// [!code-html[open](../../api/actions.xml?range=11-11 "open")]
    /// 
    /// </summary>
    public class Action : CfgNode {

        /// <summary>
        /// Indicates what type of action to perform.
        /// </summary>
        [Cfg(required = true, toLower = true, domain = "copy,web,tfl,run,open,move,replace,log,print,wait,sleep,internal,exit,archive,form-commands,humanize-labels,cs-script", ignoreCase = true)]
        public string Type { get; set; }

        /// <summary>
        /// Set this to `true` to run the action *after* the pipeline runs.
        /// </summary>
        [Cfg(value = true)]
        public bool After { get; set; }

        [Cfg(value = Constants.DefaultSetting, domain = "exception,abort,continue," + Constants.DefaultSetting, ignoreCase = true, toLower = true)]
        public string ErrorMode { get; set; }

        public ErrorMode ToErrorMode() {
            switch (ErrorMode) {
                case "abort":
                    return Transformalize.ErrorMode.Abort;
                case "continue":
                    return Transformalize.ErrorMode.Continue;
                case "exception":
                    return Transformalize.ErrorMode.Exception;
                default:
                    return Transformalize.ErrorMode.Default;
            }
        }

        [Cfg(value = "")]
        public string Arguments { get; set; }

        [Cfg(value = "")]
        public string Bcc { get; set; }

        /// <summary>
        /// Set this to `true` to run the action *before* the pipeline runs.
        /// </summary>
        [Cfg(value = false)]
        public bool Before { get; set; }

        [Cfg(value = "")]
        public string Cc { get; set; }

        [Cfg(value = "")]
        public string Command { get; set; }

        [Cfg(value = "", toLower = true)]
        public string Connection { get; set; }

        [Cfg(value = "")]
        public string File { get; set; }

        [Cfg(value = "")]
        public string From { get; set; }

        [Cfg(value = true)]
        public bool Html { get; set; }

        [Cfg(value = "get", domain = "get,post", toLower = true, ignoreCase = true)]
        public string Method { get; set; }

        [Cfg(value = "*", toLower = true)]
        public string Mode { get; set; }

        [Cfg(value = "")]
        public string NewValue { get; set; }

        [Cfg(value = "")]
        public string OldValue { get; set; }

        [Cfg(value = "")]
        public string Subject { get; set; }

        [Cfg(value = 0)]
        public int TimeOut { get; set; }

        [Cfg(value = "")]
        public string To { get; set; }

        [Cfg(value = "")]
        public string Url { get; set; }

        [Cfg]
        public int Id { get; set; }

        public bool InTemplate { get; set; }

        [Cfg(value = "")]
        public string Body { get; set; }

        [Cfg(value="@()")]
        public string PlaceHolderStyle { get; set; }

        [Cfg]
        public List<NameReference> Modes { get; set; }

        public string[] GetModes() {
            return Modes.Any() ? Modes.Select(m => m.Name).ToArray() : new[] { Mode };
        }

        protected override void Validate() {
            if ((Type == "open" || Type == "tfl") && File == string.Empty && Url == string.Empty && Id == 0) {
                Error($"The {Type} action requires a file, url, or id.");
            }
            if ((Type == "run" || Type == "form-commands") && string.IsNullOrEmpty(Connection)) {
                Error($"A {Type} action requires a connection.");
            }
        }

        protected override void PreValidate() {
            if (Type != null && Type == "copy" && File != string.Empty && To == string.Empty) {
                To = File;
            }
        }

        /// <summary>
        /// Set for dependency injection
        /// </summary>
        public string Key { get; set; }

        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        [Cfg(value = "info", domain = "info,information,informational,error,warn,warning,debug", toLower = true, trim = true)]
        public string Level { get; set; }

        [Cfg(value = "")]
        public string Message { get; set; }

        [Cfg(value = -1)]
        public int RowCount { get; set; }

        [Cfg(value = "")]
        public string Description { get; set; }
    }
}