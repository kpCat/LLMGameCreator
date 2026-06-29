# Codex task — GOAL 035 Lua Module Manifest Registry

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
goal_035_lua_module_manifest_registry
Goal 035: Lua Module Manifest Registry
```

Required goal marker / gate marker:

```text
lua_module_manifest_registry_verification
```

Codex reasoning level:

```text
very high
```

## Current process policy update

For LLMGameCreator tasks, final commit/push is required even if the result is GREEN, BLOCKED or FAILED.

Do not pretend blocked work is accepted. Do not mark manual gates passed unless the user explicitly says so. But do push the final state so GitHub remains the source of truth.

Commit message policy:

```text
Goal 035 lua module manifest registry
```

if final checks are green enough for the requested manual gate state.

```text
BLOCKED Goal 035 lua module manifest registry
```

if implementation/evidence is present but final gate is blocked.

```text
FAILED Goal 035 lua module manifest registry
```

if the implementation could not be completed but diagnostics or partial artifacts should be preserved.

Final report must clearly state `GREEN`, `BLOCKED`, or `FAILED`.

## Starting state

This task starts after Goal 034 manual acceptance.

Expected current docs state:

- `semantic_authoring_intent_resolver_verification passed`;
- `strict_llm_draft_artifact_loop_verification passed`;
- Goal 031 and Goal 032 may remain produced-for-review/not passed;
- recommended next work is `goal_035_lua_module_manifest_registry`;
- Goal 035 implementation is not started.

If the local state contradicts this, report it and continue only if the contradiction is a stale-doc wording issue that can be corrected in the allowed scope.

## Purpose

Add a BCL-only Application-layer Lua module manifest registry that defines how future Lua/manual/import/LLM-generated modules are declared, reviewed, selected and bounded before any Lua execution is allowed.

This is a safety/architecture goal, not a Lua runtime goal.

What becomes more real:

```text
Future Lua/manual/import/LLM module output can only become selectable through deterministic manifest records, host API surface policy, dependency planning, provenance checks and invalid/fake/leak diagnostics before any executor is allowed.
```

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CURRENT_GENERATOR_STATE.md`
3. `docs/CURRENT_GENERATOR_STATE.json`
4. `docs/CONTEXT_INDEX.md`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_035_LUA_MODULE_MANIFEST_REGISTRY_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_035_LUA_MODULE_MANIFEST_REGISTRY.md`
8. `docs/GOAL_034_STRICT_LLM_DRAFT_ARTIFACT_LOOP_SPEC.md` if present
9. `docs/GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER_SPEC.md` if present
10. `docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md` if present
11. `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md` if present
12. `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md` if present
13. Existing source/tests under:
    - `src/LLMGameCreator.Application/Design/StrictLlmDraftArtifactLoop/`
    - `src/LLMGameCreator.Application/Design/SemanticAuthoringIntentResolver/`
    - `src/LLMGameCreator.Application/Design/DynamicSemanticFeatures/`
    - `src/LLMGameCreator.Application/Design/SemanticPackComposition/`
    - `src/LLMGameCreator.Application/Design/SemanticArtifactContracts/`
    - matching tests under `tests/LLMGameCreator.Tests/`

Do not read the whole repository unless local search shows a needed pattern is elsewhere.

## Allowed files / areas

You may create or edit:

```text
docs/GOAL_035_LUA_MODULE_MANIFEST_REGISTRY_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_035_LUA_MODULE_MANIFEST_REGISTRY.md
docs/agent-tasks/GOAL_035_LUA_MODULE_MANIFEST_REGISTRY.md
docs/agent-tasks/GOAL_035_LAUNCHER.txt

docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md

src/LLMGameCreator.Application/Design/LuaModuleManifestRegistry/**
tests/LLMGameCreator.Tests/Application/LuaModuleManifestRegistry/**
tests/LLMGameCreator.Tests/ProductSmoke/LuaModuleManifestRegistryProductSmokeTests.cs

.llmgc/procedural/goal-035-lua-module-manifest-registry/**
```

Pre-authorized bounded repair areas, only if needed by final checks and only for stale current-state/handoff guards:

```text
tests/LLMGameCreator.Tests/Devflow/*CurrentState* or specific stale handoff guard tests
tests/LLMGameCreator.Tests/Application/**/**/*AcceptanceTests.cs
```

Use these only to remove stale assumptions about the latest active gate while preserving strict historical assertions.

## Forbidden files / areas

Do not modify unless explicitly required by a pre-authorized bounded repair above:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Generation/**
src/LLMGameCreator.Scripting/**
unity/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
*.Designer.cs
```

Also forbidden:

- adding NuGet/external dependencies;
- executing Lua;
- parsing Lua source;
- generating Lua source;
- changing public GamePackage schema;
- calling provider/LLM/RAG;
- generating final prose;
- runtime host binding;
- Unity work;
- UI work;
- broad refactors.

## Exact behavior to implement

### 1. Preflight

- Confirm branch is `main`.
- Confirm local state corresponds to Goal 034 accepted and Goal 035 not started.
- Use existing Application-layer design/test/evidence style.

### 2. Lua module manifest model

Create small BCL-only models for Lua module manifest governance.

Each manifest should represent at least:

- module id;
- family id;
- version;
- display name;
- description/summary;
- lifecycle status;
- target dialect declaration;
- source/provenance;
- profile/scenario compatibility;
- semantic scopes;
- artifact contract ids / intent families;
- dependencies;
- allowed host API groups;
- denied host API groups or denied operation kinds;
- side-effect class;
- resource budget metadata;
- promotion/review status;
- deterministic ordering key.

Suggested statuses:

```text
ready
optional
blocked
future_required
deprecated
draft
quarantined
review_required
```

Suggested target dialect values:

```text
manifest_only
lua_5_2_future
lua_5_4_future
lua_5_5_or_later_future
```

Do not execute or parse Lua.

### 3. Seed registry

Seed a deterministic registry covering at least these module families:

- world generation hints;
- region/biome/weather/hazard rules;
- NPC/species/archetype rules;
- faction/reputation/social relation rules;
- quest/objective/reward rules;
- dialogue act/tone/localization hint rules;
- item/resource/recipe/loot/economy rules;
- combat/stat/ability/status rules;
- settlement/building/landmark rules;
- event/global pressure rules;
- metamodule species/archetype expansion rules.

The `metamodule_kingdoms` scenario should demonstrate many species/archetype slot module declarations without creating a huge file or giant class.

### 4. Host API surface policy

Create host API surface declarations.

At minimum include API groups like:

```text
semantic.read
feature.read
intent.read
quest.plan
dialogue.intent
economy.plan
combat.plan
world.plan
event.plan
metamodule.expand
```

Also include denied/future/blocked groups for:

```text
filesystem
network
os_process
reflection
provider_llm_rag
ui_winforms
runtime_direct_mutation
unity_direct_call
gamepackage_schema_mutation
arbitrary_code_generation
implicit_lua_execution
```

The validator must reject leakage attempts into denied groups.

### 5. Validator

Implement deterministic validation with stable diagnostic codes.

Cover at least:

- duplicate module id;
- invalid module id;
- duplicate family id conflict;
- unknown dependency;
- dependency cycle;
- unknown host API group;
- denied host API group used as allowed;
- missing required semantic scope;
- unknown artifact contract/intent family reference;
- fake profile/scenario;
- provenance mismatch;
- draft/quarantined candidate marked ready without review;
- over-budget module;
- future-required module treated as ready;
- side-effect class mismatch;
- final prose content;
- Lua source/execution claim;
- provider/LLM/RAG leak;
- Runtime/UI/Unity/GamePackage schema leak;
- nondeterministic ordering mutation.

Ordinary validation failures must return diagnostics, not throw.

### 6. Planner / selector

Implement a deterministic planner that accepts a scenario/profile id and selected context, then returns:

- selected manifests;
- dependency order;
- blocked manifests;
- future-required manifests;
- missing dependencies;
- denied API diagnostics;
- compatibility diagnostics;
- stable summary counts.

Required scenarios:

```text
frontier_survival
gothic_intrigue
caravan_trade
metamodule_kingdoms
```

The four scenarios must produce meaningfully different module selections.

### 7. Goal 034 compatibility

The manifest registry should model compatibility with strict draft loop concepts:

- request family id for future Lua module manifests;
- quarantined candidate provenance;
- repair-required diagnostics;
- promotion decision preconditions.

Do not call providers or use the Goal 034 implementation as a service unless a clean in-process model dependency already exists and is appropriate. A lightweight compatibility record is enough.

### 8. Evidence writer

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-035-lua-module-manifest-registry/
```

Required files:

```text
lua-module-registry-summary.json
lua-host-api-surface-policy.json
lua-module-selection-frontier.json
lua-module-selection-gothic.json
lua-module-selection-caravan.json
lua-module-selection-metamodule-kingdoms.json
lua-module-dependency-plan.json
invalid-lua-manifest-diagnostics-matrix.json
lua-module-manifest-registry-report.md
```

Evidence must be deterministic:

- no timestamps;
- no absolute paths;
- no heavy logs;
- stable ordering;
- compact JSON.

Report must contain:

```text
lua_module_manifest_registry_verification required
```

Do not mark accepted/passed.

### 9. Docs/current-state update

Update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
```

Expected final state:

- Goal 034 accepted/passed remains recorded;
- Goal 035 produced for review;
- active/manual gate is `lua_module_manifest_registry_verification required`;
- Goal 036 recommended but not started;
- Goal 031/032 remain produced-for-review/not passed unless the current docs intentionally preserve that status.

## Tests

Add focused tests in existing style.

Suggested classes:

```text
LuaModuleManifestRegistryTests
LuaModuleManifestValidatorTests
LuaModuleManifestPlannerTests
LuaModuleManifestEvidenceTests
LuaModuleManifestRegistryProductSmokeTests
```

Tests must prove:

- seed registry validates cleanly;
- host API surface policy is deterministic and denies dangerous groups;
- scenario selections are deterministic and meaningfully different;
- dependency ordering is stable;
- metamodule scenario includes many species/archetype slot declarations without massive code duplication;
- evidence artifacts exist and parse;
- invalid/fake/leak matrix has causal diagnostic codes;
- no Lua execution/source generation/provider/runtime/UI/Unity/GamePackage mutation is required.

## Validation commands

Run from:

```text
C:\Users\endim\LLMGameCreator\
```

Commands:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LuaModuleManifestRegistry|FullyQualifiedName~LuaModuleManifest|FullyQualifiedName~Goal035"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~LuaModuleManifestRegistryProductSmokeTests"

Get-ChildItem .\.llmgc\procedural\goal-035-lua-module-manifest-registry -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-035-lua-module-manifest-registry\lua-module-manifest-registry-report.md -TotalCount 120

.\.devflow\scripts\check-all.ps1
```

Run final artifact scope guard if it exists in the repo's standard flow. Do not invent a new guard.

## Pre-authorized bounded repairs

To reduce repeated blocking, these bounded repairs are allowed during this task:

### A. Stale current-state / handoff guard tests

If `check-all.ps1` fails only because old tests hardcode the previous latest gate or previous latest slice, you may update the affected tests so that:

- historical goal-specific assertions remain strict;
- current active gate is read from `CURRENT_GENERATOR_STATE.json`;
- current gate is checked against `CURRENT_GENERATOR_STATE.md` and `CONTEXT_INDEX.md`;
- tests do not become meaningless `not null` checks.

Include exact changed test files in final report.

### B. Accidental historical artifact mutations

If `check-all.ps1` mutates tracked historical artifacts outside Goal 035 scope, you may run:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

Only for exact accidental historical artifacts, not for Goal 035 code/docs/evidence.

Before restoring, list exact paths. After restoring, run `git status --short --untracked-files=all` and continue.

### C. Historical runtime/Unity log evidence

If unrelated historical tests require two known old generated logs and those logs are missing from ignored root paths, you may copy them only from an existing real generated cache/log path to the exact expected path, with source->target recorded in final report.

Do not fabricate log contents.

### D. Final commit/push even if blocked

Commit and push final state regardless of GREEN/BLOCKED/FAILED.

## Git policy

Allowed:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <changed-files>
git add <changed-files>
git commit -m "<message>"
git push origin main
git restore --source=HEAD -- <exact accidental historical artifact paths>   # only under bounded repair rule B
```

Forbidden:

```text
git checkout
git switch
git reset
git clean
git stash
git merge
git rebase
git cherry-pick
git push --force
```

Final commit/push is mandatory.

Commit messages:

- GREEN: `Goal 035 lua module manifest registry`
- BLOCKED: `BLOCKED Goal 035 lua module manifest registry`
- FAILED: `FAILED Goal 035 lua module manifest registry`

## Stop / classify conditions

Do not silently continue as if green.

Classify as BLOCKED and commit/push if:

- Goal 035 implementation/evidence exists but `check-all.ps1` fails outside allowed scope;
- an unrelated historical evidence problem cannot be repaired under bounded rules;
- artifact scope guard fails outside Goal 035 and bounded repair scope;
- stale tests require broader modernization than allowed.

Classify as FAILED and commit/push if:

- core implementation cannot compile;
- focused tests fail and cannot be fixed within allowed scope;
- implementation would require forbidden areas or external dependencies.

Classify as GREEN and commit/push if:

- focused tests pass;
- check-all passes;
- artifacts are present and clean;
- gate remains `lua_module_manifest_registry_verification required`.

## Final report format

Report in Russian:

```text
Goal 035 выполнен / заблокирован / провален

Status:
GREEN / BLOCKED / FAILED

Gate:
lua_module_manifest_registry_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<registry / API policy / validator / planner / evidence>

Evidence artifacts:
<список файлов>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered cases>

Bounded repairs:
<none or exact stale tests/artifacts/logs repaired>

Git:
<commit hash and push result>

Ограничения:
<confirm no Lua execution/provider/runtime/UI/Unity/GamePackage/etc.>

Следующий разумный шаг:
<Goal 036 recommendation, no implementation started unless requested>
```
