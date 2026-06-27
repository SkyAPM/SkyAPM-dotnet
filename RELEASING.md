# Releasing SkyAPM-dotnet

Status: **proposed automation** · Goal: pushing a `vX.Y.Z` tag **automatically** builds, packs,
signs, publishes all NuGet packages to nuget.org, and creates a GitHub Release.

The draft workflow that implements this is [`.github/workflows/release.yml`](.github/workflows/release.yml).
It is inert until the `NUGET_API_KEY` secret is added (see [§4](#4-one-time-setup)).

---

## 1. Current state

- **Packages:** ~30 packable libraries under `src/` (incl. the meta-packages `SkyAPM.Agent.AspNetCore`
  / `SkyAPM.Agent.GeneralHost`) plus the `SkyApm.DotNet.CLI` global tool. Published to **nuget.org**.
- **Version** comes from `build/version.props` (`VersionPrefix`, currently `2.3.0`).
- **Signing** is strong-name via `build/SkyAPM.snk`, which is **committed** in the repo → no signing
  secret is needed; the workflow only needs to build in `Release`.
- **CI today** (`.github/workflows/net-ci-it.yml`) already triggers on `v*` tags but only
  **builds/tests** — it never packs or pushes. Releasing is currently manual.
- **The Cake script is stale** for releasing: `build/version.cake` detects tags only on
  AppVeyor/Travis (not GitHub Actions), so a tag build under GHA would wrongly emit a
  `-preview-<stamp>` suffix; it also globs a non-existent `./cli` path. → The release workflow uses
  **plain `dotnet pack`**, not the Cake `Pack` task.

## 2. How the automated release works

`release.yml` triggers on **`push: tags: ['v*']`** and:

1. **Checks out with submodules** (`recursive`) so the `protocol-v3` protobuf source is present.
2. **Sets up .NET 8 + 10.**
3. **Derives and verifies the version:** `VERSION=${GITHUB_REF_NAME#v}` and asserts that its base
   (without any `-rc`/`-beta` suffix) equals `VersionPrefix` in `build/version.props`. A mismatch
   **fails the job** — preventing a `v2.3.0` tag from publishing a differently-numbered package.
4. **Builds the protocol project first**, then **packs the solution** in `Release`:
   `dotnet pack -p:Version=$VERSION -p:ContinuousIntegrationBuild=true -p:SymbolPackageFormat=snupkg`
   → `./artifacts`. (Samples are excluded via `sample/Directory.Build.props` → `IsPackable=false`;
   test projects are already `IsPackable=false`.)
5. **Pushes** `*.nupkg` + `*.snupkg` to nuget.org with `--skip-duplicate` (re-runs are safe).
6. **Creates a GitHub Release** for the tag with auto-generated notes
   (`gh release create --generate-notes`), marked **prerelease** when the tag has a `-suffix`.

## 3. Cutting a release

1. **Bump the version** in `build/version.props` (`VersionMajor`/`Minor`/`Patch`) on a PR and merge
   it. (The consistency check in step 3 above turns a forgotten bump into a hard failure rather than
   a mis-numbered publish.)
2. **Tag and push:**
   ```bash
   git tag v2.3.0
   git push origin v2.3.0
   ```
3. The **Release** workflow runs → packages on nuget.org + a GitHub Release appear.
4. **Prereleases:** tag like `v2.4.0-rc1` → published as a NuGet prerelease and a GitHub
   *pre-release*. (`version.props` may stay at `2.4.0`; only the base is compared.)

## 4. One-time setup

- **Add the `NUGET_API_KEY` secret** (repo → Settings → Secrets and variables → Actions, or at the
  SkyAPM org level). Create the key on nuget.org scoped to **Glob `SkyApm.*` and `SkyAPM.*`** with
  push + push-new-package rights.
- That's the only required secret. The strong-name key is already in the repo.

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
