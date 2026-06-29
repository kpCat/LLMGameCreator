# Codex task — GOAL 034 Strict LLM Draft Artifact Loop

## Assignment metadata

Repository:

```text
https://github.com/kpCat/LLMGameCreator
```

Working copy:

```text
C:\Users\endim\LLMGameCreator\
```

Branch:

```text
main
```

Composite goal id/name:

```text
goal-034-strict-llm-draft-artifact-loop-v1
Goal 034: Strict LLM Draft Artifact Loop
```

Required goal marker / gate marker:

```text
strict_llm_draft_artifact_loop_verification
```

Codex reasoning level:

```text
very high
```

## Starting state

Start only if local state matches the accepted Goal 033 handoff:

- current branch: `main`;
- Goal 033 accepted by user decision: `semantic_authoring_intent_resolver_verification passed`;
- current recommended next work: `goal_034_strict_llm_draft_artifact_loop`;
- Goal 031 and Goal 032 remain produced-for-review/not passed if the current docs say so;
- Goal 034 is not already implemented.

If the local repo differs materially, stop and report. Do not invent state.

## Required `/goal` behavior

This is a `/goal` task. Maintain the goal until final gate evidence is produced or a stop condition is reached.

Do not mark `strict_llm_draft_artifact_loop_verification` passed. This goal produces reviewable evidence and leaves the manual gate required.

## Purpose

Implement the strict draft artifact loop that prevents future LLM/manual/import candidates from bypassing the deterministic generator.

The loop must be Application-layer and BCL-only. It must not call LLM providers. It must not generate final prose. It must not materialize `GamePackage` content. It must define and test how draft requests, quarantined candidate envelopes, deterministic validation, repair requests and promotion decisions work.

The answer to “What became more real?” must be:

```text
Future LLM/manual/import output can only enter the generator as quarantined contract-bound draft candidates, and the program deterministically validates, repairs or rejects them before any promotion.
```

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CURRENT_GENERATOR_STATE.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CONTEXT_INDEX.md`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP.md`
8. `docs/GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER_SPEC.md` if present
9. `docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md` if present
10. `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md` if present
11. `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md` if present
12. Existing source/tests under:
    - `src/LLMGameCreator.Application/Design/SemanticAuthoringIntentResolver/`
    - `src/LLMGameCreator.Application/Design/DynamicSemanticFeatures/`
    - `src/LLMGameCreator.Application/Design/SemanticPackComposition/`
    - `src/LLMGameCreator.Application/Design/SemanticArtifactContracts/`
    - matching test folders under `tests/LLMGameCreator.Tests/`
13. Existing current-state guard tests that may need narrow current-state update after Goal 034, especially tests recently repaired for Goal 033.

Use targeted search. Do not read the whole repo.

## Allowed files / areas

You may create or edit:

```text
docs/EXTERNAL_SCOUTING_GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP.md
docs/GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP_SPEC.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/StrictLlmDraftArtifactLoop/**
tests/LLMGameCreator.Tests/Application/StrictLlmDraftArtifactLoop/**
tests/LLMGameCreator.Tests/ProductSmoke/StrictLlmDraftArtifactLoopProductSmokeTests.cs
.llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/**
```

Bounded test scope extension is allowed only if `check-all.ps1` exposes stale current-state/handoff assertions equivalent to the Goal 033 repair pattern. In that case you may update only the specific failing current-state guard test files and must preserve strict historical assertions while replacing stale latest-gate hardcoding with current-state consistency.

## Forbidden files / areas

Do not modify unless a stop condition asks the user for explicit permission:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Generation/** provider/LLM call paths
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
templates/**
samples/**
*.sln
*.csproj
*.Designer.cs
package.json / public GamePackage schema/model files
```

Also forbidden:

- external NuGet dependencies;
- LLM/provider/RAG calls;
- final prose/dialogue/quest/lore text generation as accepted output;
- prompt-only implementation;
- Lua execution;
- media/image/audio generation;
- weakening acceptance/evidence tests;
- broad refactors;
- branch, merge, rebase, cherry-pick, reset, stash, clean, force push.

## Exact behavior

### 1. Preflight

- Confirm branch `main`.
- Confirm current state says Goal 033 was accepted and Goal 034 is recommended/not started.
- Confirm no uncommitted user changes outside this task.
- If untracked launcher/task files from previous goals remain, do not delete them unless the user explicitly asked. Report them and continue only if they do not affect build/test scope.

### 2. Model: strict draft loop

Implement small BCL-only models under `Design/StrictLlmDraftArtifactLoop`.

The domain must represent draft requests, candidate envelopes, repair requests and promotion decisions. Use typed BCL records/classes, stable identifiers and stable ordering.

#### Draft request

Required meanings:
- request id;
- scenario/profile id;
- target draft family;
- source intent ids from Goal 033 where possible;
- allowed artifact contract ids;
- allowed semantic scopes;
- required field names;
- forbidden field names;
- maximum candidates;
- expected source kinds;
- no-final-prose flag;
- no-runtime-authority flag;
- repair policy id;
- deterministic ordering key.

#### Candidate envelope

Required meanings:
- candidate id;
- request id;
- source kind: `manual`, `llm`, `imported`, `programmatic_fixture`;
- provenance id/details;
- draft family;
- typed payload fields;
- linked intent ids/features/contracts/scopes;
- declared constraints;
- status: `quarantined`, `rejected`, `repair_required`, `promotable`, `promoted`;
- diagnostics.

The model must not use arbitrary dynamic runtime objects or provider-specific response objects.

#### Repair request

Required meanings:
- repair request id;
- candidate id;
- request id;
- blocking diagnostic codes;
- allowed fields to fix;
- fields that must not be changed;
- semantic context hints/digest;
- retry number / max retry count;
- status.

This is a data record only, not a provider call.

#### Promotion decision

Required meanings:
- candidate id;
- request id;
- target draft artifact id;
- promoted boolean;
- reasons;
- diagnostics;
- preserved provenance;
- status.

Promotion means “accepted as a draft artifact/candidate for later pipeline”. It does not mean final prose and does not mean `GamePackage` materialization.

### 3. Draft families

Seed a deterministic catalog with at least these families:

```text
lore_rule_draft
species_archetype_feature_draft
faction_relation_draft
npc_role_personality_draft
quest_motive_objective_draft
dialogue_act_template_slot_draft
economy_item_resource_hint_draft
combat_ability_progression_hint_draft
settlement_region_event_hint_draft
```

Each family must define required fields, forbidden fields and allowed semantic/intent scopes.

The dialogue family must explicitly forbid final dialogue prose. It may allow dialogue acts, tone tags, template slot ids, state conditions and localization key hints.

### 4. Request builder

Implement deterministic request creation from Goal 033-style scenarios. It can use static/fixture integration with the existing Goal 033 catalog/evidence style, but must not duplicate huge data or read heavy artifacts unless the repository pattern already does that.

At minimum produce request sets for:

- `frontier_survival`;
- `gothic_intrigue`;
- `caravan_trade`;
- `metamodule_kingdoms`.

Metamodule scenario must prove many species/archetype slots can be represented as draft requests without asking the LLM to write final content.

### 5. Validator

Implement deterministic causal validation for:

- duplicate request id;
- duplicate candidate id;
- unknown request;
- wrong family;
- missing required field;
- forbidden final prose field;
- provider/runtime/UI/Unity/Lua/GamePackage/code generation leakage;
- candidate self-marked promoted/accepted without promotion decision;
- source/provenance mismatch;
- missing intent trace;
- missing feature/contract/scope trace where required;
- fake target contract;
- fake semantic scope;
- incompatible scenario/profile;
- over-budget candidate count;
- invalid repair target;
- repair request attempts to modify immutable fields;
- nondeterministic ordering mutation.

Diagnostics must have stable codes. Ordinary validation failures must return diagnostics, not throw.

### 6. Repair loop planner

Implement a deterministic repair planner that converts blocking diagnostics into repair request records.

The repair planner must:
- classify fixable vs non-fixable diagnostics;
- list allowed fields to fix;
- list immutable fields;
- preserve original candidate provenance;
- cap retries;
- reject repair attempts for leakage/provider/runtime/code/GamePackage/final-prose violations if appropriate.

No provider calls. No prompt text as the primary artifact. If a human-readable hint is useful, keep it inside a bounded field and test that it does not contain runtime/provider instructions.

### 7. Promotion decision engine

Implement a deterministic promotion decision engine:
- accepts valid quarantined candidates only;
- rejects candidates with blocking diagnostics;
- may set repair_required for fixable diagnostics;
- preserves provenance;
- creates stable target draft artifact ids;
- never writes GamePackage definitions;
- never outputs final prose as accepted content.

### 8. Evidence writer

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-034-strict-llm-draft-artifact-loop/
```

Required files:

```text
draft-loop-contract-summary.json
draft-request-matrix.json
candidate-quarantine-matrix.json
repair-request-matrix.json
promotion-decision-matrix.json
strict-draft-plan-frontier.json
strict-draft-plan-gothic.json
strict-draft-plan-caravan.json
strict-draft-plan-metamodule-kingdoms.json
invalid-draft-diagnostics-matrix.json
strict-llm-draft-artifact-loop-report.md
```

Evidence rules:
- stable ordering;
- compact JSON;
- no absolute paths;
- no nondeterministic timestamps unless repo has a deterministic convention;
- no heavy logs;
- report must contain `strict_llm_draft_artifact_loop_verification required` and `accepted=false`;
- report must state no provider/LLM/RAG call happened, no final prose was generated/promoted, and no GamePackage materialization happened.

### 9. Docs/current-state update

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected state:
- Goal 033 remains accepted/passed.
- Goal 031 and Goal 032 remain produced-for-review/not passed if currently recorded that way.
- Goal 034 is produced for review.
- Active/manual gate: `strict_llm_draft_artifact_loop_verification required`.
- Goal 035 remains future/not started.

Do not mark Goal 034 passed.

## Tests

Add focused tests in existing style. Suggested test classes:

```text
StrictLlmDraftArtifactLoopCatalogTests
StrictLlmDraftRequestBuilderTests
StrictLlmDraftCandidateValidatorTests
StrictLlmDraftRepairPlannerTests
StrictLlmDraftPromotionDecisionTests
StrictLlmDraftArtifactLoopEvidenceTests
StrictLlmDraftArtifactLoopProductSmokeTests
```

Tests must prove:
- draft family catalog is deterministic and validates cleanly;
- request builder produces distinct scenario request sets;
- metamodule scenario can express many species/archetype slots without final prose;
- candidate quarantine defaults are safe;
- valid candidates become promotable/promoted only through the promotion engine;
- invalid/fake/leak cases produce causal diagnostics;
- repair planner creates bounded repair records and does not call providers;
- final prose fields are rejected, especially for dialogue/quest/lore;
- provider/runtime/UI/Unity/Lua/GamePackage/code generation leakage is rejected;
- evidence files are written and directly inspectable;
- current-state docs are consistent.

## Validation commands

Run focused checks first:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~StrictLlmDraftArtifactLoop|FullyQualifiedName~Goal034"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~StrictLlmDraftArtifactLoopProductSmokeTests"

Get-ChildItem .\.llmgc\procedural\goal-034-strict-llm-draft-artifact-loop -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-034-strict-llm-draft-artifact-loop\strict-llm-draft-artifact-loop-report.md -TotalCount 120
```

Then run final gate:

```powershell
.\.devflow\scripts\check-all.ps1
```

After green check-all, run the existing artifact scope guard for Goal 034 if the repo has the standard script/pattern. Do not invent a new scope guard implementation.

## Stop conditions

Stop and report without commit/push if:

1. You need to call a real LLM/provider/RAG.
2. You need to generate final prose/dialogue/quest/lore content as accepted output.
3. You need to change GamePackage schema/model.
4. You need to touch WinForms/UI, Runtime, Unity, Lua, generator-library, `.sln`, `.csproj`, provider paths or external dependencies.
5. Focused tests fail and cannot be fixed inside allowed scope.
6. `check-all.ps1` fails and the failing files are outside allowed/bounded current-state guard scope.
7. Evidence contains absolute paths, timestamps without deterministic convention, heavy logs or final prose promoted as accepted content.
8. Any candidate can self-declare accepted/promoted without the promotion engine.
9. The implementation becomes a giant monolithic class.

## Git policy

Allowed preflight/inspection:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <changed-files>
```

Allowed final commands only after green final gate and clean scope:

```powershell
git add <changed-files>
git commit -m "Goal 034 strict LLM draft artifact loop"
git push origin main
```

Forbidden always unless the user gives a separate explicit instruction:

```text
git checkout
git switch
git merge
git rebase
git cherry-pick
git reset
git stash
git clean
git push --force
```

## Final report format

Report in Russian:

```text
Goal 034 выполнен / остановлен

Gate:
strict_llm_draft_artifact_loop_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<catalog/request builder/candidate validator/repair planner/promotion/evidence>

Evidence artifacts:
<required files>

Сценарии:
<frontier/gothic/caravan/metamodule counts>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases and matched expectations>

Git:
<commit hash and push result, or no-commit/no-push reason>

Ограничения:
<confirm provider/LLM/RAG not called; no final prose; no GamePackage/UI/Runtime/Unity/Lua/generator-library/external deps touched>

Следующий разумный шаг:
<Goal 035 or alternative, one concise paragraph>
```
