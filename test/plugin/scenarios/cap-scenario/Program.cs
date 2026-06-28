// CAP plugin test scenario for the SkyAPM-dotnet plugin-test framework.
//
// The agent core is activated zero-code via ASPNETCORE_HOSTINGSTARTUPASSEMBLIES
// (see Dockerfile); this app additionally registers the opt-in CAP tracing plugin
// and runs real DotNetCore.CAP with its in-memory storage + queue (no broker).
//
// GET /case/cap publishes a CAP message that an in-process subscriber consumes, so
// the agent emits a CAP publisher span (within the inbound entry segment) and a CAP
// subscriber segment to the mock collector. The runner then validates those emitted
// segments against config/expectedData.yaml.

using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using Savorboard.CAP.InMemoryMessageQueue;
using SkyApm;
using SkyApm.Diagnostics.CAP;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCap(x =>
{
    x.UseInMemoryStorage();
    x.UseInMemoryMessageQueue();
});
builder.Services.AddSingleton<CapSubscriber>();

// Register the opt-in CAP tracing plugin. AddCap() does exactly this; we register
// directly to avoid a second AddSkyAPM call on top of the HostingStartup activation.
builder.Services.AddSingleton<ITracingDiagnosticProcessor, CapTracingDiagnosticProcessor>();

var app = builder.Build();

app.MapGet("/case/healthCheck", () => "success");

app.MapGet("/case/cap", async (ICapPublisher publisher) =>
{
    await publisher.PublishAsync("skyapm.plugin.test.cap", new CapMessage { Body = "hello-skyapm" });
    return "ok";
});

app.Run();

public class CapMessage
{
    public string Body { get; set; }
}

public class CapSubscriber : ICapSubscribe
{
    [CapSubscribe("skyapm.plugin.test.cap")]
    public void Handle(CapMessage message)
    {
        // consume; the act of subscribing produces the CAP subscriber trace
    }
}
