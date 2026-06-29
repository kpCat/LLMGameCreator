# Codex task — Goal 033 Semantic Authoring Workspace And Feature-Driven Intent Resolver

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
goal-033-semantic-authoring-intent-resolver-v1
Goal 033: Semantic Authoring Workspace And Feature-Driven Intent Resolver
```

Required final gate marker:

```text
semantic_authoring_intent_resolver_verification required
```

Codex reasoning level:

```text
very high
```

## Starting state

Goal 032 was pushed to `origin/main` as commit `3ca0d768` according to user handoff. Current docs may still keep Goal 031 and Goal 032 at manual `required` gates. Do not mark previous gates as passed unless the repository state already says they were accepted. It is acceptable for Goal 033 to proceed from the user handoff while preserving those prior gates as produced-for-review.

Goal 033 must not become another report-only layer. It must make a practical authoring and content-intent planning capability real.

## Preflight

From `C:\Users\endim\LLMGameCreator\`:

1. Confirm branch `main`.
2. Confirm upstream status.
3. Run `git status --short --untracked-files=all` and `git diff --stat`.
4. If only old untracked task/launcher files such as `docs/agent-tasks/GOAL_032_*` are present, do not stage, edit or delete them. Report them and continue only if they are clearly unrelated to Goal 033.
5. If tracked files have unexpected changes outside this task, stop.
6. If the historical `boot.config` path appears status-only modified but `git diff --exit-code -- <path>` is clean, run `git update-index -q --refresh` and re-check status. Do not restore/reset it unless separately authorized.

## Read-first list

Read these first, in order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER.md`
8. `docs/GOAL_032_DYNAMIC_SEMANTIC_FEATURE_SYSTEM_SPEC.md`
9. `src/LLMGameCreator.Application/Design/DynamicSemanticFeatures/**`
10. `tests/LLMGameCreator.Tests/Application/DynamicSemanticFeatures/**`
11. `.llmgc/procedural/goal-032-dynamic-semantic-feature-system/dynamic-semantic-feature-system-report.md`
12. `.llmgc/procedural/goal-032-dynamic-semantic-feature-system/feature-catalog-summary.json`
13. `.llmgc/procedural/goal-032-dynamic-semantic-feature-system/dynamic-authoring-schema-matrix.json`
14. `src/LLMGameCreator.Application/Design/SemanticPackComposition/**`
15. `src/LLMGameCreator.Application/Design/SemanticArtifactContracts/**`
16. Existing product smoke patterns under `tests/LLMGameCreator.Tests/ProductSmoke/` relevant to Goal 030-032.
17. Existing current-state/gate assertion tests that were updated for Goal 032.

Do not read the whole repository unless a narrow search shows required code lives elsewhere.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_033_SEMANTIC_AUTHORING_INTENT_RESOLVER.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
src/LLMGameCreator.Application/Design/SemanticAuthoringIntentResolver/**
tests/LLMGameCreator.Tests/Application/SemanticAuthoringIntentResolver/**
tests/LLMGameCreator.Tests/ProductSmoke/SemanticAuthoringIntentResolverProductSmokeTests.cs
.llmgc/procedural/goal-033-semantic-authoring-intent-resolver/**
```

If repository naming strongly suggests a better `Design/*` subfolder name, keep it close to `SemanticAuthoringIntentResolver` and do not create broad parallel architecture.

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
- runtime LLM/provider/RAG calls;
- final dialogue/prose generation as the expected output;
- Lua execution;
- media generation;
- Unity build/runtime changes;
- WinForms/UI changes;
- broad refactors;
- weakening existing tests/evidence;
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Reuse Goal 030-032 seams

Implement Goal 033 as a consumer/planning layer over the existing semantic stack:

- Goal 030 artifact contract registry / compatibility planner;
- Goal 031 semantic pack composition blueprint;
- Goal 032 dynamic semantic features / applicability / inheritance / influence resolver / authoring schema records.

Do not duplicate those systems. Use them where feasible. If direct reuse is awkward, introduce a narrow adapter/service in the new allowed folder that consumes their public/Application-layer models.

### 2. Authoring workspace model

Implement deterministic BCL-only records and services for dynamic authoring workspaces.

The workspace must represent:

- scenario/profile id;
- domain groups: world, kingdom, region, species, archetype, faction, NPC, quest, dialogue, economy, combat, settlement, event;
- dynamic sections;
- fields with stable ids;
- feature references;
- value kind;
- required/optional/repeatable status;
- applicability/inheritance hints;
- control hints for future UI, without touching UI;
- completion status;
- provenance classification: `user`, `programmatic`, `inherited`, `semantic_pack`, `llm_candidate`, `imported_candidate`, `unset`, `blocked`;
- diagnostics.

The model must allow legal absence. Examples: NPC without faction, NPC without mood, species without profession, faction-less creature archetypes.

### 3. Lore intake skeleton

Implement a deterministic lore intake skeleton service/model.

It must support:

- high-level lore brief id;
- style/profile id;
- world themes;
- kingdom/region/faction/species/archetype slots;
- magic/system axes;
- conflict axes;
- manual-fill slots;
- programmatically inferable slots;
- LLM-candidate slots that are quarantined and not accepted automatically.

For `metamodule_kingdoms`, create a compact high-complexity skeleton with:

- 6 or 7 kingdoms;
- at least 100 species/archetype slots or compact generated slot records;
- feature families for module carriers, mana resonance, forbidden affinities, kingdom pressure, faction relation, dialogue intent, quest motive, event intent, economy pressure and combat pressure;
- deterministic counts and representative samples in evidence.

### 4. Feature-driven content intent resolver

Implement a resolver that uses Goal 032 resolved feature states and produces content intent records.

Intent families must include at least:

- NPC role intent;
- relationship pressure;
- faction reaction;
- quest motive;
- dialogue act;
- event intent;
- economy pressure;
- combat pressure;
- settlement need;
- lore gap / authoring gap.

Intent records must include:

- stable intent id;
- target id/domain;
- source feature ids;
- resolved feature value summary;
- deterministic priority/weight;
- template hint or localization key hint where relevant;
- blockers/gaps;
- provenance summary;
- trace summary.

No final dialogue lines. No final quest text. No final GamePackage definitions. This is intent-level planning only.

### 5. Manual vs auto authoring matrix

Produce a matrix that proves the system can distinguish:

- explicitly user-set values;
- programmatic defaults;
- inherited values;
- semantic-pack-derived values;
- optional absent values;
- required missing values;
- LLM candidates requiring review;
- imported candidates requiring review;
- blocked/invalid values.

### 6. Evidence writer

Write deterministic compact evidence under:

```text
.llmgc/procedural/goal-033-semantic-authoring-intent-resolver/
```

Required files:

```text
authoring-workspace-schema-summary.json
lore-intake-skeleton-metamodule-kingdoms.json
manual-vs-auto-authoring-matrix.json
intent-resolution-frontier.json
intent-resolution-gothic.json
intent-resolution-caravan.json
intent-resolution-metamodule-kingdoms.json
invalid-authoring-intent-diagnostics-matrix.json
semantic-authoring-intent-resolver-report.md
```

Evidence requirements:

- deterministic ordering;
- no absolute paths;
- no nondeterministic timestamps;
- compact JSON;
- hashes if local style supports them;
- report contains `semantic_authoring_intent_resolver_verification required`;
- report states that final dialogue/prose/GamePackage/runtime/UI/Unity/provider/LLM/RAG/Lua/media generation was not performed.

### 7. Invalid/fake/leak matrix

Add causal negative coverage for at least:

- duplicate workspace field id;
- unknown feature reference;
- unknown target/domain;
- illegal feature/domain applicability;
- required manual field missing;
- optional absent field valid and traceable;
- conflicting provenance for same field;
- LLM candidate accidentally treated as accepted;
- imported candidate accidentally treated as accepted;
- final dialogue/prose leakage;
- final GamePackage materialization leakage;
- runtime/UI/Unity/provider/LLM/RAG/Lua/media boundary leakage;
- fake intent target accepted;
- missing source feature trace;
- nondeterministic ordering mutation.

Each invalid case must produce stable diagnostic codes or explicit blocked status.

### 8. Docs/current-state update

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- Goal 033 spec/scouting docs if implementation deviates from the prepared plan.

Preserve honesty about prior gates. If Goal 031/032 are still `required` in repo state, do not mark them passed. Record that Goal 033 was started by user handoff after Goal 032 technical completion.

Final state for this goal:

```text
semantic_authoring_intent_resolver_verification required
```

Do not mark it passed.

## Tests

Add focused tests in repository style. Suggested test classes:

```text
SemanticAuthoringWorkspaceTests
LoreIntakeSkeletonTests
FeatureDrivenIntentResolverTests
SemanticAuthoringIntentEvidenceTests
SemanticAuthoringIntentValidatorTests
SemanticAuthoringIntentResolverProductSmokeTests
```

Tests must prove:

- workspace schema is deterministic;
- legal absence works;
- manual/programmatic/inherited/semantic-pack/LLM/imported provenance is separated;
- LLM candidates are quarantined and never accepted automatically;
- intent resolver produces distinct but structurally comparable scenarios;
- metamodule scenario has high-complexity slot coverage;
- invalid/fake/leak matrix is causal;
- evidence artifacts are physically written and directly inspectable;
- no GamePackage/UI/Runtime/Unity/provider/LLM/RAG/Lua/generator-library behavior is required.

Do not weaken existing tests.

## Validation commands

Run from `C:\Users\endim\LLMGameCreator\`.

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~SemanticAuthoringIntentResolver|FullyQualifiedName~SemanticAuthoringWorkspace|FullyQualifiedName~FeatureDrivenIntent|FullyQualifiedName~LoreIntakeSkeleton|FullyQualifiedName~Goal033"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~SemanticAuthoringIntentResolverProductSmokeTests"

Get-ChildItem .\.llmgc\procedural\goal-033-semantic-authoring-intent-resolver -File | Sort-Object Name | Select-Object Name,Length
Get-Content .\.llmgc\procedural\goal-033-semantic-authoring-intent-resolver\semantic-authoring-intent-resolver-report.md -TotalCount 120

.\.devflow\scripts\check-all.ps1
```

Avoid broad filters such as `FullyQualifiedName~ProductSmoke` if they select the whole product smoke set. Use exact new class filters.

If the repository has an artifact scope guard command/pattern, run it for Goal 033 artifacts after check-all or as part of check-all if already integrated.

## Stop conditions

Stop and report without commit/push if:

1. You need to change public GamePackage schema/model.
2. You need to touch UI/WinForms, Runtime, Unity, provider/LLM/RAG, Lua, generator-library, `.sln` or `.csproj`.
3. You need an external dependency.
4. You need to generate final dialogue prose or final GamePackage content to satisfy the goal.
5. Goal 031/032 gate state cannot be represented honestly.
6. Focused tests fail and cannot be repaired within allowed scope.
7. `check-all.ps1` fails.
8. check-all mutates historical artifacts outside Goal 033 and restore/cleanup is not explicitly allowed by current task policy. Report exact paths and stop.
9. Evidence contains absolute paths, nondeterministic timestamps, large logs or unstable ordering.
10. The implementation turns into a monolithic class instead of small model/catalog/validator/resolver/evidence components.

## Git policy

Allowed inspection commands anytime:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <changed-files>
git log --oneline -5
```

Allowed final commands only after all validation is green:

```text
git add <changed-files>
git commit -m "Goal 033 semantic authoring intent resolver"
git push origin main
```

Forbidden always unless the user separately authorizes:

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

Do not stage unrelated untracked `docs/agent-tasks/GOAL_032_*` files.

## Final report format

Report in Russian:

```text
Goal 033 выполнен / остановлен

Gate:
semantic_authoring_intent_resolver_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<список>

Реализовано:
<workspace / lore skeleton / provenance / intent resolver / evidence>

Evidence artifacts:
<список файлов>

Сценарии:
<frontier/gothic/caravan/metamodule summary>

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<negative cases>

Docs/current-state:
<what was updated, how prior gates were preserved>

Git:
<commit hash and push result or no-commit reason>

Ограничения / что не делалось:
<GamePackage/UI/Runtime/Unity/provider/LLM/RAG/Lua/generator-library/external deps not touched>

Следующий разумный шаг:
<one concise paragraph>
```
