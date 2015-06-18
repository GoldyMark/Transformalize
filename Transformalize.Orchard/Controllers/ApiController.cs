﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Logging;
using Transformalize.Extensions;
using Transformalize.Main;
using Transformalize.Orchard.Models;
using Transformalize.Orchard.Services;

namespace Transformalize.Orchard.Controllers {
    public class ApiController : Controller {

        private static readonly string OrchardVersion = typeof(ContentItem).Assembly.GetName().Version.ToString();
        private readonly ITransformalizeService _transformalize;
        private readonly IApiService _apiService;
        private readonly IJobsQueueService _jobQueueService;
        private readonly string _moduleVersion;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private static int _jobPriority = 100000;

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public ApiController(
            ITransformalizeService transformalize,
            IApiService apiService,
            IExtensionManager extensionManager,
            IJobsQueueService jobQueueService
        ) {
            _stopwatch.Start();
            _transformalize = transformalize;
            _apiService = apiService;
            _jobQueueService = jobQueueService;
            _moduleVersion = extensionManager.GetExtension("Transformalize.Orchard").Version;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        [ActionName("Api/Configuration")]
        public ActionResult Configuration(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            var query = _transformalize.GetQuery();

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    query["format"],
                    query["flavor"]
                );
            }

            return new ApiResponse(request, part.Configuration).ContentResult(
                query["format"],
                query["flavor"]
            );
        }

        [ActionName("Api/MetaData")]
        public ActionResult MetaData(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            var query = _transformalize.GetQuery();

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    query["format"],
                    query["flavor"]
                );
            }

            request.RequestType = ApiRequestType.MetaData;
            var transformalizeRequest = new TransformalizeRequest(part, query, null, Logger);
            var logger = new TransformalizeLogger(transformalizeRequest.Part.Title(), part.LogLevel, Logger, OrchardVersion, _moduleVersion);

            var errors = transformalizeRequest.Root.Errors();
            if (errors.Any()) {
                var bad = new TransformalizeResponse();
                request.Status = 501;
                request.Message = "Configuration Problem" + errors.Length.Plural();
                bad.Log.AddRange(errors.Select(p => new[] { DateTime.Now.ToString(CultureInfo.InvariantCulture), "error", ".", ".", p }));
                return new ApiResponse(request, "<tfl></tfl>", bad).ContentResult(
                    query["format"],
                    query["flavor"]
                );
            }

            var metaData = new MetaDataWriter(ProcessFactory.CreateSingle(transformalizeRequest.Root.Processes[0], logger, new Options { Mode = "metadata" })).Write();
            return new ApiResponse(request, metaData).ContentResult(
                query["format"],
                query["flavor"]
            );
        }

        [ActionName("Api/Enqueue"), ValidateInput(false)]
        public ActionResult Enqueue(int id) {
            ConfigurationPart part;
            ApiRequest request;

            var query = _transformalize.GetQuery();

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    query["format"],
                    query["flavor"]
                );
            }

            request.RequestType = ApiRequestType.Enqueue;

            var args = string.Concat(part.Id, ",", query["mode"]);
            var parameters = new Dictionary<string, object> { { "args", args } };
            _jobQueueService.Enqueue("ITransformalizeJobService.Run", parameters, _jobPriority--);

            return new ApiResponse(request, part.Configuration).ContentResult(
                query["format"],
                query["flavor"]
            );
        }

        [ActionName("Api/Execute"), ValidateInput(false)]
        public ActionResult Execute(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            var query = _transformalize.GetQuery();

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    query["format"],
                    query["flavor"]
                );
            }

            request.RequestType = ApiRequestType.Execute;
            _transformalize.InitializeFiles(part, query);

            // ready
            var processes = new TransformalizeResponse();
            var transformalizeRequest = new TransformalizeRequest(part, query, null, Logger);

            try {
                processes = _transformalize.Run(transformalizeRequest);
            } catch (Exception ex) {
                request.Status = 500;
                request.Message = ex.Message + " " + WebUtility.HtmlEncode(ex.StackTrace);
            }

            return new ApiResponse(request, transformalizeRequest.Configuration, processes).ContentResult(
                transformalizeRequest.Query["format"].ToString(),
                transformalizeRequest.Query["flavor"].ToString()
            );
        }

    }
}