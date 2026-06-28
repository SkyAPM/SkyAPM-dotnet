# Releasing SkyAPM-dotnet

Status: **automated** · Goal: one manual *Run workflow* click **bumps** the version, commits, tags,
packs + publishes all NuGet packages to nuget.org, creates a GitHub Release, and opens the
next-development-version PR.

The workflow that implements this is [`.github/workflows/release.yml`](.github/workflows/release.yml).
It is inert until a NuGet **Trusted Publisher** policy + the `NUGET_USER` repo variable are configured
(see [§4](#4-one-time-setup-nuget-trusted-publishing--no-api-key)).

---

## 1. Current state

- **Packages:** ~30 packable libraries under `src/` (incl. the meta-packages `SkyAPM.Agent.AspNetCore`
  / `SkyAPM.Agent.GeneralHost`) plus the `SkyApm.DotNet.CLI` global tool. Published to **nuget.org**.
- **Version** comes from `build/version.props` (`VersionPrefix`, currently `2.3.0`).
- **Signing** is strong-name via `build/SkyAPM.snk`, which is **committed** in the repo → no signing
  secret is needed; the workflow only needs to build in `Release`.
- **CI** (`.github/workflows/net-ci-it.yml`) builds/tests on pushes and PRs; it never packs or
  pushes. Packing/publishing lives only in `release.yml`.
- **The Cake script is stale** for releasing: `build/version.cake` detects tags only on
  AppVeyor/Travis (not GitHub Actions), so a tag build under GHA would wrongly emit a
  `-preview-<stamp>` suffix; it also globs a non-existent `./cli` path. → The release workflow uses
  **plain `dotnet pack`**, not the Cake `Pack` task.

## 2. How the automated release works

`release.yml` is **manually dispatched** (Actions tab → *Release* → *Run workflow*) with two inputs —
`release_version` and `next_version` — and is **not** triggered by a tag. One run does everything:

1. **Validates** the two inputs and **checks out with submodules** (`recursive`) + sets up .NET 8/10.
2. **Bumps `build/version.props`** to the release version (via `build/set-version.sh`), **commits**
   `Release <version>` and **pushes** to the branch it was run from (default `main`). If the file is
   already at that version (e.g. it was pre-bumped), it skips the commit and just tags.
3. **Tags** `v<release_version>` and pushes the tag.
4. **Builds the protocol first**, then **packs the solution** in `Release`
   (`dotnet pack -p:Version=<release_version> -p:SymbolPackageFormat=snupkg → ./artifacts`). Samples,
   tests, the benchmark, and the e2e demo are `IsPackable=false`; a guard step **aborts before push**
   if any non-shipping package slips into `artifacts/`.
5. **Authenticates via OIDC** — `NuGet/login` exchanges the GitHub OIDC token for a short-lived
   nuget.org key (Trusted Publishing; needs `permissions: id-token: write`) — then **pushes**
   `*.nupkg` + `*.snupkg` with `--skip-duplicate` (re-runs are safe).
6. **Creates a GitHub Release** on the tag with auto-generated notes, marked **prerelease** when the
   release version has a `-suffix`.
7. **Opens a PR** bumping `build/version.props` to `next_version` (`chore/bump-to-<next>`).

> The published version comes from the `release_version` **input**; `VersionQuality` /
> `build/version.cake` are not consulted. The job runs `dotnet nuget push` from `release.yml`, so the
> OIDC claim matches the nuget.org policy (whose *Workflow file* must be `release.yml`).

## 3. Cutting a release

1. Go to **Actions → Release → Run workflow** and enter:
   - **release_version** — e.g. `2.3.0` (or `2.4.0-rc1` for a prerelease)
   - **next_version** — the next development version, e.g. `2.4.0`
2. The run bumps + commits + tags, publishes to nuget.org, creates the GitHub Release, and opens the
   next-version PR. **Merge that PR** to continue development on `next_version`.
3. **Prereleases:** `release_version` `2.4.0-rc1` → published as a NuGet prerelease + GitHub
   *pre-release*; `version.props` is set to the base `2.4.0`.

> **Requirements:** the workflow pushes the release commit + tag directly, so branch protection on
> the run branch must allow the `github-actions` bot to push (or have no direct-push restriction).
> The first run also needs the one-time setup in §4.

## 4. One-time setup (NuGet Trusted Publishing — no API key)

NuGet.org recommends **Trusted Publishing** (OIDC) over long-lived API keys for CI. Set it up once —
**no secret is stored**:

1. **Create a trusted-publisher policy on nuget.org.** Sign in with the account that owns the
   `SkyApm.*` / `SkyAPM.*` packages → click your **username → Trusted Publishing** → add a new policy:
   - **Policy owner:** the nuget.org user/org that owns the packages. Trusted Publishing has **no
     per-package glob** — the policy covers *all* packages owned by the selected owner.
   - **Repository owner** `SkyAPM`, **Repository** `SkyAPM-dotnet`
   - **Workflow file** `release.yml` — the file name only, **not** the `.github/workflows/` path
   - (leave Environment empty — the workflow doesn't use a GitHub Environment)

   > A new policy is "temporarily active for 7 days" until the first successful publish locks in the
   > repo/owner IDs (resurrection-attack protection). Only relevant if the repo is private —
   > SkyAPM-dotnet is public, so it activates permanently on the first publish.
2. **Add a `NUGET_USER` repo variable** (repo → Settings → Secrets and variables → Actions →
   **Variables**, or at the SkyAPM org level) = your **individual nuget.org username / profile name**
   (e.g. `wu-sheng`) — **not** your email, and **not** the org name, even when the policy *owner* is
   the `skyapm` org. You must stay an active member of that org for the policy to remain valid. This
   is a plain *variable*, not a secret.

The strong-name key is already committed, so there is no signing secret either. The workflow's
`permissions: id-token: write` (already set) lets `NuGet/login` perform the OIDC exchange.

> **Fallback:** for local/manual publishing or unsupported CI, an API key still works:
> `dotnet nuget push <pkg> --api-key <key> --source https://api.nuget.org/v3/index.json`.

## 5. Decisions taken (no-ask defaults) & open questions

**Defaults chosen:** publish target is **nuget.org only**; tag convention standardized on
**`vX.Y.Z`** for both the code tag and the GitHub Release (older releases used a separate
`vX.Y.Z-release` tag — dropped); symbols as **`.snupkg`**; the CLI global tool is published in the
**same** release run and version; `net-ci-it.yml` keeps its `v*` trigger for build/test, while
pack/push lives only in `release.yml`.

**Open questions for review:** keep publishing prereleases to the myget `-vnext` feed on `main`
pushes, or nuget.org only? `PackageRequireLicenseAcceptance=True` together with an SPDX
`PackageLicenseExpression` triggers a nuget.org warning — consider dropping the former. Confirm the
committed `SkyAPM.snk` is intentional identity-only signing (not to be rotated to a secret).
