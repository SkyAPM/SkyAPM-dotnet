# Plugin tests

Per-plugin tests that verify a diagnostic plugin produces the **right trace** by asserting on the
agent's **real emitted segments** — the only thing that is real. Modelled on the
[Apache SkyWalking Java plugin-test framework](https://skywalking.apache.org/docs/skywalking-java/next/en/setup/service-agent/java-agent/plugin-test/).

Unlike [`../e2e`](../e2e) (a full, heavy OAP + BanyanDB backend), these tests report to a lightweight
**dedicated mock collector** — a SkyWalking receiver that captures what the agent sends and validates
it — and run a scenario **version-by-version** against each supported version of the instrumented
library. That cross-version coverage is what catches breakages like
[#565](https://github.com/SkyAPM/SkyAPM-dotnet/issues/565).

## Layout

```
test/plugin/
  run.sh                       runner: builds the collector, then per scenario × version:
                               build app image (agent from source) → run with collector → drive → validate
  scenarios/
    cap-scenario/              first scenario: SkyApm.Diagnostics.CAP
      Case.csproj / Program.cs minimal app: real DotNetCore.CAP (in-memory) + the CAP plugin
      skyapm.json              agent config (reports to mock-collector:19876)
      Dockerfile               agent built FROM SOURCE; TFM + CAP_VERSION are build args
      docker-compose.yml       the scenario app + the mock collector
      support-version.list     "<TFM> <version>" per line — latest patch of each supported minor
      config/expectedData.yaml expected emitted spans (SkyWalking validator DSL: nq 0, not null, …)
```

## The dedicated collector

The mock collector (`mock-collector` from
[apache/skywalking-agent-test-tool](https://github.com/apache/skywalking-agent-test-tool)) speaks the
agent's V8 gRPC protocol on `:19876` and exposes HTTP `:12800`:

- `GET /receiveData` — everything it captured, as YAML.
- `POST /dataValidate` — validate captured data against a posted `expectedData.yaml` (`200`/`success` = pass).

`run.sh` builds it from a **pinned commit** (the same one `apache/skywalking-java` pins today); the
published `skyapm/mock-collector:latest` image is from 2019 and predates the V8 management protocol the
current agent needs.

## Run

```bash
bash test/plugin/run.sh                 # all scenarios, all versions
bash test/plugin/run.sh cap-scenario    # one scenario

# reuse a pre-built collector image (skips the Java build):
COLLECTOR_IMAGE=skyapm/mock-collector:<tag> bash test/plugin/run.sh
```

Requires `docker`; building the collector also needs `git` + a JDK. CI runs it via
[`.github/workflows/plugin-test.yml`](../../.github/workflows/plugin-test.yml).

## Add a scenario

1. `scenarios/<lib>-scenario/` with a minimal app that references the agent + the plugin **from source**
   and the instrumented library at `$(<lib>Version)`, exposing `/case/<name>` (entry) and
   `/case/healthCheck`.
2. A `Dockerfile` (agent from source; library version as a build arg) and a `docker-compose.yml`.
3. `support-version.list` — the latest patch of each supported minor.
4. `config/expectedData.yaml` — author it from a real run: `curl localhost:12800/receiveData`, then keep
   the spans you want to assert and replace volatile fields with matchers. See
   [Plugin testing](../../content/docs/guides/plugin-testing.md).
