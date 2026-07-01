# Codex task — GOAL 067 Programmatic Narrative Quest Dialogue Event Matrix

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
goal-067-programmatic-narrative-quest-dialogue-event-matrix
Goal 067: Programmatic Narrative Quest Dialogue Event Matrix
```

Codex reasoning level:

```text
very high
```

Expected gate:

```text
programmatic_narrative_quest_dialogue_event_matrix_verification required
```

## Status policy

This is an aggressive composite goal.

You must commit and push final state to `origin/main` even if the result is `GREEN`, `BLOCKED`, or `FAILED`.

Use honest commit messages:

```text
GREEN Goal 067 programmatic narrative quest dialogue event matrix
BLOCKED Goal 067 programmatic narrative quest dialogue event matrix
FAILED Goal 067 programmatic narrative quest dialogue event matrix
```

Do not mark the manual gate passed. Leave:

```text
programmatic_narrative_quest_dialogue_event_matrix_verification required
accepted=false
```

unless the user explicitly asks for acceptance later.

## Preflight

1. Confirm current branch is `main`.
2. Confirm working copy state.
3. Record Goal 066 handoff acceptance in the docs quartet before implementing Goal 067:

```text
settlement_construction_destruction_production_matrix_verification passed before Goal 067
```

4. Do not start Goal 068.
5. Do not invent acceptance for Goal 031/032 if current docs keep them produced-for-review/not passed.

## Read-first list

Read in this order:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/ROADMAP_TO_FULL_GENERATOR.md` if present
8. `docs/GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX_SPEC.md`
9. `docs/EXTERNAL_SCOUTING_GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX.md`
10. `docs/agent-tasks/GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX.md`
11. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
12. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
13. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
14. Goal 063 artifacts under `.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/`
15. Goal 064 artifacts under `.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/`
16. Goal 065 artifacts under `.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/`
17. Goal 066 artifacts under `.llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix/`
18. Closest implementation analogs:
    - `src/LLMGameCreator.Application/Design/GameplayConsequenceDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/LivingWorldNpcFactionSimulationMatrix/**`
    - `src/LLMGameCreator.Application/Design/InterlockedGameplaySystemsDepthMatrix/**`
    - `src/LLMGameCreator.Application/Design/SettlementConstructionDestructionProductionMatrix/**`
    - their focused tests and product smoke tests
19. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

Do not read the whole repo unless a local search shows the exact relevant file is elsewhere.

## Allowed files / areas

You may create/edit only:

```text
docs/GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX.md
docs/agent-tasks/GOAL_067_PROGRAMMATIC_NARRATIVE_QUEST_DIALOGUE_EVENT_MATRIX.md
docs/agent-tasks/GOAL_067_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/**
tests/LLMGameCreator.Tests/Application/ProgrammaticNarrativeQuestDialogueEventMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/ProgrammaticNarrativeQuestDialogueEventMatrixProductSmokeTests.cs
.llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

Narrow Unity allowance is only for deterministic marker loading/proof. Do not implement broad UI or a narrative runtime.

## Forbidden files / areas

Do not change:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/**
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- external dependencies;
- Yarn Spinner/ink integration;
- provider/LLM/RAG calls;
- final unbounded prose generation;
- Runtime narrative execution engine;
- public GamePackage schema/model changes;
- arbitrary Lua execution;
- media/provider generation.

## Exact behavior

### 1. Build an Application-layer seam

Create:

```text
src/LLMGameCreator.Application/Design/ProgrammaticNarrativeQuestDialogueEventMatrix/
```

Use small files following local Goal 063–066 style. Suggested components:

```text
ProgrammaticNarrativeSourceLoader
ProgrammaticNarrativeMatrixBuilder
ProgrammaticNarrativeModels
ProgrammaticNarrativeValidator
ProgrammaticNarrativeEvidenceService
ProgrammaticNarrativeHash
ProgrammaticNarrativeUnityProofRunner
```

### 2. Source loading

The source loader must consume existing compact artifacts from Goals 060–066. It must verify:

- Goal 066 handoff phrase is present in state docs after preflight;
- Goal 060 materialized package rows exist;
- Goal 061 review package RC exists;
- Goal 062 spatial detail rows exist;
- Goal 063 gameplay consequence rows exist;
- Goal 064 living-world rows exist;
- Goal 065 interlocked gameplay rows exist;
- Goal 066 settlement rows exist.

Do not infer success from filenames only. Read compact JSON/report fields where possible.

### 3. Produce 9 narrative rows

For each family/seed row, create a narrative row with:

- quest stage graph;
- dialogue option graph;
- event trigger/consequence chain;
- localization key/template-slot records;
- memory/rumor propagation record;
- state delta references;
- links to:
  - living-world NPC/faction state;
  - interlocked gameplay economy/combat/status state;
  - settlement construction/destruction/production state;
  - package row;
  - spatial row.

Narrative rows must be state-changing. The row must show before/after or applied deltas, not only descriptions.

### 4. Programmatic dialogue policy

Do not generate final dialogue prose.

Allowed record examples:

```json
{
  "lineKey": "map_panel_rpg.seed_alpha.dialogue.blacksmith.warning.001",
  "templateId": "npc_warning_low_trust",
  "speakerRole": "settlement_crafter",
  "toneTags": ["tense", "low_trust"],
  "slots": {
    "settlementName": "frontier_watch",
    "resourceName": "iron_fittings"
  },
  "conditions": ["factionTrust < 0", "settlementDamage > 0"],
  "optionEffects": ["repair_discount_blocked", "rumor_recorded"]
}
```

Avoid unbounded text fields like `lineText` or `finalDialogue`.

### 5. Quest/event consequence proof

Each row must show:

- at least 3 ordered quest/event/dialogue steps;
- at least 2 meaningful state deltas;
- at least 1 consequence that changes later availability/option/event state;
- replay determinism;
- save/load restoration.

### 6. Unity proof

Create a staged command plan under the Goal 067 artifact root. Extend `AlphaRuntimeBootstrap.cs` narrowly to load and emit markers.

Required marker concepts:

```text
narrative_row_loaded
quest_stage_started
dialogue_option_available
dialogue_option_selected
event_trigger_resolved
event_consequence_applied
memory_rumor_recorded
localization_key_bound
narrative_row_completed
```

Unity/player proof must run through the existing local pattern if available. If Unity route is unavailable, commit/push `BLOCKED` with exact reason.

### 7. Evidence artifacts

Write deterministic compact artifacts under:

```text
.llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix/
```

Required artifacts are listed in the spec.

No timestamps unless the repo already has deterministic timestamp convention. No absolute machine paths in evidence. No heavy build/log/unity-work outputs.

### 8. Invalid/fake/leak matrix

Cover at least:

- missing Goal 066 source;
- fake package row;
- fake NPC/faction ref;
- fake settlement ref;
- fake interlocked gameplay ref;
- duplicate narrative row id;
- missing quest stage graph;
- missing dialogue option graph;
- final prose leakage;
- provider/LLM/RAG claim;
- Yarn/ink runtime dependency claim;
- Runtime/UI/GamePackage schema mutation claim;
- unsafe Unity broad mutation claim;
- nondeterministic ordering;
- missing replay trace;
- event consequence without state delta;
- localization key without template/slots;
- memory/rumor without source actor/faction context.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/ProgrammaticNarrativeQuestDialogueEventMatrix/
```

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/ProgrammaticNarrativeQuestDialogueEventMatrixProductSmokeTests.cs
```

Tests must prove:

- source loading;
- 9 row matrix;
- quest/dialogue/event ledgers;
- localization key/template-slot table;
- save/load/replay;
- meaningful variance;
- invalid/fake/leak matrix;
- Unity/player marker proof if available;
- evidence artifacts written and parseable.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~ProgrammaticNarrativeQuestDialogue|FullyQualifiedName~Goal067"

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~ProgrammaticNarrativeQuestDialogueEventMatrixProductSmokeTests"

dotnet test .	ests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal067|FullyQualifiedName~ProgrammaticNarrative"

.\.devflow\scripts\check-all.ps1

.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-067-programmatic-narrative-quest-dialogue-event-matrix"
```

If a broad filter accidentally selects too much and times out, rerun with exact new test class filters and report it.

## Bounded repairs pre-authorized

You may perform bounded repairs if needed:

1. Update stale current-state/handoff guard tests only if check-all fails because an old test hardcodes a previous current gate.
2. Restore exact accidental historical `.llmgc/procedural/**` artifact paths from HEAD if check-all mutates them outside Goal 067 scope.
3. Add Goal 067 artifact-scope policy entry.
4. Fix mojibake only in changed Goal 067/docs files.

Do not use these permissions for broad refactors.

## Git policy

Always commit/push final state to `origin/main`.

Allowed git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git add -- <explicit allowed paths>
git commit -m "<honest message>"
git rev-parse HEAD
git rev-parse --short HEAD
git push origin main
```

Forbidden:

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
Status: GREEN / BLOCKED / FAILED
Gate: programmatic_narrative_quest_dialogue_event_matrix_verification required

Что стало реальнее:
<1-3 предложения>

Изменённые файлы:
<summary>

Proof:
- 9/9 rows?
- state-changing rows?
- quest/dialogue/event ledgers?
- localization key/template proof?
- save/load/replay?
- Unity/player proof?
- invalid matrix?

Проверки:
<commands and results>

Git:
<commit hash, push result, clean/aligned state>

Ограничения:
<confirm no provider/LLM/RAG, no final prose, no Yarn/ink runtime, no public GamePackage schema, no Runtime/UI broad changes>
```
