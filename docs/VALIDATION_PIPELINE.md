# Validation Pipeline

Status: Goal 089 process guidance.

## Purpose

LLMGameCreator uses tiered validation so ordinary Codex goals can prove their own scope without treating the full historical `check-all.ps1` run as a mandatory blind wait every time.

This does not weaken full validation. The existing `.devflow/scripts/check-all.ps1` route remains the authoritative full route when it is selected.

## Tiers

### Current Goal

Default command:

```powershell
.\.devflow\scripts\check-current-goal.ps1 -Scenario "<scenario>" -FocusedFilter "<filter>" -ProductSmokeFilter "<filter>"
```

Use this as the default required validation route for ordinary feature goals. It restores and builds unless skipped, runs the provided focused filter, runs the provided product-smoke filter when present, runs `CurrentState`, runs artifact scope for the scenario when present and runs `git diff --check`.

Every future Codex task pack should define the scenario id and focused filter expected by this tier.

### Spine Fast

Default command:

```powershell
.\.devflow\scripts\check-spine-fast.ps1
```

Use this after visual, world, gameplay, validation-policy or shared spine changes. It covers `CurrentState`, the latest high-value visual stack filters and matching product smoke filters when practical.

### Full

Default command:

```powershell
.\.devflow\scripts\check-all.ps1
```

Full check-all is required for consolidation, milestone, shared/core-risk and release-like changes. It is not required for every small feature goal.

### Full Observed

Default command:

```powershell
.\.devflow\scripts\check-all-observed.ps1 -TimeoutMinutes 45 -HeartbeatSeconds 60
```

Use this when the full route is required and the run needs heartbeat, timeout and cleanup diagnostics. It wraps the unchanged full route; it does not skip or remove tests.

## Policy

- `check-current-goal.ps1` is the default route for ordinary feature goals.
- `check-spine-fast.ps1` is recommended after visual/world/gameplay spine changes.
- Full `check-all.ps1` or `check-all-observed.ps1` is required for consolidation, milestone, shared/core-risk and release-like work.
- Full check-all remains authoritative when run.
- Agents should not ask the user to manually run check-all.
- Future Codex task packs should request a validation tier based on scope and must not require the full route for every ordinary goal by default.

## Goal 088A Runtime Baseline

Goal 088A proved that full `check-all.ps1` passed with 1235 non-product tests, 0 failures, 0 skipped tests, 0 warnings and about 1110.7 seconds wrapper wall clock. The root cause was a long-running historical suite and wrapper timeout budget, not a Goal 088 code hang.
