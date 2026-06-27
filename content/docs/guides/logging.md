---
title: "Logging"
weight: 7
---

# Logging

The SkyAPM .NET agent deals with logs in two completely separate ways, and it is
important not to confuse them:

1. **Application log reporting** — your application's `Microsoft.Extensions.Logging`
   records are shipped to the SkyWalking OAP backend (with trace context attached), so
   they show up in the SkyWalking UI alongside traces.
2. **The agent's own self-diagnostic file log** — an internal Serilog file that records
   what the agent itself is doing (startup, gRPC connectivity, errors). This is for
   troubleshooting the agent and is never sent to OAP.

These are configured by two different sections (`SkyWalking:Diagnostics:Logging` and
`SkyWalking:Logging`) and serve different purposes. See the
[Configuration](skyapm_config.md) guide for the full configuration model and an end-to-end
`skyapm.json` sample.

---

## 1. Application log reporting (Microsoft.Extensions.Logging)

The `SkyApm.Diagnostics.MSLogging` integration registers an `ILoggerProvider` that
captures every `Microsoft.Extensions.Logging` record produced by your application and
reports it to the SkyWalking OAP backend. When a record is produced inside an active
trace, the current `TraceId` and `SegmentId` are attached, so the log can be correlated
with its trace in the UI. The reported record also carries the logger category, level,
managed thread id, the active endpoint (operation name), and — for exceptions — the
exception type and a demystified stack trace.

This integration is **enabled by default**. The standard agents
(`SkyApm.Agent.AspNetCore` and `SkyApm.Agent.Hosting`) wire it up automatically when the
agent starts; you do not need to add any logger provider yourself.

### Controlling which records are reported

Only records at or above `SkyWalking:Diagnostics:Logging:CollectLevel` are reported. The
default is `Information`, so `Trace` and `Debug` records are dropped by default.

`CollectLevel` accepts the following values (least to most severe):

| Value         | Reports records at level…                                  |
| ------------- | ---------------------------------------------------------- |
| `Trace`       | Trace and above (everything)                               |
| `Debug`       | Debug and above                                            |
| `Information` | Information and above (**default**)                        |
| `Warning`     | Warning and above                                          |
| `Error`       | Error and above                                            |
| `Critical`    | Critical only                                              |
| `None`        | Nothing — disables application log reporting entirely      |

Set the level in `skyapm.json` (or `skyapm.{Environment}.json`):

```json
{
  "SkyWalking": {
    "Diagnostics": {
      "Logging": {
        "CollectLevel": "Warning"
      }
    }
  }
}
```

You can also set it with an environment variable (double-underscore separators), which
overrides the file:

```bash
export SkyWalking__Diagnostics__Logging__CollectLevel=Warning
```

To stop reporting application logs without touching the rest of your configuration, set
`CollectLevel` to `None`.

> Note on transport: log reporting also requires the agent's log transport to be active,
> which it is by default (`SkyWalking:LogActive` is `true`). Setting `LogActive` to `false`
> stops log records from being sent to OAP regardless of `CollectLevel`.

---

## 2. The agent's self-diagnostic file log (Serilog)

The agent writes its **own** diagnostic log to a rolling file on disk using Serilog. This
captures the agent's internal lifecycle and errors — for example, gRPC connection
problems when the SkyWalking OAP backend is unreachable. It is the first place to look when
the agent does not appear to be reporting data.

**This file is the agent's internal log only. It is not your application's log, and it is
never sent to OAP.** Application logs reach OAP exclusively through the
`Microsoft.Extensions.Logging` reporting described in section 1 above.

It is configured under `SkyWalking:Logging`:

```json
{
  "SkyWalking": {
    "Logging": {
      "Level": "Information",
      "FilePath": "logs/skyapm-{Date}.log",
      "FileSizeLimitBytes": 268435456,
      "RollingInterval": "Day",
      "RollOnFileSizeLimit": false,
      "RetainedFileCountLimit": 10,
      "RetainedFileTimeLimit": 864000000,
      "FlushToDiskInterval": 1000
    }
  }
}
```

### Options

| Key                      | Default                  | Description                                                                                          |
| ------------------------ | ------------------------ | ---------------------------------------------------------------------------------------------------- |
| `Level`                  | `Information`            | Minimum level written to the file. Maps to Serilog levels; an unrecognized value falls back to `Error`. |
| `FilePath`               | `logs/skyapm-{Date}.log` | Path of the log file. `{Date}` is substituted by the rolling logic.                                  |
| `FileSizeLimitBytes`     | `268435456` (256 MB)     | Maximum size of a single file before it is capped (or rolled, see `RollOnFileSizeLimit`).             |
| `RollingInterval`        | `Day`                    | How often a new file is started. Serilog interval name, e.g. `Day`, `Hour`, `Month`, `Year`, `Infinite`. |
| `RollOnFileSizeLimit`    | `false`                  | When `true`, also roll to a new file once `FileSizeLimitBytes` is reached.                            |
| `RetainedFileCountLimit` | `10`                     | Number of rolled files to keep; older files are deleted.                                              |
| `RetainedFileTimeLimit`  | `864000000` (10 days)    | Maximum age of rolled files to keep, in milliseconds.                                                 |
| `FlushToDiskInterval`    | `1000`                   | How often buffered log entries are flushed to disk, in milliseconds.                                  |

The same configuration sources apply as elsewhere: values can come from `skyapm.json`,
`appsettings.json`, the host `IConfiguration`, or environment variables (double-underscore
separators, e.g. `SkyWalking__Logging__Level=Debug`), with environment variables and later
sources overriding the file. See [Configuration](skyapm_config.md) for details.

---

## What this is not

There is **no Serilog sink that reports logs to SkyWalking**. The `SkyWalking:Logging`
(Serilog) section configures only the agent's internal file described in section 2 — it
does not forward any records to OAP. If you want your application logs in the SkyWalking
UI, use the `Microsoft.Extensions.Logging` reporting in section 1 (it is on by default);
write your application logs through `Microsoft.Extensions.Logging` and they will be
collected automatically.

## See also

- [Configuration](skyapm_config.md) — full configuration reference and `skyapm.json` sample.
- [Documentation index](/docs/)
