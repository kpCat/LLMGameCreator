# Automated Validation Tiers

Status: proposed automation policy.

## Purpose

The owner should not have to manually launch, inspect and debug every goal.
Manual checks should be rare milestone gates. Normal goals must strengthen
automated validation.

Narrow alpha must be an expansion-safe kernel, not a hardcoded demo.
Projection-only goals are not enough for product readiness.
Canonical runtime playthrough is required for the next product milestone.

Validation tiers protect the product-line seams defined in
`docs/PRODUCT_LINE_CORE_STRATEGY.md`: `FeatureModule`, `RuntimePrimitive`,
`SemanticPack`, `VisualPartPack`, `WorldSourceAdapter` and `PlayerAdapter`.

## Tier 0 — Source and Scope Gate

Runs on every goal.

Checks:

```text
dotnet build
focused unit tests
source-health scan
forbidden file mutation scan
artifact-scope guard
current-state docs consistency
```

Existing commands may include:

```powershell
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\run-product-smoke.ps1 -Scenario <scenario-id>
```

## Tier 1 — Package Gate

Checks physical package correctness.

```text
load GamePackage
validate manifest/game/assets/scripts/generated content
validate cross references
validate feature compatibility
validate no fake/missing/tampered artifact paths
```

## Tier 2 — Canonical Runtime Gate

Checks gameplay without Unity.

```text
create GameRuntimeState
execute deterministic command script
verify state transitions
verify diagnostics and event correlation
save state
load state
replay command script
compare state-hash chain
```

This tier is the main replacement for frequent manual checks.

## Tier 3 — Candidate Matrix Gate

Checks generated variation.

```text
generate/read N candidates
validate each package
run Tier 2 for each runnable candidate
score coverage and distinctness
reject duplicate/degenerate/fake-success rows
write matrix result
```

## Tier 4 — Player/Unity Gate

Checks presentation, not game truth.

```text
run Unity batchmode or player smoke
load package or canonical runtime transcript
verify visible markers/HUD/logs
scan logs for fail markers/warnings
apply bounded cleanup
```

Unity must not become the source of gameplay truth.

## Tier 5 — Manual Milestone Gate

Used rarely.

Examples:

```text
play 15-30 minutes
accept/reject alpha milestone
record manual acceptance outside committed raw manual inputs
```

Manual gates should validate product feel, not replace automated correctness.

## Goal Requirements

Every new goal must declare:

```text
validationTiersTouched
normalCommand
expectedPassMarkers
expectedArtifacts
forbiddenScope
manualRequired: true/false
```

A goal that does not improve any tier must justify why it is necessary.
