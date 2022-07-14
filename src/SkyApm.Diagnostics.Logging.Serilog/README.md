# serilog-sinks-skywalking
Write Serilog events to skywalking apm

# Quick Start

1. install [SkyAPM.Agent.AspNetCore](https://github.com/SkyAPM/SkyAPM-dotnet)
2. install [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)
3. install `SkyApm.Diagnostics.Logging.Serilog`
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) =>
            {
                configuration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.SkyApm(services) // add this line
                .WriteTo.Console();
            }) 
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
```

or in .Net6 `MiniApi`

```c#
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
       .MinimumLevel.Debug()
       .Enrich.FromLogContext()
       .WriteTo.SkyApm(services) // add this line
       .WriteTo.Console();
});
```

# Advanced
use Custome Formater

```c#
configuration
    .WriteTo.SkyApm(services, new JsonFormatter())
```
nore Formatter please see Serilog Docs，or inherit `ITextFormatter`
