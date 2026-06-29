# Codex task — GOAL 032 Dynamic Semantic Feature System And Influence Rule Kernel

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
goal-032-dynamic-semantic-feature-system-v1
Goal 032: Dynamic Semantic Feature System And Influence Rule Kernel
```

Required gate marker:

```text
dynamic_semantic_feature_system_verification
```

Codex reasoning level:

```text
very high
```

## Goal launcher context

This task starts after Goal 031 technical completion on `main`.

The user wants LLMGameCreator to avoid using LLM as the combinatorial generator for NPC mood, faction relation, quest motive, dialogue acts, species/archetype variation, etc.

The intended architecture is:

```text
LLM may help with lore intake and seed-pack drafting.
The program owns semantic features, inheritance, influence rules, validation, authoring hints and deterministic resolution.
```

Goal 032 must create that kernel.

Final state must be:

```text
dynamic_semantic_feature_system_verification required
```

Do not mark the gate passed.

## Read-first list

Read these first:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM.md`
8. `docs/GOAL_030_SEMANTIC_ARTIFACT_CONTRACT_REGISTRY_SPEC.md`
9. `docs/GOAL_031_SEMANTIC_PACK_COMPOSITION_BLUEPRINT_SPEC.md`
10. Existing code/tests under:
    - `src/LLMGameCreator.Application/Design/SemanticArtifactContracts/`
    - `src/LLMGameCreator.Application/Design/SemanticPackComposition/`
    - `tests/LLMGameCreator.Tests/Application/SemanticArtifactContracts/`
    - `tests/LLMGameCreator.Tests/Application/SemanticPackComposition/`
    - `tests/LLMGameCreator.Tests/ProductSmoke/`
11. Existing current-state/gate tests. Search narrowly for current active marker and Goal 031 gate assertion tests.

Do not scan the whole repository unless local search shows relevant files elsewhere.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/DynamicSemanticFeatures/**
tests/LLMGameCreator.Tests/Application/DynamicSemanticFeatures/**
tests/LLMGameCreator.Tests/ProductSmoke/DynamicSemanticFeatureSystemProductSmokeTests.cs
.llmgc/procedural/goal-032-dynamic-semantic-feature-system/**
```

If the repository style strongly prefers another subfolder name for this exact concept, use the local style but keep the scope equivalent and report it.

You may update narrow current-state/gate tests only if they already assert the active gate and must move from Goal 031 to Goal 032. Keep such edits minimal and explicit.

## Forbidden files / areas

Do not modify:

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
public GamePackage schema/model files
```

Also forbidden:

- external NuGet dependencies;
- provider/LLM/RAG calls;
- Lua execution;
- media generation;
- broad refactors;
- weakening/deleting acceptance or evidence tests;
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior to implement

### 1. Preflight

From `C:\Users\endim\LLMGameCreator\`:

- confirm branch `main`;
- inspect status;
- confirm Goal 031 is the current produced-for-review gate;
- identify local patterns used by Goal 030/031 for models, validator, planner/composer, evidence writer, product smoke and tests.

If local state contradicts the user handoff so strongly that Goal 032 cannot be started honestly, stop and report.

### 2. Dynamic semantic feature model

Implement BCL-only model types under the allowed Application folder.

At minimum support:

Feature definitions:

- stable feature id;
- display name;
- target scope;
- value kind;
- cardinality;
- required/optional mode;
- default strategy;
- inheritance mode;
- allowed values or bounds;
- applicability conditions;
- tags;
- conflicts/requires;
- authoring group;
- provenance;
- status/notes.

Feature assignments:

- target id;
- target scope;
- feature id;
- typed value;
- source layer;
- source id;
- override mode;
- weight/priority where relevant;
- provenance;
- status.

Semantic scopes must include at least:

```text
world
kingdom
region
biome
settlement
faction
species
archetype
npc
item
resource
quest
dialogue
event
magic
combat
relationship
```

Value kinds must include at least:

```text
flag
number
enum
weighted_tag
relation
text_key
list
```

Do not implement arbitrary expression-string evaluation.

### 3. Applicability and absence

Implement applicability checks so that features are not globally forced.

Required behavior:

- NPC without faction can be valid.
- NPC without mood can be valid if mood feature is optional/inapplicable.
- Some species/archetypes can add extra feature families.
- Illegal feature assignment to an incompatible scope is diagnostic, not silent.
- Missing required feature is diagnostic.
- Missing optional/inapplicable feature is not an error but should be traceable.

### 4. Inheritance kernel

Implement deterministic feature inheritance.

Supported chain examples:

```text
world -> kingdom -> region -> biome -> settlement
world -> kingdom -> faction -> npc
world -> species -> archetype -> npc
world -> kingdom -> species -> archetype -> npc
```

Do not hardcode NPC-only behavior. The model should be generic enough to work for species, factions, quests and settlements.

Resolver output must trace:

- inherited values;
- overrides;
- defaults;
- absent optional values;
- blocked illegal values.

### 5. Influence rules

Implement typed influence rules, not code strings.

Minimum condition operators:

```text
feature_exists
feature_missing
enum_equals
number_at_least
number_at_most
tag_contains
relation_exists
scope_is
target_has_tag
```

Minimum effects:

```text
set_feature
adjust_number
add_weighted_tag
add_relation
add_intent
block_feature
raise_diagnostic
suggest_feature
```

Rules must include:

- stable id;
- target scope/family;
- condition clauses;
- effect records;
- weight;
- priority;
- deterministic tie-breaker;
- status;
- provenance;
- explanation.

Influence output must be deterministic and traceable.

### 6. Resolver

Implement a deterministic resolver that accepts:

- feature definitions;
- feature assignments;
- influence rules;
- hierarchy/context;
- profile/style id;
- seed.

It returns resolved semantic state containing:

- target id/scope;
- final feature values;
- inheritance/default/manual/generated flags;
- influence effects;
- authoring suggestions;
- diagnostics;
- stable summary.

The same input twice must produce structurally equivalent output.

### 7. Dynamic authoring schema records

Do not touch UI.

Create UI-ready planning records for future dynamic tabs/UserControls:

- feature group;
- field kind;
- label/key;
- option list or numeric bounds;
- required/optional/applicable status;
- inherited value;
- can override;
- suggested default;
- diagnostic links;
- safe editor hints.

This is only an Application-layer data contract/evidence path.

### 8. Seed catalog / proof scenarios

Create an in-memory deterministic seed catalog with at least four scenarios.

Required scenarios:

```text
frontier_survival
gothic_intrigue
caravan_trade
metamodule_kingdoms
```

The `metamodule_kingdoms` scenario must prove the system is not only a simple blacksmith/NPC demo. It should include:

- multiple kingdoms/regions or at least a compact representation of that;
- a fantasy species/archetype such as `metamodule_bearer`;
- species/archetype-specific feature families;
- optional faction/mood behavior;
- extra factors such as module capacity, mana resonance, kingdom pressure, forbidden affinity or equivalent;
- deterministic resolution with diagnostics clean for the valid scenario.

### 9. Evidence writer

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-032-dynamic-semantic-feature-system/
```

Required files:

```text
feature-catalog-summary.json
influence-rule-summary.json
dynamic-authoring-schema-matrix.json
resolved-feature-state-frontier.json
resolved-feature-state-gothic.json
resolved-feature-state-caravan.json
resolved-feature-state-metamodule-kingdoms.json
invalid-feature-diagnostics-matrix.json
dynamic-semantic-feature-system-report.md
```

Evidence rules:

- stable ordering;
- no timestamps unless repository has deterministic timestamp convention;
- no absolute paths;
- no heavy logs/build outputs;
- compact JSON;
- no LLM/provider/RAG claims;
- report must contain exact marker:

```text
dynamic_semantic_feature_system_verification required
```

### 10. Validator and invalid/fake/leak matrix

Implement validator diagnostics with stable codes.

Must cover:

- duplicate feature id;
- invalid/empty id;
- unknown feature reference;
- unknown target scope;
- invalid value kind/value shape;
- illegal assignment for target scope;
- required feature missing;
- optional feature missing is not an error;
- feature conflict;
- unknown inheritance source;
- circular inheritance;
- unknown influence target;
- circular influence or repeated self-feeding effect where applicable;
- overconstrained output;
- fake selected feature id;
- forbidden leakage terms/claims involving Runtime/UI/Unity/provider/LLM/RAG/Lua/GamePackage schema.

Diagnostics should be causal and machine-readable.

### 11. Docs/current-state update

Update docs consistently:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Goal 032 spec/scouting docs if implementation deviates.

Expected final docs state:

- accepted through Goal 030;
- Goal 031 produced for manual review;
- Goal 032 produced for manual review if the repo process permits starting it from the user handoff;
- active/current gate after this implementation:

```text
dynamic_semantic_feature_system_verification required
```

Do not mark Goal 032 passed.

If the local docs require explicitly recording the user's acceptance of Goal 031 before starting Goal 032, record only what the user provided: Goal 031 technical completion and manual-gate status. Do not invent a passed gate unless repository state or user handoff explicitly says it is accepted.

## Tests

Add focused tests in local style. Suggested classes:

```text
DynamicSemanticFeatureCatalogTests
DynamicSemanticFeatureValidatorTests
DynamicSemanticFeatureResolverTests
DynamicSemanticFeatureAuthoringSchemaTests
DynamicSemanticFeatureEvidenceTests
DynamicSemanticFeatureSystemProductSmokeTests
```

Tests must prove:

- valid seed catalog validates cleanly;
- same input resolves deterministically;
- feature absence can be valid when optional/inapplicable;
- illegal assignment is diagnostic;
- inheritance order and overrides work;
- influence rules produce traced deterministic outputs;
- dynamic authoring schema exposes field groups/options/applicability;
- four scenarios produce meaningfully different outputs through the same resolver;
- `metamodule_kingdoms` exercises fantasy species/archetype-specific feature families;
- invalid/fake/leak matrix produces causal diagnostics;
- evidence artifacts are written and directly inspectable;
- no GamePackage/Runtime/UI/Unity/provider/LLM/RAG/Lua dependency is required.

Do not weaken existing tests.

## Validation commands

Run focused checks first. Run full gate once near the end.

PowerShell from `C:\Users\endim\LLMGameCreator\`:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~DynamicSemanticFeature"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~ProductSmoke|FullyQualifiedName~Goal032|FullyQualifiedName~DynamicSemanticFeature"

Get-ChildItem .\.llmgc\procedural\goal-032-dynamic-semantic-feature-system -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-032-dynamic-semantic-feature-system\dynamic-semantic-feature-system-report.md -TotalCount 120

.\.devflow\scripts\check-all.ps1
```

If the filter syntax does not match local test names, run the exact new test class filters separately and report commands used.

## Historical artifact cleanup policy

`check-all.ps1` may regenerate or mutate historical tracked `.llmgc/procedural/**` artifacts outside this goal.

You may use this command only for exact accidental tracked historical artifact paths outside Goal 032 scope:

```powershell
git restore --source=HEAD -- <exact accidental historical artifact paths>
```

Rules:

- Before restore, list exact paths.
- Do not restore Goal 032 code/docs/tests/evidence.
- Do not restore current-state docs unless you intentionally decide the edit is accidental and explain why.
- Do not use `git checkout`, `git reset`, `git clean`, or `git stash`.
- If `check-all.ps1` fails because ignored local Unity logs are missing, do not fabricate logs. Stop and report exact missing paths. Copying logs from cache requires separate user permission unless a previous repository-local policy explicitly permits it.

## Stop conditions

Stop and report without commit/push if any of these happens:

1. You need to change public `GamePackage` schema/model.
2. You need to touch WinForms/UI, Runtime, Unity, provider/LLM/RAG, Lua, generator-library, `.sln`, `.csproj`.
3. You need an external dependency.
4. Existing docs/evidence contradict the user handoff so strongly that starting Goal 032 would invent acceptance.
5. Focused tests fail and cannot be fixed within allowed scope.
6. `check-all.ps1` fails.
7. Evidence contains absolute paths, nondeterministic timestamps, heavy logs, or non-compact build output.
8. Implementation becomes a monolithic class instead of model/validator/resolver/evidence components.
9. The resolver relies on LLM/provider calls or expression-string execution.

## Git policy

Allowed before final green checks:

```text
git status
git status -sb
git branch --show-current
git diff --stat
git diff -- <changed-files>
git ls-tree -r HEAD -- <path>
```

Allowed final commands only after green validation and clean final report:

```text
git add <changed-files>
git commit -m "Goal 032 dynamic semantic feature system"
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
Goal 032 выполнен / остановлен

Gate:
dynamic_semantic_feature_system_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<feature model / applicability / inheritance / influence rules / resolver / authoring schema / evidence>

Evidence artifacts:
<список файлов под .llmgc/procedural/goal-032-dynamic-semantic-feature-system/>

Сценарии:
<frontier / gothic / caravan / metamodule kingdoms summary>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<covered negative cases>

Historical artifact cleanup:
<none or exact restored paths>

Git:
<commit hash and push result, or explicit no-commit/no-push reason>

Ограничения / что не делалось:
<GamePackage schema, UI, Runtime, Unity, provider/LLM/RAG, Lua, generator-library, external deps not touched>

Следующий разумный шаг:
<Goal 033 or repair note, one concise paragraph>
```
