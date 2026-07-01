# Codex Task — Goal 070 Integrated Campaign Timeline Simulation Matrix

## Invocation metadata

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
goal_070_integrated_campaign_timeline_simulation_matrix
Goal 070: Integrated Campaign Timeline Simulation Matrix
```

Required goal marker / manual gate:

```text
integrated_campaign_timeline_simulation_matrix_verification required
```

Codex reasoning level:

```text
very high
```

## Required first action

Read this task file fully. Treat it as the authoritative contract for this goal.

This is an aggressive composite goal. It must first record Goal 069 acceptance by user handoff, then implement Goal 070.

Do not ask for confirmation unless a safety or repository integrity stop condition is hit.

## Read-first list

Read, in this order:

1. `AGENTS.md`
2. `README.md`
3. `docs/CONTEXT_INDEX.md`
4. `docs/CURRENT_GENERATOR_STATE.md`
5. `docs/CURRENT_GENERATOR_STATE.json`
6. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
7. `docs/GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX_SPEC.md`
8. `docs/EXTERNAL_SCOUTING_GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX.md`
9. `.devflow/artifact-scope/artifact-scope-policy.json`
10. Goal 060 artifacts under `.llmgc/procedural/goal-060-full-campaign-gamepackage-materialization-matrix/`
11. Goal 061 artifacts under `.llmgc/procedural/goal-061-full-campaign-playable-review-package-rc/`
12. Goal 062 artifacts under `.llmgc/procedural/goal-062-constrained-spatial-detail-generation/`
13. Goal 063 artifacts under `.llmgc/procedural/goal-063-gameplay-consequence-depth-matrix/`
14. Goal 064 artifacts under `.llmgc/procedural/goal-064-living-world-npc-faction-simulation-matrix/`
15. Goal 065 artifacts under `.llmgc/procedural/goal-065-interlocked-gameplay-systems-depth-matrix/`
16. Goal 066 artifacts under `.llmgc/procedural/goal-066-settlement-construction-destruction-production-matrix/`
17. Goal 067 artifacts under `.llmgc/procedural/goal-067-programmatic-narrative-quest-dialogue-event-matrix/`
18. Goal 068 artifacts under `.llmgc/procedural/goal-068-combat-magic-ability-boss-encounter-matrix/`
19. Goal 069 artifacts under `.llmgc/procedural/goal-069-world-event-weather-daynight-crisis-matrix/`
20. The closest local Application seams/tests/product smokes for Goals 063-069.
21. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`

Read only the relevant files. Do not scan the whole repo.

## Preflight: record Goal 069 acceptance

Before implementing Goal 070, update the docs quartet to record:

```text
world_event_weather_daynight_crisis_matrix_verification passed before Goal 070
```

This is a user handoff acceptance. Do not mark Goal 070 passed.

Goal 031 and Goal 032 must remain produced-for-review/not passed if that is how current docs record them.

## Allowed files / areas

You may create/edit only:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
docs/GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX.md
docs/agent-tasks/GOAL_070_INTEGRATED_CAMPAIGN_TIMELINE_SIMULATION_MATRIX.md
docs/agent-tasks/GOAL_070_LAUNCHER.txt
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/IntegratedCampaignTimelineSimulationMatrix/**
tests/LLMGameCreator.Tests/Application/IntegratedCampaignTimelineSimulationMatrix/**
tests/LLMGameCreator.Tests/ProductSmoke/IntegratedCampaignTimelineSimulationMatrixProductSmokeTests.cs
.llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix/**
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
```

The Unity file may only receive a narrow staged command-plan loader/marker extension for Goal 070, following the existing Goal 063-069 patterns.

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG/media paths
src/LLMGameCreator.Scripting/**
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

Also forbidden:

- public GamePackage schema changes;
- Runtime or Runtime.Abstractions changes;
- broad Unity gameplay implementation;
- external dependencies;
- arbitrary Lua or generated Lua execution;
- live provider/LLM/RAG/media generation calls;
- final prose generation as gameplay content;
- branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Source loading

Create a BCL-only Application seam under:

```text
src/LLMGameCreator.Application/Design/IntegratedCampaignTimelineSimulationMatrix/
```

It must load compact evidence from Goals 060-069 and verify source integrity before producing anything.

The source loader must verify:

- Goal 069 handoff acceptance is recorded in current docs;
- Goal 060-069 artifact folders exist;
- family/seed row identity aligns across source artifacts;
- required row/proof/report files exist;
- no source row is replaced by an invented id;
- source proofs are GREEN/produced as expected.

### 2. Timeline row matrix

Produce exactly 9 family/seed timeline rows:

```text
map_panel_rpg / seed_alpha
map_panel_rpg / seed_beta
map_panel_rpg / seed_gamma
survival_sandbox / seed_alpha
survival_sandbox / seed_beta
survival_sandbox / seed_gamma
first_person_grid_dungeon / seed_alpha
first_person_grid_dungeon / seed_beta
first_person_grid_dungeon / seed_gamma
```

Each row must contain at least 6 ordered ticks/phases. Example phase families:

- dawn/night/weather/crisis pressure;
- NPC/faction/world event update;
- settlement production/damage/repair/defense update;
- narrative/quest/dialogue/event update;
- combat/magic/ability/status update;
- economy/crafting/resource/inventory/progression update;
- spatial/traversal/chunk update.

Do not merely list all categories. The row must include causal before/after deltas.

### 3. Cascades and arbitration

Each row must prove:

- at least three cross-system cascades;
- at least one conflict or arbitration decision.

Examples:

```text
storm -> bridge damage -> route change -> merchant delay -> faction reputation change
night crisis -> NPC unavailable -> quest option locked -> alternative dialogue option enabled
boss phase hazard -> settlement defense damage -> repair resource cost -> crafting demand
```

Conflict/arbitration examples:

- weather reduces travel but quest deadline pressures travel;
- NPC schedule conflicts with crisis duty;
- resource shortage conflicts with settlement repair;
- boss loot conflicts with faction embargo;
- survival need conflicts with narrative branch.

### 4. State proof

For every row, produce:

- initial state hash;
- per-tick state hash;
- final state hash;
- save checkpoint hash;
- loaded checkpoint hash;
- replay hash;
- state-changing proof.

The final hash must differ from initial hash. Replay hash must match expected replay. Save/load checkpoint must roundtrip.

### 5. Variance proof

Prove meaningful variance:

- same family, different seeds must differ in at least two meaningful categories;
- different families must differ in family-specific phase profile;
- row hashes must be distinct;
- variance must not be only id/hash/name differences.

### 6. Unity Alpha proof

Narrowly extend `AlphaRuntimeBootstrap.cs` to load a staged Goal 070 command plan and emit deterministic markers.

Required marker families:

```text
campaign_timeline_loaded
campaign_timeline_row_started
campaign_timeline_tick
campaign_timeline_cascade
campaign_timeline_arbitration
campaign_timeline_row_completed
campaign_timeline_matrix_completed
review_package_proof=goal070
```

Unity proof must run via the existing local Unity Alpha proof pattern. Do not fake Unity/player proof in JSON only.

### 7. Evidence artifacts

Write compact deterministic artifacts under:

```text
.llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix/
```

Required artifacts:

```text
source-manifest.json
timeline-matrix-summary.json
campaign-timeline-row-map_panel_rpg-seed_alpha.json
campaign-timeline-row-map_panel_rpg-seed_beta.json
campaign-timeline-row-map_panel_rpg-seed_gamma.json
campaign-timeline-row-survival_sandbox-seed_alpha.json
campaign-timeline-row-survival_sandbox-seed_beta.json
campaign-timeline-row-survival_sandbox-seed_gamma.json
campaign-timeline-row-first_person_grid_dungeon-seed_alpha.json
campaign-timeline-row-first_person_grid_dungeon-seed_beta.json
campaign-timeline-row-first_person_grid_dungeon-seed_gamma.json
cross-system-cascade-ledger.json
conflict-arbitration-ledger.json
save-load-replay-audit.json
variance-metrics.json
unity-command-plan.json
unity-player-proof-summary.json
preview-export-timeline-payload.json
invalid-diagnostics-matrix.json
artifact-scope-report.json
integrated-campaign-timeline-simulation-matrix-report.md
```

Use deterministic ordering. No absolute paths. No timestamps unless already required by an existing deterministic convention.

### 8. Invalid/fake/leak matrix

Cover at least these cases:

- missing Goal 069 source;
- stale Goal 069 handoff;
- missing family row;
- duplicate row id;
- fake source id;
- fake family;
- fake seed;
- missing cross-system cascade;
- missing arbitration;
- unchanged final state;
- replay mismatch;
- save/load mismatch;
- variance only by id/hash;
- final prose leakage;
- provider/LLM/RAG/media generation claim;
- arbitrary Lua execution claim;
- Runtime/UI/GamePackage schema mutation claim;
- broad Unity gameplay mutation claim;
- unsafe path;
- nondeterministic order.

Each invalid case must produce a causal diagnostic/status.

## Tests

Add focused tests under:

```text
tests/LLMGameCreator.Tests/Application/IntegratedCampaignTimelineSimulationMatrix/
```

Suggested coverage:

- source loading and handoff guard;
- 9-row timeline matrix shape;
- cascade/arbitration presence;
- save/load/replay proof;
- meaningful variance;
- invalid/fake/leak matrix;
- evidence artifact writing and JSON parse;
- Unity command plan proof.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/IntegratedCampaignTimelineSimulationMatrixProductSmokeTests.cs
```

## Validation commands

Run from `C:\Users\endim\LLMGameCreator\`:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~IntegratedCampaignTimeline|FullyQualifiedName~Goal070"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~IntegratedCampaignTimelineSimulationMatrixProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal070|FullyQualifiedName~IntegratedCampaignTimeline"

.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-070-integrated-campaign-timeline-simulation-matrix"
```

Also perform a mojibake marker scan over changed/new text files and Goal 070 artifacts.

## Bounded repairs pre-authorized

You may perform bounded repairs without asking the user if all limits are respected:

1. Update stale current-state/handoff guard tests if they hardcode the previous latest goal and block valid Goal 070 state.
2. Restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates them outside Goal 070 scope.
3. Update artifact-scope policy only for the exact Goal 070 artifact folder and changed path groups.
4. Narrowly extend `AlphaRuntimeBootstrap.cs` only for staged Goal 070 command plan markers.

No reset/stash/clean/merge/rebase/cherry-pick/force-push.

## Git policy

At the end, always commit and push final state to `origin/main`, regardless of GREEN/BLOCKED/FAILED.

Allowed inspection/final git commands:

```text
git branch --show-current
git status -sb
git status --short --untracked-files=all
git diff --stat
git diff -- <explicit files>
git add -- <explicit allowed paths>
git diff --cached --name-status
git diff --cached --stat
git diff --cached --check
git commit -m "GREEN Goal 070 integrated campaign timeline simulation matrix"
git commit -m "BLOCKED Goal 070 integrated campaign timeline simulation matrix"
git commit -m "FAILED Goal 070 integrated campaign timeline simulation matrix"
git rev-parse HEAD
git push origin main
```

Commit message must reflect status.

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

## Stop / BLOCKED / FAILED policy

Use GREEN only when all GREEN thresholds are honestly met.

Use BLOCKED if:

- Unity route is unavailable;
- source evidence chain cannot be consumed safely;
- current public schema cannot support proof without forbidden changes;
- check-all fails due to unrelated but unresolved blocker;
- artifact scope cannot be made clean within allowed scope.

Use FAILED if implementation is incomplete or internally inconsistent.

Even for BLOCKED/FAILED, commit and push the final state with honest report.

## Final report format

Report in Russian:

```text
Goal 070 выполнен / заблокирован / failed
Status: GREEN / BLOCKED / FAILED
Gate: integrated_campaign_timeline_simulation_matrix_verification required
Commit: <hash>
Push: <result>

Что стало реальнее:
<1-3 предложения>

Изменённые области:
<files/folders>

Proof:
- row count
- state-changing rows
- cascade count
- arbitration count
- save/load/replay
- variance
- Unity proof

Проверки:
<commands and results>

Invalid/fake/leak matrix:
<summary>

Ограничения:
<what was not touched>

Следующий разумный шаг:
<one paragraph>
```
