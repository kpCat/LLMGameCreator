# Goal 072 — Generator Spine Quality Consolidation And Risk Audit

## Purpose

Goal 072 is a quality gate after the aggressive Goal 038–071 feature run.

The goal is to prevent the repository from becoming a pile of green reports with hidden maintainability and proof-quality problems. It must inspect actual source/artifact/test quality, apply bounded safe fixes, and produce a durable technical debt register.

This is not a new gameplay feature. It is also not a license to rewrite everything.

## Product rationale

Recent goals successfully created a broad generated playable/simulatable spine:

- semantic/draft/Lua/sandbox/expansion;
- world/chunk/runtime traversal;
- multi-family loops;
- media materialization and Unity Alpha proofs;
- GamePackage materialization;
- spatial detail;
- gameplay consequences;
- living world;
- interlocked gameplay systems;
- settlement;
- narrative;
- combat/magic;
- weather/day-night/crisis;
- integrated timeline;
- interactive Unity Alpha campaign player.

The risk is now architectural quality:

- large one-off seams;
- repeated SourceLoader/EvidenceService/Hash/Validator/UnityProofRunner patterns;
- increasingly large Unity `AlphaRuntimeBootstrap.cs`;
- tests that might assert reports instead of behavior;
- artifacts that might stop being reproducible;
- line-ending/minification issues;
- proof routes that might drift toward hardcoded success markers.

## Manual handoff

Before implementing Goal 072, record Goal 071 acceptance by user handoff:

```text
unity_alpha_interactive_campaign_player_verification passed before Goal 072
```

Goal 072 itself must remain:

```text
generator_spine_quality_consolidation_verification required
accepted=false
```

## Required evidence

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-072-generator-spine-quality-consolidation/
```

Required artifacts:

```text
quality-inventory-summary.json
source-format-risk-report.json
large-file-and-method-risk-report.json
unity-alpha-bootstrap-risk-report.json
proof-quality-risk-report.json
artifact-reproducibility-risk-report.json
safe-fix-summary.json
technical-debt-register.json
quality-dashboard.json
generator-spine-quality-consolidation-report.md
```

No timestamps, absolute local paths, heavy logs, build folders, or nondeterministic data.

## Quality categories

### P0 — must fix or BLOCKED

- source files that are effectively minified / one-line despite containing many declarations;
- generated/checked-in C# with extreme line lengths that make normal review impossible;
- Goal 071 or current Unity proof printing success markers without reading staged command-plan/evidence;
- artifacts with absolute local machine paths in compact evidence;
- tests that pass while the staged proof input is missing or fake;
- current state docs inconsistent with `CURRENT_GENERATOR_STATE.json`;
- check-all failure.

### P1 — fix if bounded and safe, otherwise register debt

- `AlphaRuntimeBootstrap.cs` becoming too monolithic for the next Unity proof goals;
- very large single-responsibility drift in recent `Design/*` seams;
- product smoke tests that check only `Passed=true` without checking counts, hashes, deltas, matched/missing markers;
- repeated private helpers that can be safely local-extracted without changing public contracts;
- artifact-scope allowlist gaps for the current goal;
- brittle current-state guard tests that will fail on the next gate transition.

### P2 — register, do not fix in this goal unless trivial

- broader shared infrastructure extraction across many seams;
- unifying all source loaders/evidence writers;
- creating a generic proof-runner framework;
- full Unity Alpha architecture refactor;
- moving generated proof features into Runtime/GamePackage;
- adding analyzers or permanent build enforcement.

### P3 — ignore

- cosmetic formatting not linked to readability/reviewability;
- stylistic preferences;
- renaming for taste.

## Safe fix policy

Allowed safe fixes:

- line-ending/readability fixes in Goal 071 and recent generated code when truly needed;
- small private helper extraction in `AlphaRuntimeBootstrap.cs`;
- strengthening tests in existing goal-specific test files;
- removing absolute path/timestamp leaks from current goal evidence;
- adding deterministic quality scanner/evidence code;
- adding a debt register with concrete, prioritized, actionable items.

Forbidden broad fixes:

- large-scale refactor across all previous goals;
- changing public GamePackage schema;
- changing Runtime/Runtime.Abstractions contracts;
- changing WinForms/UI;
- changing provider/LLM/RAG;
- changing Lua execution model;
- adding dependencies;
- changing `.sln` / `.csproj`;
- rewriting Unity Alpha architecture.

## Expected outcome

A GREEN Goal 072 means:

- P0 issues are absent or fixed;
- bounded P1 issues are fixed;
- non-bounded P1/P2 issues are registered;
- Goal 071 quality audit remains green;
- Unity Alpha proof chain still works;
- check-all is green;
- technical debt is visible and prioritized.

A BLOCKED Goal 072 is acceptable if the audit finds real P0/P1 risks that cannot be safely fixed in the allowed scope. It must still commit/push the evidence and debt register.
