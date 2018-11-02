﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stratis.Bitcoin.Builder;
using Stratis.Bitcoin.Builder.Feature;

namespace Stratis.Bitcoin.Features.SignalR
{
    public sealed class SignalRFeature : FullNodeFeature
    {
        public ISignalRService SignalRService { get; }

        private readonly ILogger logger;

        public SignalRFeature(ISignalRService signalRService, ILoggerFactory loggerFactory)
        {
            this.SignalRService = signalRService;
            this.logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <inheritdoc />
        public override Task InitializeAsync()
        {
            this.logger.LogInformation("SignalR hub starting at URL '{0}'.", this.SignalRService.HubRoute);
            return this.SignalRService.StartAsync();
        }

        public override void Dispose()
        {
            this.SignalRService.Dispose();
        }
    }

    public static class SignalRFeatureExtension
    {
        public static IFullNodeBuilder UseSignalR(this IFullNodeBuilder fullNodeBuilder)
        {
            fullNodeBuilder.ConfigureFeature(features =>
            {
                features
                    .AddFeature<SignalRFeature>()
                    .FeatureServices(services =>
                        {
                            services.AddSingleton(fullNodeBuilder);
                            services.AddSingleton<ISignalRService, SignalRService>();
                        });
            });

            return fullNodeBuilder;
        }
    }
}
