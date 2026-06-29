# Codex task — GOAL 031 Semantic Pack Composition Blueprint

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
goal-031-semantic-pack-composition-blueprint-v1
Goal 031: Semantic Pack Composition Blueprint
```

Required goal marker / gate marker:

```text
semantic_pack_composition_blueprint_verification
```

Codex reasoning level:

```text
very high
```

## Launcher prompt expected from user

The user may start this task with:

```text
/goal Выполни composite goal из файла docs/agent-tasks/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT.md в рабочей копии C:\Users\endim\LLMGameCreator\ на ветке main. Считай файл задачи авторитетным источником требований: read-first list, allowed/forbidden scope, exact behavior, tests, validation commands, stop conditions, final report и git policy. Не делай branch/merge/rebase/cherry-pick. Commit/push разрешены только если в файле задачи это разрешено и только после зелёного final gate.
```

## Starting gate assumption

This task is issued after Goal 030 was implemented, cleaned up, checked with `check-all.ps1`, and pushed to `origin/main`.

User handoff confirms:

```text
Goal 030 implemented.
Goal 030 cleanup completed.
check-all.ps1 passed: 924/924.
main synchronized with origin/main.
Proceed further.
```

If local state docs still show:

```text
semantic_artifact_contract_registry_verification required
```

then treat the user's explicit "proceed further" handoff as the authorization to move beyond Goal 030. Update docs so that Goal 030 is recorded as passed/accepted according to the repository's existing wording conventions, then set Goal 031 as the current required gate.

Do not alter Goal 030 code/evidence except if local current-state documentation has to reference it.

Final state after this task must be:

```text
semantic_pack_composition_blueprint_verification required
```

Do not mark Goal 031 passed inside this task.

## Purpose

Goal 030 gave the project a semantic artifact contract registry, validator, compatibility planner, semantic expansion slots, and evidence writer.

Goal 031 must use that layer to introduce the next generator-generalization level:

- deterministic semantic pack composition;
- semantic fact/relation merging;
- cross-artifact blueprint planning;
- profile-specific but planner-shared evidence;
- negative/conflict/fake/leak tests.

This is not a feature-specific NPC/quest/biome module. It is the layer that lets all future feature modules receive one coherent semantic blueprint instead of inventing isolated vertical rules.

The answer to “What became more real?” should be:

```text
Selected semantic packs can now be composed into a deterministic cross-artifact generation blueprint that links world, biome, faction, NPC, quest, dialogue, economy, combat, settlement and event intent before GamePackage materialization.
```

## Read-first list

Read these first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT.md`
8. Goal 030 implementation/evidence:
   - `src/LLMGameCreator.Application/Design/SemanticArtifactContracts/**`
   - `tests/LLMGameCreator.Tests/Application/SemanticArtifactContracts/**`
   - `.llmgc/procedural/goal-030-semantic-artifact-contract-registry/**`
9. Existing semantic catalog/profile/capability docs and tests, if present:
   - `src/LLMGameCreator.Application/Design/SemanticCatalog/**`
   - `tests/LLMGameCreator.Tests/Application/SemanticCatalog/**`
   - `docs/SEMANTIC_PACK_AND_RAG_STRATEGY.md` if present
   - `docs/GAME_SYSTEM_VARIANT_TAXONOMY.md` if present
   - `docs/GAME_BLUEPRINT_CAPABILITY_GRAPH_SPEC.md` if present
   - `docs/CAPABILITY_BUNDLE_PIPELINE_INPUTS_CONTRACT_V1.md` if present
10. Existing product smoke/evidence tests that encode current goal/gate conventions.

Do not read the whole repository unless targeted search shows relevant code elsewhere.

## Allowed files / areas

You may create or edit only these areas:

```text
docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/SemanticPackComposition/**
src/LLMGameCreator.Application/Design/SemanticArtifactContracts/**
tests/LLMGameCreator.Tests/Application/SemanticPackComposition/**
tests/LLMGameCreator.Tests/Application/SemanticArtifactContracts/**
tests/LLMGameCreator.Tests/ProductSmoke/*SemanticPackComposition*
tests/LLMGameCreator.Tests/ProductSmoke/*SemanticExpansion*
tests/LLMGameCreator.Tests/** current-state/gate/evidence tests that fail only because the active gate moves from Goal 030 to Goal 031
.llmgc/procedural/goal-031-semantic-pack-composition-blueprint/**
```

Use existing local naming/style conventions. If the repository already has a better folder for semantic pack composition, use it, but stay inside `src/LLMGameCreator.Application/Design/**` and matching tests.

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
public GamePackage schema/model changes
```

Also forbidden:

- adding external NuGet dependencies;
- weakening or deleting existing tests/evidence tests;
- calling LLM/provider/RAG;
- executing Lua;
- generating images/audio/video;
- changing Runtime/Unity behavior;
- broad refactors unrelated to this goal;
- branch, merge, rebase, cherry-pick, reset, stash, clean, force-push.

## Exact behavior to implement

### 1. Preflight and local pattern discovery

- Confirm branch `main`.
- Check local status.
- Inspect Goal 030 implementation and tests.
- Reuse Goal 030 models/services where sensible, but do not turn them into a giant monolith.
- Prefer small classes:
  - catalog/seed provider;
  - validator;
  - composer/planner;
  - evidence writer;
  - diagnostics/model records.

### 2. Goal 030 gate transition

If current docs still say `semantic_artifact_contract_registry_verification required`, update them according to existing repository convention so that Goal 030 is recorded as passed/accepted and Goal 031 becomes current.

Do not change Goal 030 evidence contents.

The final active/manual gate must be:

```text
semantic_pack_composition_blueprint_verification required
```

### 3. Semantic pack composition model

Implement a BCL-only composition model. Exact class names may follow local style, but the model must represent these meanings:

- semantic pack id;
- family/profile support ids;
- provided semantic scopes;
- theme tags;
- semantic facts;
- relation hints;
- exclusions/conflicts;
- expansion intents;
- priority/order key;
- optional/future flags;
- source notes/status text;
- deterministic diagnostics.

Semantic facts should be structured records, not only prose. Minimum fact domains:

```text
world_region
route_pressure
biome_hazard
weather_event
faction_role
reputation_axis
npc_archetype
social_relation
quest_motive
quest_objective
dialogue_tone
localization_hint
economy_chain
resource_theme
recipe_theme
loot_theme
combat_pressure
progression_axis
settlement_pattern
landmark_theme
global_event
```

### 4. Semantic pack catalog seed

Create a deterministic seed catalog with enough packs to prove real composition.

Minimum base packs/profile families:

```text
frontier_survival
gothic_intrigue
caravan_trade
```

Minimum optional/mix-in packs:

```text
ruins_and_relics
winter_hazards
merchant_guilds
border_conflict
folk_magic
scarcity_economy
```

These names can be adapted to existing repository naming conventions, but the coverage intent must remain.

The seed catalog must provide overlapping facts and deliberate non-breaking tensions so the composer proves it can merge multiple packs.

### 5. Validator

Implement validation diagnostics with stable machine-readable codes.

Required checks:

- duplicate pack ids;
- invalid/empty ids;
- unsupported/unknown profile/family reference;
- missing semantic scopes;
- duplicate fact ids;
- invalid fact domain;
- relation references unknown facts;
- expansion intent references unknown fact or unknown Goal 030 contract/artifact kind;
- incompatible exclusions/conflicts;
- cyclic relation implication if implemented as directed implications;
- future-only pack accidentally treated as ready;
- forbidden leakage tags/notes that imply Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage schema changes as part of Goal 031.

Validation failures must return diagnostics, not ordinary exceptions.

### 6. Composer / blueprint planner

Implement a deterministic composer that accepts:

- profile/family id;
- selected pack ids;
- optional complexity/scale hint;
- Goal 030 registry/compatibility planner where appropriate.

It must return a semantic blueprint plan containing at least:

- selected pack ids;
- rejected/incompatible pack ids with reasons;
- merged semantic facts;
- relation graph or relation list;
- resolved expansion intents;
- Goal 030 contract coverage ids where applicable;
- cross-artifact links;
- missing/future-required/blocked diagnostics;
- stable summary.

Minimum blueprint sections:

1. world regions / route pressure;
2. biome/weather/hazard/event pressure;
3. factions and reputation/social relation anchors;
4. NPC archetype variation anchors;
5. quest motive/objective/reward pattern anchors;
6. dialogue tone/localization/string-table hints;
7. economy/resource/recipe/loot chains;
8. combat/progression/ability pressures;
9. settlement/building/landmark anchors;
10. global events;
11. coverage gaps and future-required items.

The cross-artifact links are important. Each smoke scenario must show multiple links like:

- faction -> NPC archetype -> quest motive -> dialogue tone;
- biome hazard -> resource scarcity -> economy chain -> loot theme;
- settlement pattern -> landmark -> route pressure -> global event;
- combat pressure -> progression axis -> reward pattern.

### 7. Product smoke route and evidence writer

Add a focused product-smoke route that composes three scenarios through the same composer.

Write deterministic evidence under:

```text
.llmgc/procedural/goal-031-semantic-pack-composition-blueprint/
```

Required files:

```text
pack-catalog-summary.json
composition-matrix.json
semantic-blueprint-plan-frontier.json
semantic-blueprint-plan-gothic.json
semantic-blueprint-plan-caravan.json
cross-artifact-linkage-report.json
semantic-pack-composition-blueprint-report.md
```

Evidence requirements:

- no wall-clock timestamps unless existing repo convention has deterministic fixed timestamps;
- no absolute machine paths;
- no heavy logs/build output;
- stable ordering;
- compact JSON;
- markdown report must contain the exact marker:
  `semantic_pack_composition_blueprint_verification required`.

### 8. Invalid/fake/leak matrix

Tests must include causal negative cases:

- duplicate pack id mutation;
- unknown profile/family mutation;
- missing semantic scope mutation;
- duplicate fact id mutation;
- unknown fact relation mutation;
- expansion intent references fake Goal 030 contract/artifact kind;
- incompatible pack selection mutation;
- future-only pack treated as ready;
- fake selected pack id accepted by composer;
- leakage attempt: Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage-schema tag or note treated as allowed.

Each mutation must produce a specific diagnostic code or blocked status.

### 9. Docs/current-state update

Update docs consistently:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md` if implementation deviates from the prepared spec.

Expected final docs position:

- accepted through Goal 030;
- Goal 031 produced for review;
- active/current gate is `semantic_pack_composition_blueprint_verification required`;
- Goal 032 remains future, not started.

Do not mark Goal 031 accepted/passed.

## Tests

Add focused tests in the existing test style. Suggested class names, adapt to local conventions:

```text
SemanticPackCompositionCatalogTests
SemanticPackCompositionValidatorTests
SemanticPackCompositionPlannerTests
SemanticPackCompositionEvidenceTests
SemanticPackCompositionProductSmokeTests
Goal031SemanticPackCompositionBlueprintEvidenceTests
```

Tests must prove:

- seed pack catalog validates cleanly;
- composer ordering is deterministic;
- three scenarios are meaningfully different;
- three scenarios use the same composer and Goal 030 registry/planner integration path where applicable;
- cross-artifact links exist and are stable;
- evidence artifacts are written and directly inspectable;
- invalid/fake/leak matrix produces causal diagnostics;
- no public GamePackage/runtime/UI/provider/LLM/RAG/Lua/Unity behavior is required.

Do not weaken existing tests.

## Validation commands

Run focused checks first, then full gate only at the end.

Use PowerShell from `C:\Users\endim\LLMGameCreator\`:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~SemanticPackComposition|FullyQualifiedName~Goal031"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~ProductSmoke|FullyQualifiedName~Goal031"

Get-ChildItem .\.llmgc\procedural\goal-031-semantic-pack-composition-blueprint -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-031-semantic-pack-composition-blueprint\semantic-pack-composition-blueprint-report.md -TotalCount 100

.\.devflow\scripts\check-all.ps1
```

If a filter does not match local class names, run equivalent focused class filters and report the exact commands used.

Do not run `check-all.ps1` repeatedly for every small edit. Run it at the final gate.

## Stop conditions

Stop and report without commit/push if any of these happens:

1. You need to change public `GamePackage` schema/model.
2. You need to touch WinForms/UI, Runtime, Unity, provider/LLM/RAG, Lua, `generator-library`, `.sln`, or `.csproj`.
3. You need an external dependency.
4. Goal 030 implementation/evidence is missing or local docs contradict the user handoff so strongly that moving on would mean inventing history.
5. Focused tests fail and cannot be fixed within the allowed scope.
6. `check-all.ps1` fails.
7. A failure appears unrelated and predates your changes; do not hide it. Report exact failing command/output summary and changed files.
8. Evidence artifacts contain absolute paths, nondeterministic timestamps, heavy logs, or unstable ordering.
9. The implementation becomes a giant monolithic class instead of small catalog/validator/composer/evidence components.

## Git policy

Allowed git commands before final green checks:

```text
git status
git status -sb
git branch --show-current
git diff --stat
git diff -- <changed-files>
git log --oneline -5
```

After all validation commands are green and the final report is clean, you may commit and push to `origin/main`.

Allowed final git commands only after green validation:

```text
git add <changed-files>
git commit -m "Goal 031 semantic pack composition blueprint"
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
Goal 031 выполнен / остановлен

Gate:
semantic_pack_composition_blueprint_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<кратко по catalog / validator / composer / blueprint sections / cross-artifact links / evidence>

Evidence artifacts:
<список файлов под .llmgc/procedural/goal-031-semantic-pack-composition-blueprint/>

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
<Goal 032 or alternative, one concise paragraph>
```
