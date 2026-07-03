# Goal 089 Tiered Validation Pipeline Report

- Status: GREEN
- Manual gate: tiered_validation_pipeline_verification required
- Accepted: false
- check-all default behavior preserved: true
- Product code changed: false

## Summary

Goal 089 adds tiered devflow validation without weakening `.devflow/scripts/check-all.ps1`.

Added routes:

- `.devflow/scripts/check-current-goal.ps1`: ordinary feature-goal validation.
- `.devflow/scripts/check-spine-fast.ps1`: recent visual/world/gameplay spine validation.
- `.devflow/scripts/check-all-observed.ps1`: heartbeat/timeout/cleanup wrapper around the unchanged full route.
- `.devflow/validation-profiles/validation-tiers.json`: machine-readable tier profile.
- `docs/VALIDATION_PIPELINE.md`: future task policy.

## Full Check-All Handling

The full route remains `.devflow/scripts/check-all.ps1` and is still authoritative when run. Goal 089 does not claim a fresh full observed pass. It uses the Goal 088A baseline, where full check-all passed with 1235 non-product tests, 0 unexpected warnings and 1110.7 seconds wrapper wall clock.

## Validation Evidence

- `dotnet restore .\LLMGameCreator.sln`: passed.
- `dotnet build .\LLMGameCreator.sln --no-restore`: passed with 6 pre-existing warnings.
- `dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState`: passed 16/16.
- `check-current-goal.ps1` dry-run: passed.
- `check-spine-fast.ps1` dry-run: passed.
- `check-all-observed.ps1` dry-run: passed.
- Final artifact scope / diff checks: recorded in `quality-gate-scan.json` after final validation.

## Scope

Goal 089 changes only devflow scripts, validation profile/docs, state docs, task docs and compact evidence. It does not change product code, Runtime, Unity, public GamePackage schema, providers, LLM/RAG/media execution, Lua, generator-library, project files, dependencies, binary media, heavy logs or prompt dumps.
