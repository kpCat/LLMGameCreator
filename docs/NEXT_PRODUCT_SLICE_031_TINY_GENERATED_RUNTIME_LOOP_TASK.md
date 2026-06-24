# Product Slice 031 - Tiny Generated Runtime Loop

## Purpose

Product Slice 031 must move the project from "generated plan and rule pack exist" to "the generated plan and rules can drive a tiny deterministic gameplay loop".

The target is not a full game runtime. The target is a small Application-layer simulation that proves the generated artifacts from Slice 029 and Slice 030 are executable enough to produce visible state transitions:

- enter a generated region;
- resolve a generated encounter;
- apply at least one generated action/effect;
- advance at least one generated quest/event;
- update a tiny player/world state report;
- write deterministic JSON and Markdown sidecars.

This slice must also repair the currently failing documentation guard tests before adding new functionality.

## Current failing tests to fix first

Visual Studio currently reports 662/665 passing and 3 failed tests under:

`LLMGameCreator.Tests.Docs.CurrentGeneratorStateDocsTests`

Observed failing tests:

- `ReadmeLinksCurrentState`
- `CurrentStateJsonBlocksM5UntilRealEvaluationGatePasses`
- `GeneratorPlanStrictEvaluationDocsLinkedFromCurrentStateOrContextIndex`

The failures are stale M4.1 documentation expectations. The current product direction after Slices 029 and 030 is strategy-reset/playable-procedural-generator work, with Slice 031 recommended as `tiny_generated_runtime_loop`.

Do not make these tests pass by reverting `CURRENT_GENERATOR_STATE` back to M4.1.

Repair them by updating tests and/or docs so the current state is internally consistent.

Expected repair direction:

- `CURRENT_GENERATOR_STATE.json`
  - `current_phase` should describe the strategy-reset/playable procedural generator phase, not `m4_1_real_model_evaluation_gate`.
  - `recommended_next_work_item` before Slice 031 should be `tiny_generated_runtime_loop`.
  - M5/Lua/runtime-expansion locks must remain represented as blocked/locked until explicitly unlocked by a future milestone.
- `README.md`
  - Must still link to `docs/CURRENT_GENERATOR_STATE.md`.
  - Must describe the current procedural-generator direction after Slices 029/030.
  - Must not imply the next step is the old M4.1 real-model evaluation gate.
- `docs/CURRENT_GENERATOR_STATE.md` and/or `docs/CONTEXT_INDEX.md`
  - Must include a link/reference to `docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md` as historical/locked-gate context if the guard still requires it.
  - Must clearly identify Slice 031 as the next work item before implementation.
- `CurrentGeneratorStateDocsTests`
  - Update stale assertions to the current state names and current recommended work item.
  - Keep meaningful guard coverage: README links current state, current state JSON parses, blocked milestones remain blocked, strict LLM evaluation documentation is still discoverable.

Run at least:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Proceed to implementation only after these docs tests pass.

## Functional goal

Add a deterministic tiny generated runtime loop that consumes:

- `ProceduralGeneratedGamePlan` from Slice 029;
- `FormulaEffectActionRulePack` and validation output from Slice 030.

The loop should produce:

- `.llmgc/procedural/tiny-runtime-loop-state.json`
- `.llmgc/procedural/tiny-runtime-loop-report.json`
- `.llmgc/procedural/tiny-runtime-loop-report.md`

Exact filenames may be adjusted only if there is a stronger existing naming convention in the repo. Keep paths under `.llmgc/procedural`.

## Required behavior

The loop must deterministically choose or derive:

- a starting region from the generated plan;
- one region entry/movement/exploration event;
- one encounter seed from the generated plan;
- one quest/event seed from the generated plan;
- at least one applicable action/effect from the Slice 030 rule pack.

The output state/report must show at minimum:

- seed/mode/source plan identity;
- starting region id;
- visited region ids;
- resolved encounter id or diagnostic explaining why none was resolvable;
- advanced quest/event id or diagnostic explaining why none was advanceable;
- applied action/effect ids;
- player inventory/resource delta, granted item, or equivalent reward state;
- at least one flag/state mutation;
- faction reputation delta if an applicable reputation effect exists;
- diagnostics for missing refs, unsupported action/effect types, or invalid rule pack state.

Invalid or incomplete input must not crash normal operation. It should produce diagnostics and a deterministic report.

## Architecture constraints

Keep this in the Application layer unless the existing codebase strongly indicates a better nearby home.

Suggested namespace/path:

`src/LLMGameCreator.Application/Generation/Procedural`

Suggested new files:

- `TinyGeneratedRuntimeLoopModels.cs`
- `TinyGeneratedRuntimeLoopService.cs`
- `TinyGeneratedRuntimeLoopMarkdownRenderer.cs`

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- broad runtime command/state contract redesign;
- GamePackage schema changes;
- UI.

If an existing runtime or scripting helper is safely reusable without changing public contracts, it may be used. Otherwise prefer a small isolated Application-layer simulation service.

## Determinism requirements

For identical inputs, outputs must be byte-stable after repository-normalized paths and timestamps are handled according to existing project conventions.

Avoid current time, random GUIDs, environment-specific absolute paths, nondeterministic dictionary iteration, and culture-sensitive formatting.

Use explicit ordering for all collections written to JSON/Markdown.

## Validation requirements

The service must validate or diagnose:

- missing generated plan;
- missing or invalid rule pack;
- unresolved region/encounter/quest/action/effect references;
- unsupported action/effect types;
- rule pack validation failures from Slice 030;
- empty plan sections where the loop cannot run.

Diagnostics must be written to JSON/Markdown reports.

## Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/Procedural`

Suggested test file:

`TinyGeneratedRuntimeLoopServiceTests.cs`

Required test coverage:

- deterministic output for identical generated plan + rule pack;
- successful loop applies at least one action/effect and mutates tiny runtime state;
- encounter resolution is visible in state/report;
- quest/event advancement is visible in state/report;
- missing refs or unsupported effect/action types produce diagnostics instead of unhandled exceptions;
- Markdown report contains the important ids and diagnostic summary;
- docs guard tests fixed and passing.

Add product smoke coverage under:

`tests/LLMGameCreator.Tests/ProductSmoke`

Suggested test file:

`TinyGeneratedRuntimeLoopSmokeTests.cs`

The smoke scenario should run the generated pipeline:

`Slice 029 plan -> Slice 030 rule pack -> Slice 031 tiny runtime loop`

and verify all expected sidecars exist.

## Devflow and docs updates

Update:

- `.devflow/scripts/run-product-smoke.ps1`
- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

After Slice 031 is complete, `CURRENT_GENERATOR_STATE` should state that:

- Slice 031 exists and produces a tiny generated runtime loop;
- M5/Lua/full runtime expansion remains locked unless explicitly unlocked;
- the next recommended work item is a generated package/playable MVP mapping step.

Recommended next work item name after Slice 031:

`generated_package_mvp`

Acceptable wording:

`Generated Package MVP: map generated plan/rules/tiny-loop output into the minimal package/runtime path needed for a visible playable prototype.`

Do not recommend more report/review infrastructure as the primary next item unless it directly blocks the playable path.

## Verification commands

Run in this order:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~TinyGeneratedRuntimeLoop"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario tiny-generated-runtime-loop
.\.devflow\scripts\check-all.ps1
```

If `check-all.ps1` cannot be run, state the exact reason and provide the narrower checks that were run. Do not report the slice as fully accepted without either `check-all.ps1` passing or an explicit explanation of the gap.

## Acceptance criteria

Slice 031 is acceptable only if:

- the 3 currently failing docs guard tests are fixed;
- targeted runtime-loop tests pass;
- product smoke for `tiny-generated-runtime-loop` passes;
- `CURRENT_GENERATOR_STATE.json` parses;
- generated sidecars are deterministic and under `.llmgc/procedural`;
- no LLM/provider/Lua/Unity/media execution is introduced;
- no broad GamePackage/runtime contract redesign is introduced;
- docs point to the next playable-oriented step instead of another pure infrastructure slice.

## Non-goals

Do not implement a full game.

Do not build UI.

Do not unlock M5, Lua authoring, Unity runtime execution, or provider execution.

Do not implement external maps/OSM.

Do not create a large ECS/runtime framework.

Do not rewrite Slice 029 or Slice 030 unless a small compatibility fix is required.

Do not add broad test suites unrelated to this slice.

