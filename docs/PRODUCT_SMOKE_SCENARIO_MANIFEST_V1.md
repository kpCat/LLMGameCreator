# Product Smoke Scenario Manifest v1

Status: Goal 029 contract  
Final gate: `modular_generator_kernel_parallel_readiness_verification`

## Purpose

`product_smoke_scenario_manifest_v1` lets product-smoke scenarios be added by
manifest instead of editing the shared `run-product-smoke.ps1` routing table for
every module-only change.

The runner must check `.devflow/product-smoke-scenarios/<scenario>.json` first.
If a manifest exists, the runner uses its `testFilter` and validates its
expected artifact/report paths when possible. If no manifest exists, the runner
falls back to the existing hardcoded routing for backward compatibility.

## Required Fields

- `scenarioId`: command-line scenario id.
- `testFilter`: xUnit filter passed to `dotnet test`.
- `artifactRoot`: compact root the scenario writes or validates.
- `ownedModuleId`: module id that owns the scenario.
- `expectedReportPath`: deterministic report expected after the scenario.
- `forbiddenPaths`: paths the scenario must not mutate.
- `timeoutPolicy`: deterministic timeout class and seconds.
- `isProductVerticalGate`: true only for rare product vertical gates.
- `allowedForModuleOnlyVerification`: true when Tier 1 module proof may use the
  scenario.

## Validation

Manifest validation must reject:

- missing `testFilter`;
- missing artifact/report path;
- expected report outside artifact root;
- forbidden public schema or historical artifact path claims;
- module-only verification with `isProductVerticalGate=true`;
- a new manifest scenario that still requires a hardcoded runner branch.

## Initial Manifests

Goal 029 creates manifests for:

- `modular-generator-kernel-readiness`;
- `package-assembly-world-entities`;
- `package-assembly-dialogue-quests`.

Other existing scenarios continue through the fallback table until a later
bounded task migrates them.
