using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using SkyApm;
using SkyApm.Utilities.DependencyInjection;
using System;
using System.Diagnostics;

namespace SkyApm.Diagnostics.FreeRedis
{

    public static class SkyWalkingBuilderExtensions
    {
        public static readonly DiagnosticListener dl = new DiagnosticListener("FreeRedisDiagnosticListener");

        public static SkyApmExtensions AddFreeRedis(this SkyApmExtensions extensions, RedisClient redisClient, bool includeAuth = false)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }
            extensions.Services.AddSingleton<ITracingDiagnosticProcessor, FreeRedisTracingDiagnosticProcessor>();
            if (redisClient != null)
            {
                ConfigAop(redisClient, includeAuth);
            }
            return extensions;
        }


        public static void ConfigAop(RedisClient redisClient, bool includeAuth = false)
        {
            redisClient.Notice += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Log) && (includeAuth || (!e.Log.Contains("> AUTH "))))
                {
                    dl.Write(FreeRedisTracingDiagnosticProcessor.FreeRedis_Notice, e);
                }
            };
        }
    }
}