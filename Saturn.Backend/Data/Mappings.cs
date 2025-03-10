﻿using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using Saturn.Backend.Data.Enums;
using Saturn.Backend.Data.Services;
using Saturn.Backend.Data.Utils;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Saturn.Backend.Data.Utils.BenBot;

namespace Saturn.Backend.Data
{
    public class Mappings
    {
        private readonly IBenBotAPIService _benBotAPIService; 
        private readonly DefaultFileProvider _provider; 
        private readonly IFortniteAPIService _fortniteAPIService;
        private readonly IJSRuntime _jsRuntime;
        
        private readonly RestApiHelper _restApiHelper;

        public Mappings(DefaultFileProvider provider, IBenBotAPIService benbotAPIService, IFortniteAPIService fortniteApiService, IJSRuntime jsRuntime)
        {
            _benBotAPIService = benbotAPIService;
            _provider = provider;
            _fortniteAPIService = fortniteApiService;
            _jsRuntime = jsRuntime;

            _restApiHelper = new RestApiHelper();
        }

        public async Task Init()
        {
            Directory.CreateDirectory(Config.MappingsFolder);
            await _restApiHelper.ThreadWorker.Begin(cancellationToken =>
            {
                var mappings = _restApiHelper.BenbotApi.GetMappings(cancellationToken);
                if (mappings is { Length: > 0 })
                {
                    foreach (var mapping in mappings)
                    {
                        if (mapping.Meta.CompressionMethod != "Oodle") continue;

                        var mappingPath = Path.Combine(Config.MappingsFolder, mapping.FileName);
                        if (!File.Exists(mappingPath))
                        {
                            _restApiHelper.BenbotApi.DownloadFile(mapping.Url, mappingPath);
                        }

                        _provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingPath);
                        Logger.Log($"Mappings pulled from '{mapping.FileName}'");
                        break;
                    }
                }
                else
                {
                    Logger.Log("Couldn't get mappings from BenBot API, it's probably down. Falling back to local mappings!", LogLevel.Warning);
                    var latestUsmaps = new DirectoryInfo(Config.MappingsFolder).GetFiles("*_oo.usmap");
                    if (_provider.MappingsContainer != null || latestUsmaps.Length <= 0)
                    {
                        Logger.Log("Local mappings folder doesn't contain mappings! Downloading them from Discord!", LogLevel.Warning);
                        var mappingPath = Path.Combine(Config.MappingsFolder, "++" + FileUtil.SubstringFromLast(Config.MappingsURL, '/'));
                        new WebClient().DownloadFile(Config.MappingsURL, mappingPath);
                        latestUsmaps = new DirectoryInfo(Config.MappingsFolder).GetFiles("*_oo.usmap");
                    }

                    var latestUsmapInfo = latestUsmaps.OrderBy(f => f.LastWriteTime).Last();
                    _provider.MappingsContainer = new FileUsmapTypeMappingsProvider(latestUsmapInfo.FullName);
                    Logger.Log($"Mappings pulled from '{latestUsmapInfo.Name}'", LogLevel.Warning);
                }
            });
        }
    }
}