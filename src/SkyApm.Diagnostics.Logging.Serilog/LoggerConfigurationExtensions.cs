using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using SkyApm.Diagnostics.Logging.Serilog.Sinks;
using SkyApm.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SkyApm.Diagnostics.Logging.Serilog
{
    public static class LoggerConfigurationExtensions
    {
        static LoggerConfigurationExtensions() { }

        public static LoggerConfiguration Skywalking(
            this LoggerSinkConfiguration loggerConfiguration, IServiceProvider serviceCollection, ITextFormatter formatter = null)
        {
            return loggerConfiguration
                .Sink(new SkywalkingSink(serviceCollection, formatter));
        }
    }
}
