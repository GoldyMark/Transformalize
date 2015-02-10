﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Orchard.Localization;
using Transformalize.Configuration;
using Transformalize.Extensions;
using Transformalize.Main;
using Transformalize.Orchard.Models;
using Transformalize.Orchard.Services;

namespace Transformalize.Orchard.Controllers {
    public class ApiController : TflController {

        private readonly ITransformalizeService _transformalize;
        private readonly IApiService _apiService;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public Localizer T { get; set; }

        public ApiController(
            ITransformalizeService transformalize,
            IApiService apiService
        ) {
            _stopwatch.Start();
            _transformalize = transformalize;
            _apiService = apiService;
            T = NullLocalizer.Instance;
        }

        [ActionName("Api/Configuration")]
        public ActionResult Configuration(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    DefaultFormat,
                    DefaultFlavor
                );
            }

            var query = GetQuery();

            return new ApiResponse(request, part.Configuration).ContentResult(
                query["format"] ?? DefaultFormat,
                query["flavor"] ?? DefaultFlavor
            );
        }

        [ActionName("Api/MetaData")]
        public ActionResult MetaData(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    DefaultFormat,
                    DefaultFlavor
                );
            }

            request.RequestType = ApiRequestType.MetaData;
            var query = GetQuery();
            var transformalizeRequest = new TransformalizeRequest(part, query, null);

            var tfl = new TflRoot(transformalizeRequest.Configuration, transformalizeRequest.Query);
            var problems = tfl.Problems();
            if (problems.Any()) {
                var bad = new TransformalizeResponse();
                request.Status = 501;
                request.Message = "Configuration Problem" + problems.Count.Plural();
                bad.Log.AddRange(problems.Select(p => string.Format("{0:HH:mm:ss} | Error | . | . | {1}", DateTime.UtcNow, p)));
                return new ApiResponse(request, "<tfl></tfl>", bad).ContentResult(
                    query["format"] ?? DefaultFormat,
                    query["flavor"] ?? DefaultFlavor
                );
            }

            var metaData = new MetaDataWriter(ProcessFactory.CreateSingle(tfl.Processes[0], new Options { Mode = "metadata" })).Write();
            return new ApiResponse(request, metaData).ContentResult(
                query["format"] ?? DefaultFormat,
                query["flavor"] ?? DefaultFlavor
            );
        }


        [ActionName("Api/Execute"), ValidateInput(false)]
        public ActionResult Execute(int id) {

            Response.AddHeader("Access-Control-Allow-Origin", "*");
            ConfigurationPart part;
            ApiRequest request;

            foreach (var rejection in _apiService.Rejections(id, out request, out part)) {
                return rejection.ContentResult(
                    DefaultFormat,
                    DefaultFlavor
                );
            }

            request.RequestType = ApiRequestType.Execute;

            var query = GetQuery();

            _transformalize.InitializeFiles(part, query);

            // ready
            var processes = new TransformalizeResponse();
            var transformalizeRequest = new TransformalizeRequest(part, query, null);

            try {
                processes = _transformalize.Run(transformalizeRequest);
            } catch (Exception ex) {
                request.Status = 500;
                request.Message = ex.Message + " " + WebUtility.HtmlEncode(ex.StackTrace);
            }

            return new ApiResponse(request, transformalizeRequest.Configuration, processes).ContentResult(
                transformalizeRequest.Query["format"],
                transformalizeRequest.Query["flavor"]
            );
        }

    }
}