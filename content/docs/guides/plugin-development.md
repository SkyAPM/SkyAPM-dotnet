---
title: "Plugin development"
weight: 40
---

SkyAPM-dotnet instruments libraries through **diagnostic plugins**. A plugin is a thin adapter
that subscribes to a library's [`DiagnosticSource`](https://learn.microsoft.com/dotnet/api/system.diagnostics.diagnosticsource)
events and turns them into SkyWalking **trace segments**. This guide shows how to write one,
using the existing plugins (`SkyApm.Diagnostics.HttpClient`, `SkyApm.Diagnostics.SqlClient`,
`SkyApm.Diagnostics.CAP`, …) as the reference shape.

## How a plugin works

Most .NET libraries already emit `DiagnosticSource` events (ASP.NET Core, HttpClient,
EF Core, gRPC, CAP, MongoDB, …). A plugin listens to the named listener and maps each event to a
span. The agent owns the wiring:

```
library DiagnosticSource  ──►  TracingDiagnosticProcessorObserver  (matches ListenerName)
                                        │
                                        ▼
                          your ITracingDiagnosticProcessor          (one [DiagnosticName] method per event)
                                        │  CreateEntry/Local/ExitSegmentContext + AddTag/AddLog
                                        ▼
                                  ITracingContext.Release  ──►  emitted SegmentRequest (the real trace)
```

You implement only the middle box.

## 1. Implement `ITracingDiagnosticProcessor`

Create a class in a `SkyApm.Diagnostics.<Library>` project that implements
[`ITracingDiagnosticProcessor`](https://github.com/SkyAPM/SkyAPM-dotnet/blob/main/src/SkyApm.Abstractions/Diagnostics/ITracingDiagnosticProcessor.cs).
Its `ListenerName` must equal the library's `DiagnosticSource`/`DiagnosticListener` name, and each
event handler is annotated with `[DiagnosticName("<the.event.name>")]`:

```csharp
public class MyLibTracingDiagnosticProcessor : ITracingDiagnosticProcessor
{
    public string ListenerName => "MyLib.DiagnosticListener";   // the source's name

    private readonly ITracingContext _tracingContext;
    private readonly IExitSegmentContextAccessor _exitSegmentContextAccessor;
    private readonly TracingConfig _tracingConfig;

    public MyLibTracingDiagnosticProcessor(ITracingContext tracingContext,
        IExitSegmentContextAccessor exitSegmentContextAccessor,
        IConfigAccessor configAccessor)
    {
        _tracingContext = tracingContext;
        _exitSegmentContextAccessor = exitSegmentContextAccessor;
        _tracingConfig = configAccessor.Get<TracingConfig>();
    }

    [DiagnosticName("MyLib.Command.Before")]
    public void BeforeCommand([Object] CommandEventData data)   // [Object] binds the raw event payload
    {
        var context = _tracingContext.CreateExitSegmentContext("MyLib/" + data.Operation, data.Host);
        context.Span.SpanLayer = SpanLayer.DB;
        context.Span.Component = Components.MYLIB;          // see "Components" below
        context.Span.AddTag(Tags.DB_STATEMENT, data.Sql);
    }

    [DiagnosticName("MyLib.Command.After")]
    public void AfterCommand([Object] CommandEventData data)
    {
        var context = _exitSegmentContextAccessor.Context;
        if (context == null) return;
        _tracingContext.Release(context);                  // Release emits the segment
    }

    [DiagnosticName("MyLib.Command.Error")]
    public void ErrorCommand([Object] CommandErrorEventData data)
    {
        var context = _exitSegmentContextAccessor.Context;
        if (context == null) return;
        context.Span.ErrorOccurred(data.Exception, _tracingConfig);
        _tracingContext.Release(context);
    }
}
```

Key points:

- The `[Object]` parameter attribute tells the dispatcher to pass the event's raw payload object;
  its type is the library's event class, which the plugin references at compile time.
- A handler that throws does not crash the app — `TracingDiagnosticObserver` catches it. A
  **structural binding error** (`MissingMethodException`/`TypeLoadException`, i.e. the library
  version is incompatible with what the plugin compiled against) is logged **once** and that handler
  is then disabled, rather than spamming on every event (see *Version coupling* below).

## 2. Create segments with `ITracingContext`

[`ITracingContext`](https://github.com/SkyAPM/SkyAPM-dotnet/blob/main/src/SkyApm.Abstractions/Tracing/ITracingContext.cs)
is the only API you need to produce spans. Pick the span kind by where the work happens:

| Method | Use for | Accessor to read it back |
|---|---|---|
| `CreateEntrySegmentContext(operationName, carrierHeader)` | inbound work (a received request/message) | `IEntrySegmentContextAccessor` |
| `CreateLocalSegmentContext(operationName)` | in-process work (persistence, serialization) | `ILocalSegmentContextAccessor` |
| `CreateExitSegmentContext(operationName, networkAddress[, carrierHeader])` | outbound work (a DB/HTTP/MQ call) | `IExitSegmentContextAccessor` |

Always pair a `Create…` with a `Release(context)` (typically on the *After*/*Error* event). On the
matching *After* you usually read the context back from the accessor rather than threading it through.
On every span set:

- `Span.SpanLayer` — `Http` / `Database` (DB) / `RPCFramework` / `MQ` / `Cache` / `Unknown`.
- `Span.Component` — the component id (below).
- `Span.Peer` — the remote address for exit spans (a [peer formatter](#peers) helps here).
- `Span.AddTag(Tags.X, value)` and `Span.AddLog(LogEvent.…)` for detail; `Span.ErrorOccurred(ex, config)` on failure.

## 3. Cross-process context propagation

For spans that cross a process boundary (an MQ message, an outbound RPC), inject/extract the
SkyWalking `sw8` headers so the trace stays connected:

- **Producer / client (exit):** build an `ICarrierHeaderCollection` over the outbound message
  headers and call `ICarrierPropagator.Inject(context, header)` so the downstream side continues the trace.
- **Consumer / server (entry):** wrap the inbound headers in an `ICarrierHeaderCollection` and pass it
  to `CreateEntrySegmentContext(operationName, carrierHeader)`; the agent extracts the parent ref.

`SkyApm.Diagnostics.CAP`'s `CapCarrierHeaderCollection` is a concrete example of adapting a library's
message headers to `ICarrierHeaderCollection`.

## Components

Span components are numeric ids from the shared SkyWalking registry
([`component-libraries.yml`](https://github.com/apache/skywalking/blob/master/oap-server/server-starter/src/main/resources/component-libraries.yml)),
surfaced as `SkyApm.Common.Components`. Reuse an existing id where one fits (e.g. `Components.HTTPCLIENT`,
`Components.SQLCLIENT`, `Components.CAP`); a brand-new library needs an id registered upstream in
SkyWalking first, otherwise the UI shows it as "Unknown".

## Peers

Exit spans should carry the remote address in `Span.Peer`. Where the address must be parsed from a
connection/config object, add a `SkyApm.PeerFormatters.<Library>` implementing `IDbPeerFormatter`
(see `SkyApm.PeerFormatters.MySqlConnector`) and resolve it through the injected `IPeerFormatter`.

## 4. Register the plugin

Expose an opt-in extension on `SkyApmExtensions` that registers your processor as an
`ITracingDiagnosticProcessor` singleton — the agent discovers all registered processors and
subscribes each to its `ListenerName`:

```csharp
public static class SkyWalkingBuilderExtensions
{
    public static SkyApmExtensions AddMyLib(this SkyApmExtensions extensions)
    {
        extensions.Services.AddSingleton<ITracingDiagnosticProcessor, MyLibTracingDiagnosticProcessor>();
        return extensions;
    }
}
```

Users opt in via the setup lambda:

```csharp
services.AddSkyAPM(ext => ext.AddMyLib());
```

A small, always-on default set is registered automatically; everything else (CAP, MassTransit,
MongoDB, …) is opt-in like the above.

## Version coupling (important)

A plugin is **compiled against a specific version** of the library it instruments, but at runtime
NuGet unifies that library to the **application's** installed version. If the app's version changed
the diagnostic event types in a binary-incompatible way, the handler throws `MissingMethodException`
at runtime (see [#565](https://github.com/SkyAPM/SkyAPM-dotnet/issues/565)). The agent now degrades
gracefully (logs once, disables the handler), but the plugin still won't trace that library until the
versions line up. Two consequences for plugin authors:

- Pin the plugin's `DotNetCore.X` reference per target framework (see the existing per-TFM
  `Condition="'$(TargetFramework)' == 'netX'"` `PackageReference` blocks) and document the supported range.
- **Test the plugin against the real library, version by version** — see
  [Plugin testing](plugin-testing.md).
