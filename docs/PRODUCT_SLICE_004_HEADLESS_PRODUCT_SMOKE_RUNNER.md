# Product Slice 004: Headless Product Smoke Runner

## Goal

Stop relying on manual UI clicks for every product slice.

This slice adds an automated smoke path for the baseline product flow:

```text
approved baseline artifacts
-> package assembly
-> package export
-> validation
-> report
```

## Why now

Manual verification proved that Slice 003 can export a package. The exported package shows package assembly/export exists, but also shows why automation is needed: a manual run can accidentally apply only part of the baseline set. The smoke runner must assert the complete intended baseline flow.

## Smoke scenario 001: baseline-strict-package-assembly

Input:
A deterministic fixture approved artifact set with all four M4.1 baseline contracts:

```text
game_profile_v1
scene_pack_v1
quest_pack_v1
mechanics_pack_v1
```

Expected output:

```text
package.json exists
manifest title/description are populated from game_profile_v1
generatedContent.profile is populated
generatedContent.scenes has at least 1 scene
generatedContent.quests has at least 1 quest
generatedContent.mechanics has at least 1 mechanic
generatedContent.appliedArtifacts has exactly/at least 4 baseline applied artifacts
provenance exists for each applied artifact
no assembly errors
validation report has no critical package assembly errors
```

## Important distinction

This smoke runner must not call LLM.

It must use fixture artifacts, not LM Studio:

```text
No provider
No OpenAI-compatible API
No repair prompt
No runtime preview
No UI automation
```

## Desired command

Preferred command:

```powershell
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario baseline-strict-package-assembly
```

If implementing a script is too risky, a focused test filter is acceptable:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
```

Best result: both exist. The script simply wraps the focused test and writes a small run report.

## Output report

The smoke runner should write to:

```text
.devflow/runs/<timestamp>-product-smoke/
```

Expected files if practical:

```text
product-smoke-summary.md
product-smoke-summary.json
test-results/
package-output/package.json
```

Do not overbuild reporting. Useful and deterministic is better than fancy.

## Fixtures

Create deterministic test fixtures or test factory methods for four baseline artifacts.

Minimum artifact content:

### game_profile_v1

Must include:

```text
schema_version
artifact_kind
game.title
game.description
game.genre
game.tone
game.presentation_mode
game.world_topology
game.actor_model
game.combat_model
game.core_loop
pillars
source_context
```

### scene_pack_v1

At least one scene with id/title/description/purpose.

### quest_pack_v1

At least one quest with id/title/description/steps.

### mechanics_pack_v1

At least one mechanic with id/name/title/description.

## Done

This slice is done when a developer can run one command and verify package assembly without UI.
