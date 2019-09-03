using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.Grpc.Client;
using SkyApm.Diagnostics.Grpc.Server;
using SkyApm.Utilities.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.Grpc
{
    public static class SkyWalkingBuilderExtensions
    {
        public static SkyApmExtensions AddGrpc(this SkyApmExtensions extensions)
        {
            extensions.Services.AddSingleton<ClientDiagnosticProcessor>();
            extensions.Services.AddSingleton<ClientDiagnosticInterceptor>();
            extensions.Services.AddSingleton<ServerDiagnosticProcessor>();
            extensions.Services.AddSingleton<ServerDiagnosticInterceptor>();
            return extensions;
        }
    }
}
