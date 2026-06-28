#!/usr/bin/env python3
"""Version-robust e2e verifier: queries the SkyWalking OAP GraphQL API directly
(no swctl version coupling) to assert TRACES, LOGS, and CLR METRICS reached OAP
for each demo service.

Diagnostics go to stderr; stdout prints `result: PASSED` (and exit 0) or
`result: FAILED` (exit 1) so it works both standalone and as a
skywalking-infra-e2e verify case.

Usage: python3 verify.py [oap_graphql_url] svc1 svc2 ...
"""
import sys, json, datetime, urllib.request

URL = "http://localhost:12800/graphql"
args = sys.argv[1:]
if args and args[0].startswith("http"):
    URL = args.pop(0)
SERVICES = args or ["e2e-demo-net8", "e2e-demo-net10"]


def log(*a):
    print(*a, file=sys.stderr)


def gql(query, variables):
    body = json.dumps({"query": query, "variables": variables}).encode()
    req = urllib.request.Request(URL, body, {"Content-Type": "application/json"})
    with urllib.request.urlopen(req, timeout=20) as r:
        out = json.load(r)
    if out.get("errors"):
        raise RuntimeError(out["errors"])
    return out["data"]


def duration(minutes=30):
    now = datetime.datetime.now(datetime.timezone.utc)
    fmt = lambda t: t.strftime("%Y-%m-%d %H%M")
    return {"start": fmt(now - datetime.timedelta(minutes=minutes)), "end": fmt(now), "step": "MINUTE"}


def services():
    d = gql("query($d:Duration!){getAllServices(duration:$d){id name}}", {"d": duration()})
    return {s["name"]: s["id"] for s in d["getAllServices"]}


def instance(service_id):
    d = gql("query($d:Duration!,$s:ID!){getServiceInstances(duration:$d,serviceId:$s){id name}}",
            {"d": duration(), "s": service_id})
    return d["getServiceInstances"][0] if d["getServiceInstances"] else None


def metric_has_value(name, entity):
    d = gql("query($c:MetricsCondition!,$d:Duration!){readMetricsValues(condition:$c,duration:$d){values{values{value isEmptyValue}}}}",
            {"c": {"name": name, "entity": entity}, "d": duration()})
    return any(not v["isEmptyValue"] for v in d["readMetricsValues"]["values"]["values"])


def has_logs(service_id):
    d = gql("query($c:LogQueryCondition){queryLogs(condition:$c){logs{content}}}",
            {"c": {"serviceId": service_id, "queryDuration": duration(), "paging": {"pageNum": 1, "pageSize": 5}}})
    return len(d["queryLogs"]["logs"]) > 0


def main():
    failures = []
    svcs = services()
    log(f"services in OAP: {list(svcs)}")
    for name in SERVICES:
        log(f"--- {name} ---")
        sid = svcs.get(name)
        if not sid:
            failures.append(f"{name}: not registered"); log("  FAIL: not registered"); continue
        try:
            ok = metric_has_value("service_cpm", {"scope": "Service", "serviceName": name, "normal": True})
            log(f"  trace  {'OK' if ok else 'FAIL'} (service_cpm)"); ok or failures.append(f"{name}: no trace cpm")
        except Exception as e:
            failures.append(f"{name}: trace query error {e}"); log(f"  FAIL trace: {e}")
        try:
            ok = has_logs(sid); log(f"  log    {'OK' if ok else 'FAIL'}"); ok or failures.append(f"{name}: no logs")
        except Exception as e:
            failures.append(f"{name}: log query error {e}"); log(f"  FAIL log: {e}")
        inst = instance(sid)
        if not inst:
            failures.append(f"{name}: no instance"); log("  FAIL: no instance"); continue
        try:
            ok = metric_has_value("instance_clr_available_worker_threads",
                                  {"scope": "ServiceInstance", "serviceName": name, "serviceInstanceName": inst["name"], "normal": True})
            log(f"  metric {'OK' if ok else 'FAIL'} (instance_clr_available_worker_threads)"); ok or failures.append(f"{name}: no CLR metric")
        except Exception as e:
            failures.append(f"{name}: metric query error {e}"); log(f"  FAIL metric: {e}")
    if failures:
        log("\nFAILURES:\n  " + "\n  ".join(failures))
        print("result: FAILED")
        sys.exit(1)
    log("\ntrace + log + metric verified for: " + ", ".join(SERVICES))
    print("result: PASSED")


if __name__ == "__main__":
    main()
