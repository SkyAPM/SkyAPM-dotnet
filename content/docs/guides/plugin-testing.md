---
title: "Plugin testing"
weight: 41
---

A diagnostic plugin is only correct if the agent **emits the right trace** — and the emitted trace is
the only thing that is real (intermediate state isn't). So plugin tests run the **real** instrumented
library, let the agent report to a **dedicated mock collector**, and assert on the **captured
segments**. Because a plugin is compiled against one version of a library but runs against whatever
version the application installs (the cause of [#565](https://github.com/SkyAPM/SkyAPM-dotnet/issues/565)),
each scenario runs **version-by-version**.

This mirrors the [Apache SkyWalking Java plugin-test framework](https://skywalking.apache.org/docs/skywalking-java/next/en/setup/service-agent/java-agent/plugin-test/).
The harness lives in [`test/plugin/`](https://github.com/SkyAPM/SkyAPM-dotnet/tree/main/test/plugin);
this guide explains how it works and how to add a scenario. (For a full OAP + storage backend test
instead, see the heavier [`test/e2e/`](https://github.com/SkyAPM/SkyAPM-dotnet/tree/main/test/e2e).)

## How it works

```
scenario app  ──gRPC :19876──►  mock collector  ──HTTP :12800──►  /receiveData  /dataValidate
(agent from source,             (a SkyWalking receiver,           (captured       (validate vs
 real library)                   not real OAP+DB)                  segments)        expectedData.yaml)
```

For each scenario and each `<TFM> <version>` line in its `support-version.list`, `run.sh`:

1. builds the scenario app image — the **agent from source** plus the instrumented library at that
   version (NuGet unifies the version up, so the plugin runs against it just like a real app);
2. boots the app together with the mock collector;
3. drives the scenario's `/case/<name>` entry endpoint;
4. POSTs the scenario's `config/expectedData.yaml` to the collector's `/dataValidate`; `200`/`success`
   passes, otherwise the validator prints which span/field differed.

### The dedicated collector

The collector is `apache/skywalking-agent-test-tool`'s `mock-collector`, built from a **pinned commit**
(the same one `apache/skywalking-java` pins today). The published `skyapm/mock-collector:latest` is from
2019 and predates the V8 management protocol the current agent needs, so do **not** use it — `run.sh`
builds the pinned version for you (override with `COLLECTOR_IMAGE=…` to reuse a pre-built one).

## Authoring `expectedData.yaml`

Run the scenario once and read what the collector actually captured:

```bash
curl -s http://localhost:12800/receiveData
```

Then keep the segments/spans you want to assert and replace volatile fields with matchers. The validator
([SkyWalking validator DSL](https://skywalking.apache.org/docs/skywalking-java/next/en/setup/service-agent/java-agent/plugin-test/#expecteddatayaml-format))
matches **expected segments as a subset** of the captured ones (so you needn't describe every segment),
but within a matched segment it checks **span count, tags, and span logs exactly** — so describe a span's
full tag/log set, using matchers for anything variable.

Matchers: `not null`, `not blank`, `nq <v>` (not-equal), `gt`/`ge <v>`, `eq <v>` (default), `start with`,
`end with`. Example from the CAP scenario (the publisher span):

```yaml
segmentItems:
  - serviceName: cap-scenario
    segmentSize: nq 0
    segments:
      - segmentId: not null
        spans:
          - operationName: CAP/skyapm.plugin.test.cap/Publisher
            spanLayer: MQ
            componentId: 3004        # Components.CAP
            spanType: Entry
            startTime: nq 0          # volatile -> matcher
            endTime: nq 0
            isError: false
            peer: localhost
            tags:
              - {key: mq.topic, value: skyapm.plugin.test.cap}
              - {key: mq.broker, value: localhost}
            logs:
              - logEvent: [{key: event, value: Event Publishing Start}]
              - logEvent: [{key: message, value: CAP message publishing start...}]
              - logEvent: [{key: event, value: Event Publishing End}]
              - logEvent: [{key: message, value: not blank}]   # variable content
```

## Adding a scenario

1. Create `test/plugin/scenarios/<lib>-scenario/` with a minimal app referencing the agent and the
   plugin **from source** and the instrumented library at a build-arg version, exposing `/case/<name>`
   (which performs the instrumented action) and `/case/healthCheck`.
2. Add `skyapm.json` (report to `mock-collector:19876`), a `Dockerfile` (agent built from source;
   library version as a build arg), and a `docker-compose.yml` (app + collector).
3. Add `support-version.list` — the **latest patch of each supported minor** of the library.
4. Author `config/expectedData.yaml` from a real `/receiveData` capture, as above.
5. Run `bash test/plugin/run.sh <lib>-scenario` until it passes.

See [Plugin development](plugin-development.md) for writing the plugin itself.
