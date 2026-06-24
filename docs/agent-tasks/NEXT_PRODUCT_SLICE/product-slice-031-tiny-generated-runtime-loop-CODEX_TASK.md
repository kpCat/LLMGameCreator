# Codex Task - Product Slice 031: Tiny Generated Runtime Loop

## Objective

Implement Product Slice 031: a deterministic tiny generated runtime loop that consumes the Slice 029 procedural game plan and Slice 030 formula/effect/action rule pack, then writes visible state/report sidecars.

Before implementation, fix the currently failing docs guard tests from `CurrentGeneratorStateDocsTests`.

## First gate: fix current red tests

Visual Studio currently shows 3 failures:

- `ReadmeLinksCurrentState`
- `CurrentStateJsonBlocksM5UntilRealEvaluationGatePasses`
- `GeneratorPlanStrictEvaluationDocsLinkedFromCurrentStateOrContextIndex`

These are stale docs guard expectations from the old M4.1 phase. Do not revert the current state to M4.1.

Update docs and/or tests so they validate the current strategy-reset/procedural-generator state after Slices 029 and 030:

- current phase should remain the strategy-reset/playable procedural generator phase;
- recommended next work item before implementation is `tiny_generated_runtime_loop`;
- M5/Lua/full runtime expansion remains locked/blocked;
- README links `docs/CURRENT_GENERATOR_STATE.md`;
- `docs/GENERATOR_PLAN_STRICT_LLM_EVALUATION.md` remains discoverable from current state or context index as historical gate context.

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
```

Continue only after it passes.

## Implement Slice 031

Add an Application-layer tiny loop service under:

`src/LLMGameCreator.Application/Generation/Procedural`

Suggested files:

- `TinyGeneratedRuntimeLoopModels.cs`
- `TinyGeneratedRuntimeLoopService.cs`
- `TinyGeneratedRuntimeLoopMarkdownRenderer.cs`

The service must consume:

- `ProceduralGeneratedGamePlan`
- `FormulaEffectActionRulePack`
- the Slice 030 validation/report result if useful

It must produce deterministic sidecars:

- `.llmgc/procedural/tiny-runtime-loop-state.json`
- `.llmgc/procedural/tiny-runtime-loop-report.json`
- `.llmgc/procedural/tiny-runtime-loop-report.md`

The loop should deterministically:

- choose a start region;
- mark at least one region visited;
- resolve one generated encounter when available;
- advance one generated quest/event when available;
- apply at least one action/effect from the rule pack;
- mutate tiny state through flags, inventory/resource gains, quest progress, and/or faction reputation;
- write diagnostics for missing refs or unsupported action/effect types instead of throwing in normal invalid-input cases.

## Tests and smoke

Add focused tests:

- deterministic same-input output;
- state mutation from action/effect application;
- encounter resolution visible;
- quest/event advancement visible;
- missing/unsupported refs become diagnostics;
- markdown report includes important ids and diagnostics.

Add product smoke:

`tiny-generated-runtime-loop`

The smoke should run:

`generated plan -> formula/effect/action rule pack -> tiny generated runtime loop`

Update:

- `.devflow/scripts/run-product-smoke.ps1`
- `README.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`

After completion, state should recommend:

`generated_package_mvp`

as the next playable-oriented work item.

## Hard constraints

Do not add:

- LLM/provider execution;
- Lua execution;
- Unity execution;
- media generation;
- UI;
- broad GamePackage schema changes;
- broad runtime command/state contract redesign.

Keep outputs deterministic and byte-stable.

## Verification

Run:

```powershell
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~CurrentGeneratorStateDocsTests"
dotnet test .\LLMGameCreator.sln --filter "FullyQualifiedName~TinyGeneratedRuntimeLoop"
.\.devflow\scripts\run-product-smoke.ps1 -Scenario tiny-generated-runtime-loop
.\.devflow\scripts\check-all.ps1
```

If `check-all.ps1` cannot be run, report the exact reason and the narrower checks that passed.

## Completion report

Report:

- files changed;
- sidecars produced;
- verification commands and results;
- whether full `check-all.ps1` passed;
- confirmation that no LLM/provider/Lua/Unity/media execution, UI, broad schema, or broad runtime contract changes were introduced.

