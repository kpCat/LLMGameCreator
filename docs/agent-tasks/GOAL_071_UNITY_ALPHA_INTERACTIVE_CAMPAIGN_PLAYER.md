# Codex task — Goal 071 Unity Alpha Interactive Campaign Player

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
goal-071-unity-alpha-interactive-campaign-player
Goal 071: Unity Alpha Interactive Campaign Player
```

Codex reasoning level:

```text
very high
```

Manual gate to produce, not pass:

```text
unity_alpha_interactive_campaign_player_verification required
```

## Required preflight handoff

Before implementation, record the user handoff acceptance of Goal 070 in the docs quartet:

```text
integrated_campaign_timeline_simulation_matrix_verification passed before Goal 071
```

Do not mark Goal 071 as passed. Goal 071 must end as produced-for-review with:

```text
unity_alpha_interactive_campaign_player_verification required
accepted=false
```

## Read-first list

Read these first, in this order:

1. `AGENTS.md`
2. `docs/CONTEXT_INDEX.md`
3. `docs/CURRENT_GENERATOR_STATE.md`
4. `docs/CURRENT_GENERATOR_STATE.json`
5. `docs/FULL_GENERATOR_GOAL_QUEUE.md`
6. `docs/GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER_SPEC.md`
7. `docs/EXTERNAL_SCOUTING_GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER.md`
8. `docs/agent-tasks/GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER.md`
9. `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
10. Goal 070 artifacts under `.llmgc/procedural/goal-070-integrated-campaign-timeline-simulation-matrix/`
11. Goal 060/061/062/063/064/065/066/067/068/069 artifacts only as needed for source-loader references.
12. Local implementation/test analogs:
    - `src/LLMGameCreator.Application/Design/IntegratedCampaignTimelineSimulationMatrix/**`
    - `src/LLMGameCreator.Application/Design/FullCampaignPlayableReviewPackageRc/**`
    - `src/LLMGameCreator.Application/Design/FullGeneratorVariabilityRegressionMatrix/**`
    - `tests/LLMGameCreator.Tests/ProductSmoke/*Goal070*` or equivalent existing product smoke patterns.

Do not read the whole repository unless local search proves a file is needed.

## Allowed files / areas

You may create or edit only:

```text
docs/GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER_SPEC.md
docs/EXTERNAL_SCOUTING_GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER.md
docs/agent-tasks/GOAL_071_UNITY_ALPHA_INTERACTIVE_CAMPAIGN_PLAYER.md
docs/agent-tasks/GOAL_071_LAUNCHER.txt
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/CONTEXT_INDEX.md
docs/FULL_GENERATOR_GOAL_QUEUE.md
.devflow/artifact-scope/artifact-scope-policy.json
src/LLMGameCreator.Application/Design/UnityAlphaInteractiveCampaignPlayer/**
tests/LLMGameCreator.Tests/Application/UnityAlphaInteractiveCampaignPlayer/**
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaInteractiveCampaignPlayerProductSmokeTests.cs
unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
.llmgc/procedural/goal-071-unity-alpha-interactive-campaign-player/**
```

## Forbidden files / areas

Do not modify:

```text
src/LLMGameCreator.GamePackage/**
src/LLMGameCreator.Runtime/**
src/LLMGameCreator.Runtime.Abstractions/**
src/LLMGameCreator.WinForms/**
src/LLMGameCreator.Infrastructure/** provider/LLM/RAG paths
src/LLMGameCreator.Scripting/**
unity/** other than unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs
generator-library/**
samples/**
templates/**
*.sln
*.csproj
```

No external dependencies.

## Exact behavior

Implement a BCL-only Application seam:

```text
src/LLMGameCreator.Application/Design/UnityAlphaInteractiveCampaignPlayer/
```

Suggested small components:

- `UnityAlphaInteractiveCampaignSourceLoader`
- `UnityAlphaInteractiveCampaignBuilder`
- `UnityAlphaInteractiveCampaignValidator`
- `UnityAlphaInteractiveCampaignEvidenceService`
- `UnityAlphaInteractiveCampaignUnityProofRunner`
- models/hash helper as needed.

The seam must consume Goal 070 integrated timeline evidence and build:

1. a 9-row interactive campaign matrix;
2. a family/seed selector model;
3. an input/action script;
4. a state transition ledger;
5. a HUD/review contract;
6. a Unity staged command plan;
7. save/load/replay proof;
8. invalid/fake/leak diagnostics;
9. compact preview/export payloads.

### Unity Alpha behavior

Narrowly extend `AlphaRuntimeBootstrap.cs` so the player can load a staged Goal 071 command plan and emit deterministic logs.

Required markers include, at minimum:

```text
interactive_campaign_loaded=true
interactive_campaign_family=<family>
interactive_campaign_seed=<seed>
interactive_campaign_selected_row=<row id>
interactive_campaign_input=<input/action id>
interactive_campaign_step=<step id>
interactive_campaign_state_before=<hash>
interactive_campaign_state_after=<hash>
interactive_campaign_delta_applied=true
interactive_campaign_hud_rendered=true
interactive_campaign_row_completed=true
interactive_campaign_proof=goal071
```

Automated proof may drive a scripted input plan rather than real keyboard events if that is the existing Unity Alpha convention, but the implementation must also expose a narrow manual review surface or HUD contract for interactive use.

Use existing Unity proof patterns. Do not invent a new build system.

## Invalid/fake/leak matrix

Cover at least:

- missing Goal070 source;
- fake family/seed/row id;
- duplicate row id;
- command plan references unknown row;
- command plan skips required state transition;
- state hash does not change where it must;
- replay mismatch;
- missing HUD contract;
- Unity marker missing;
- unsafe path;
- provider/LLM/RAG claim;
- Runtime/GamePackage schema mutation claim;
- broad Unity mutation claim;
- final prose leakage;
- nondeterministic ordering.

## Tests

Add focused tests for:

- source loading;
- command plan construction;
- state transitions;
- save/load/replay proof;
- HUD contract;
- invalid/fake/leak matrix;
- evidence artifacts.

Add product smoke:

```text
tests/LLMGameCreator.Tests/ProductSmoke/UnityAlphaInteractiveCampaignPlayerProductSmokeTests.cs
```

## Validation commands

Use PowerShell from repo root:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaInteractiveCampaignPlayer|FullyQualifiedName~Goal071"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~UnityAlphaInteractiveCampaignPlayerProductSmokeTests"

dotnet test .\tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --no-build --no-restore --filter "FullyQualifiedName~CurrentState|FullyQualifiedName~Goal071|FullyQualifiedName~UnityAlphaInteractive"

.\.devflow\scripts\check-all.ps1

.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-071-unity-alpha-interactive-campaign-player"
```

If the existing test filter naming differs, adapt to the exact new class names.

## Bounded repairs pre-authorized

You may update stale current-state/handoff guard tests only if they block the new current gate and only if they are clearly the same pattern already repaired in previous goals.

You may restore exact accidental historical `.llmgc/procedural/**` artifacts from HEAD if `check-all.ps1` mutates old tracked evidence outside Goal 071.

Do not use broad git restore/reset/clean.

## Git policy

Codex must commit and push final state to `origin/main` even if result is GREEN/BLOCKED/FAILED.

Commit messages:

```text
GREEN Goal 071 unity alpha interactive campaign player
BLOCKED Goal 071 unity alpha interactive campaign player
FAILED Goal 071 unity alpha interactive campaign player
```

Allowed git commands:

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
git commit -m <message>
git rev-parse HEAD
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
Goal 071 выполнен / заблокирован / провален
Status: GREEN/BLOCKED/FAILED
Gate: unity_alpha_interactive_campaign_player_verification required
Commit: <hash>
Push: <result>

Что стало реальнее:
<1-3 предложения>

Изменённые области:
<paths>

Unity proof:
<unity/player exit codes, matched markers, row count>

Interactive proof:
<family/seed selection, input/action script, state deltas, HUD contract>

Проверки:
<commands/results>

Invalid/fake/leak matrix:
<summary>

Ограничения:
<forbidden areas untouched>

Следующий разумный шаг:
<one paragraph>
```
