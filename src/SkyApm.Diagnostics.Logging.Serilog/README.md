# serilog-sinks-skywalking
[![Nuget](https://img.shields.io/nuget/v/SkyApm.Diagnostics.Logging.Serilog)](https://www.nuget.org/packages/SkyApm.Diagnostics.Logging.Serilog/)

Write Serilog events to skywalking apm

# 使用

1. 集成[SkyAPM.Agent.AspNetCore](https://github.com/SkyAPM/SkyAPM-dotnet)
2. 集成[Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore)
3. 安装nuget包 `Serilog.Sinks.Skywalking`
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog((context, services, configuration) =>
            {
                configuration
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Skywalking(services) //添加这一行
                .WriteTo.Console();
            }) 
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
```

或者在 `MiniApi`项目中

```c#
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
       .MinimumLevel.Debug()
       .Enrich.FromLogContext()
       .WriteTo.Skywalking(services) //添加这一行
       .WriteTo.Console();
});
```

# 高级
自定义Formater

```c#
configuration
    .WriteTo.Skywalking(services, new JsonFormatter())
```
更多Formatter实现查看Serilog文档，或自行实现`ITextFormatter`
