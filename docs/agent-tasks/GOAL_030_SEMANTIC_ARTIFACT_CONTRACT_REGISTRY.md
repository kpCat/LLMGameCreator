# Codex task — GOAL 030 Semantic Artifact Contract Registry And Semantic Pack Compatibility Kernel

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
goal-030-semantic-artifact-contract-registry-v1
Goal 030: Semantic Artifact Contract Registry And Semantic Pack Compatibility Kernel
```

Required goal marker / gate marker:

```text
semantic_artifact_contract_registry_verification
```

Codex reasoning level:

```text
very high
```

## Starting gate assumption

This task is issued after the user-reported acceptance of Goal 029 / modular generator kernel / parallel readiness and after post-adoption evidence restoration. Treat the user handoff as the source of the task authorization to start Goal 030.

Before code changes, confirm the local repository state:

1. You are in `C:\Users\endim\LLMGameCreator\`.
2. Current branch is `main`.
3. There are no uncommitted user changes, or you can clearly separate them from your work.
4. Current docs/evidence are compatible with starting Goal 030.

If local docs still contain stale wording that says Goal 029 is `required`, but the surrounding local evidence/docs mention the post-adoption evidence restoration and the user handoff explicitly authorizes Goal 030, update the state docs consistently as part of this task. Do not fake test results or mark Goal 030 as accepted.

Final state after this task must be:

```text
semantic_artifact_contract_registry_verification required
```

Do not mark this gate passed inside the same goal. The goal produces reviewable evidence.

## Purpose

Implement the next generator-generalization layer after the modular generator kernel:

- a deterministic artifact contract registry for full-generator artifact families;
- a semantic pack compatibility kernel;
- a deterministic semantic expansion planning seam;
- compact evidence artifacts proving three different profile/style scenarios use the same registry/planner;
- focused tests, product smoke proof, invalid/fake/leak matrix, and current-state docs update.

This goal must make future NPC, quest, biome, faction, economy, item, combat, settlement, event and dialogue generation cheaper and safer. It must not be just a better report.

The answer to “What became more real?” should be:

```text
Future generator modules can now ask one deterministic registry which artifact contracts and semantic expansion slots are valid for a selected profile/semantic-pack set, instead of hardcoding isolated vertical paths.
```

## Read-first list

Read these first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_030_SEMANTIC_ARTIFACT_REGISTRY.md`
8. `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md` if present
9. `docs/ARTIFACT_CONTRACTS.md` if present
10. `docs/GAME_GENERATION_CAPABILITY_MATRIX.md` if present
11. `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md` if present
12. `docs/GAME_BLUEPRINT_CAPABILITY_GRAPH_SPEC.md` if present
13. `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md` if present
14. Existing Goal 029 / modular generator kernel docs and artifacts. Search narrowly by `modular_generator_kernel_parallel_readiness_verification`, `package assembly module registry`, `product-smoke scenario manifest`, and `module absence`.
15. Existing source/tests around:
    - `src/LLMGameCreator.Application/Design/`
    - `src/LLMGameCreator.Application/Design/SemanticCatalog/`
    - package assembly module registry / product smoke manifests if present
    - `tests/LLMGameCreator.Tests/` tests for SemanticCatalog, package assembly, Goal 029 evidence, product smoke.

Do not read the whole repository unless a local search shows the exact relevant files are elsewhere.

## Allowed files / areas

You may create or edit only these areas:

```text
docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_030_SEMANTIC_ARTIFACT_REGISTRY.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/**
tests/LLMGameCreator.Tests/**
.llmgc/procedural/goal-030-semantic-artifact-contract-registry/**
```

If the existing repository already has a more specific Application subfolder for artifact contracts, semantic catalog, generator kernel, package assembly registry, or product smoke manifests, use that style and folder instead of inventing a parallel structure.

## Forbidden files / areas

Do not modify unless the task becomes impossible and you stop first for user decision:

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
package.json public GamePackage schema/model changes
```

Also forbidden:

- adding external NuGet dependencies;
- weakening or deleting existing tests/evidence tests;
- calling LLM/provider/RAG;
- executing Lua;
- generating media;
- broad refactors unrelated to this goal;
- branch, merge, rebase, cherry-pick, reset, stash, force-push.

## Exact behavior to implement

### 1. Preflight and local pattern discovery

- Confirm branch `main`.
- Run a narrow status check before changes.
- Identify existing registry/service/test naming style under `src/LLMGameCreator.Application/Design/` and `tests/LLMGameCreator.Tests/`.
- Reuse existing validation/result/diagnostic patterns where practical.
- Keep classes small; do not create a God service.

### 2. Artifact contract registry V1

Implement a deterministic in-memory registry for full-generator artifact contract families. Exact class names may follow local style, but the model must represent these meanings:

- stable contract id;
- display name;
- version;
- artifact kind/domain;
- produced artifact types;
- consumed artifact types;
- required semantic scopes;
- optional semantic scopes;
- capability tags;
- compatibility tags;
- dependencies;
- module owner/module id where known;
- lifecycle status: `ready`, `optional`, `blocked`, `future_required`, `deprecated` or equivalent;
- validator/diagnostic code prefix;
- notes/proof/status text.

Seed the registry with enough contracts to cover the full-generator spine, at minimum:

- game profile / capability bundle;
- semantic pack / semantic catalog;
- world topology / region graph / route graph;
- biome/weather/hazard/event hints;
- entity archetype / NPC actor profile;
- faction/reputation/social relation;
- dialogue/string-table/localization hint;
- quest graph/objective/reward pattern;
- item/resource/recipe/loot/economy;
- combat/progression/ability;
- settlement/building/landmark;
- UI/export/presentation IR as future or optional, without touching UI/Unity.

Use existing accepted profile/style ids if they exist. If not discoverable, use three deterministic test scenario ids:

```text
frontier_survival
gothic_intrigue
caravan_trade
```

### 3. Registry validator

Implement validation for registry seed definitions:

- duplicate contract ids;
- invalid/empty ids;
- invalid version;
- unknown dependencies;
- dependency cycles;
- missing produced artifact kind;
- missing semantic scope for semantic-dependent contracts;
- unknown lifecycle status;
- incompatible tag declarations;
- future-required contracts accidentally marked as ready;
- forbidden leakage tags or notes that imply runtime/provider/LLM/Lua/UI/GamePackage schema mutation as part of Goal 030.

Diagnostics must be deterministic and machine-readable with stable codes. Do not throw for ordinary validation failures; return diagnostics.

### 4. Semantic pack descriptor and compatibility kernel

Implement BCL-only semantic pack descriptors and compatibility planning. This is not RDF, not RAG, and not runtime data.

Represent at least:

- pack id;
- supported profile/family ids;
- semantic scopes;
- semantic tags;
- relation hints;
- expansion hints;
- blocked/future capability hints;
- deterministic ordering key.

Implement a planner/resolver that accepts a selected profile/style and selected semantic packs, then returns:

- selected compatible contract ids;
- selected semantic pack ids;
- dependency order;
- missing dependencies;
- conflicts;
- blocked/future-required items;
- module absence diagnostics;
- semantic expansion slots;
- stable summary.

The planner must be deterministic. Same input twice must produce byte-equivalent JSON evidence or structurally equivalent model output.

### 5. Semantic expansion slots

Create semantic expansion slots as planning records only. They must not mutate `GamePackage` and must not call generation providers.

Minimum slot families:

- NPC/actor archetype variation;
- faction/reputation relation;
- quest motive/objective pattern;
- dialogue tone/localization/string-table hint;
- biome/weather/hazard/event hint;
- item/resource/recipe/loot hint;
- combat/progression/ability hint;
- settlement/region/route/landmark hint.

Each slot should include:

- stable slot id;
- source semantic pack id;
- target artifact contract id or artifact kind;
- profile/family id;
- semantic scopes/tags used;
- deterministic priority/order;
- status: ready/optional/blocked/future-required;
- diagnostics if blocked.

### 6. Product smoke route and evidence writer

Add a focused product-smoke route that exercises three distinct profile/style scenarios through the same registry and compatibility planner.

Write compact deterministic evidence under:

```text
.llmgc/procedural/goal-030-semantic-artifact-contract-registry/
```

Required files:

```text
registry-summary.json
compatibility-matrix.json
semantic-expansion-plan-frontier.json
semantic-expansion-plan-gothic.json
semantic-expansion-plan-caravan.json
semantic-artifact-contract-registry-report.md
```

If existing profile ids differ, adapt file names but keep the intent and make names stable.

Evidence requirements:

- no timestamps unless the repository already has a deterministic timestamp convention;
- no absolute machine paths;
- no heavy logs/build outputs;
- stable ordering;
- compact JSON;
- report must contain the exact marker `semantic_artifact_contract_registry_verification required`.

### 7. Invalid/fake/leak matrix

Tests must include causal negative cases, not only happy path:

- duplicate id mutation;
- unknown dependency mutation;
- dependency cycle mutation;
- missing semantic scope mutation;
- incompatible tag mutation;
- module absent mutation;
- future-required contract incorrectly treated as ready;
- fake contract id accepted by planner;
- leakage attempt: runtime/provider/LLM/Lua/UI/GamePackage-schema tag or note treated as allowed.

Each mutation must produce a specific diagnostic code or specific blocked status.

### 8. Docs/current-state update

Update docs consistently:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md` if implementation deviates from the prepared spec.

Expected final docs position:

- accepted through Goal 029, assuming the local repo/user handoff confirms it;
- Goal 030 produced for review;
- active/current gate is `semantic_artifact_contract_registry_verification required`;
- Goal 031 remains future, not started.

Do not mark Goal 030 accepted/passed.

## Tests

Add focused tests in the existing test style. Suggested test classes, adjust names to local conventions:

```text
SemanticArtifactContractRegistryTests
SemanticArtifactContractValidatorTests
SemanticArtifactCompatibilityPlannerTests
SemanticExpansionPlanProductSmokeTests
Goal030SemanticArtifactContractRegistryEvidenceTests
```

Tests must prove:

- registry seed validates cleanly;
- ordering is deterministic;
- dependencies are resolved in stable topological order;
- cycle/unknown dependency diagnostics work;
- semantic pack matching works by profile/family/scope/tag;
- three scenario plans are meaningfully different but use the same planner;
- evidence artifacts are written and directly inspectable;
- invalid/fake/leak matrix produces causal diagnostics;
- no public package/runtime/UI/provider/Lua behavior is required.

Do not weaken existing tests.

## Validation commands

Run focused checks first, then full gate only at the end.

Use PowerShell from `C:\Users\endim\LLMGameCreator\`:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~SemanticArtifactContractRegistry|FullyQualifiedName~SemanticArtifactContractValidator|FullyQualifiedName~SemanticArtifactCompatibility|FullyQualifiedName~SemanticExpansionPlan|FullyQualifiedName~Goal030"

# Product smoke / evidence-focused check. If the filter syntax does not match local names, run the exact new test class filters separately.
dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~ProductSmoke|FullyQualifiedName~Goal030"

# Direct artifact inspection after smoke.
Get-ChildItem .\.llmgc\procedural\goal-030-semantic-artifact-contract-registry -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-030-semantic-artifact-contract-registry\semantic-artifact-contract-registry-report.md -TotalCount 80

# Full final gate at the end only.
.\.devflow\scripts\check-all.ps1
```

If `check-all.ps1` is already known to take longer, still run it at the end. Do not run it repeatedly for every small change.

## Stop conditions

Stop and report without commit/push if any of these happens:

1. You need to change public `GamePackage` schema/model to complete the goal.
2. You need to touch WinForms/UI, Runtime, Unity, provider/LLM/RAG, Lua, `generator-library`, `.sln`, or `.csproj`.
3. You need an external dependency to complete the first version.
4. Existing docs/evidence contradict the user handoff so strongly that starting Goal 030 would mean inventing acceptance.
5. Focused tests fail and cannot be fixed within the allowed scope.
6. `check-all.ps1` fails.
7. A failure appears unrelated and predates your changes; do not hide it. Report exact failing test/command/output summary and changed files.
8. Evidence artifacts contain absolute paths, timestamps without deterministic convention, heavy logs, or nondeterministic ordering.
9. The implementation becomes a giant monolithic class instead of small registry/validator/planner/evidence components.

## Git policy

Allowed git commands before final green checks:

```text
git status
git branch --show-current
git diff --stat
git diff -- <changed-files>
```

After all validation commands are green and the final report is clean, you may commit and push to `origin/main`.

Allowed final git commands only after green validation:

```text
git add <changed-files>
git commit -m "Goal 030 semantic artifact contract registry"
git push origin main
```

Forbidden always unless the user explicitly gives a separate instruction:

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

Report in Russian with these sections:

```text
Goal 030 выполнен / остановлен

Gate:
semantic_artifact_contract_registry_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<кратко по registry / validator / semantic compatibility / expansion slots / evidence>

Evidence artifacts:
<список файлов под .llmgc/procedural/goal-030-semantic-artifact-contract-registry/>

Проверки:
<команды и результаты>

Invalid/fake/leak matrix:
<какие негативные кейсы покрыты>

Документы состояния:
<что обновлено в CURRENT_GENERATOR_STATE.*, CONTEXT_INDEX, FULL_GENERATOR_GOAL_QUEUE>

Git:
<commit hash and push result, or explicit no-commit/no-push reason>

Ограничения / что не делалось:
<GamePackage schema, UI, Runtime, Unity, provider/LLM/RAG, Lua, generator-library, external deps not touched>

Следующий разумный шаг:
<Goal 031 or alternative, one concise paragraph>
```
