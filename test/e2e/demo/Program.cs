// Minimal ASP.NET Core demo app for the SkyAPM-dotnet end-to-end test.
// The agent is attached purely via the ASPNETCORE_HOSTINGSTARTUPASSEMBLIES env var
// (no code wiring). GET /work makes an outbound HttpClient call to /downstream so
// the trace contains an inbound (AspNetCore) entry span + an HttpClient exit span
// + a second segment for the downstream entry — and writes an application log.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

// Address this app uses to call itself (the HttpClient exit span). Defaults to the
// in-container listen address; override with DEMO_SELF_URL for host runs.
var self = builder.Configuration["DEMO_SELF_URL"] ?? "http://localhost:8080";

app.MapGet("/healthz", () => "ok");

app.MapGet("/downstream", (ILogger<Program> log) =>
{
    log.LogInformation("downstream hit");
    return "downstream ok";
});

app.MapGet("/work", async (IHttpClientFactory factory, ILogger<Program> log) =>
{
    log.LogInformation("work: calling downstream");
    var client = factory.CreateClient();
    var downstream = await client.GetStringAsync($"{self}/downstream");
    return Results.Ok(new { work = "done", downstream });
});

app.Run();
