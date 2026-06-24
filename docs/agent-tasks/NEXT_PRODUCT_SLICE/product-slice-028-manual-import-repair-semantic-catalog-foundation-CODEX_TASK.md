# Product Slice 028 — Manual Import Repair + Semantic Catalog Foundation v1

Executor: Codex.

This is one intentionally large, unified task. Do not split it into separate slices unless a stop condition is hit.

## Goal

Implement Product Slice 028 as a combined repair + foundation slice:

1. Repair two S027 issues found during real user testing:
   - `Create/Open manual-import folder` fails with `Unsafe Unity archive relative path: manual-import`.
   - Idempotent `AlreadyImported` manual import runs create extra archive-review history snapshots even when target bytes do not change.

2. Add a project-local semantic catalog foundation:
   - map existing `semantic_pack_v1` artifacts into a deterministic sidecar semantic catalog;
   - preserve unknown/candidate semantic terms without blocking generation;
   - create semantic reports and diagnostics;
   - provide a generation-context preview showing how semantic terms would guide future LLM/provider/game-content generation.

3. Add an explicit LLM-minimization decision record:
   - LLM is used only where it is actually needed;
   - deterministic services should do everything else;
   - semantic dictionaries are not a limitation/taxonomy prison;
   - semantic dictionaries are a reusable, project-local memory and control layer for generation.

This task must not unlock M5/M6, must not implement Runtime behavior, must not change GamePackage schema, and must not execute providers/LLMs/generators/Lua/Unity.

---

## User-facing product direction

The product direction is:

```text
User intent / preset / example
→ minimal LLM calls for creative semantic/game-design decisions only
→ deterministic validation and assembly
→ deterministic GamePackage/export/archive/request planning
→ manual/provider fulfillment
→ review/history/comparison
→ user approval/testing
```

S028 must make this direction explicit in docs and code.

The project must not evolve into "LLM generates everything and C# accepts it".

The desired rule is:

```text
If a step can be done deterministically without quality loss, variability loss, balance loss, or authoring-power loss,
then the combiner must do it without LLM.
```

---

## Hard path and repo policy

Work in the current working tree only.

Do not run git commands.

Do not create branches.

Do not switch branches.

Do not merge.

Do not rebase.

Do not cherry-pick.

Do not push.

Use repo-relative paths and normal Windows/PowerShell paths only.

Do not use:

```text
/mnt
/home/oai
sandbox:/...
C:\mnt
```

Do not put container paths in code, docs, tests, generated JSON, or final report.

---

## Read first

Read these files before editing anything:

```text
AGENTS.md
docs/CONTEXT_INDEX.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/PRODUCT_SMOKE_SCENARIOS.md

docs/PRODUCT_SLICE_026_CONTROLLED_MANUAL_PROVIDER_OUTPUT_IMPORT.md
docs/PRODUCT_SLICE_027_CONTROLLED_MANUAL_IMPORT_WORKSPACE_UI.md

src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportMarkdownRenderer.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualImportTemplateModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualImportTemplateService.cs

src/LLMGameCreator.Application/Composition/UnityArchiveReviewSnapshotService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewHistoryService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveReviewComparisonService.cs

src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanGamePackageAssembler.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanApprovedArtifactSet*.cs
src/LLMGameCreator.Application/Design/GeneratorPlans/GeneratorPlanDraftArtifact*.cs

src/LLMGameCreator.WinForms/CompositionRoot.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewViewState.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveManualProviderImportTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveManualImportTemplateTests.cs
tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/UnityArchiveManualImportWorkflowSmokeTests.cs
```

If a listed file does not exist, search for the actual replacement and report the substitution in the final answer.

---

# Part A — Repair S027 manual-import folder helper

## Problem

Real user testing found:

```text
Manual import folder could not be opened: Unsafe Unity archive relative path: manual-import
```

Root cause is likely that `UnityArchiveManualImportTemplateService.EnsureManualImportDirectory(...)` calls a file-output path validator that rejects directory path `manual-import`.

## Required behavior

The UI action:

```text
Create/Open manual-import folder
```

must:

- safely create:
  - `.llmgc/unity-archive/manual-import/`
- open it if UI is running interactively;
- not create `import-manifest.json`;
- not run import;
- not touch provider output targets;
- not throw on valid `manual-import`;
- still reject unsafe directory paths.

## Implementation

Add a dedicated safe directory relative path helper if needed.

Accepted directory-relative path rules:

- not empty;
- not rooted;
- no drive letters;
- no backslashes in normalized archive-relative values;
- no `.` or `..` segments;
- no empty segments;
- must remain contained under archive root after `Path.GetFullPath`.

`manual-import` must be valid.

`manual-import/put-files-here` must be valid.

`manual-import/../x`, `C:/x`, `\\server\share`, `/absolute`, `manual-import\bad`, and empty path must be invalid.

## Tests

Add focused tests:

```text
EnsureManualImportDirectoryAcceptsManualImportDirectory
EnsureManualImportDirectoryRejectsTraversal
UnityArchiveReviewPresenterOpenManualImportFolderReturnsReadyStatus
```

If UI-level Explorer launching is hard to test, test presenter/service result only.

---

# Part B — Repair S027 idempotent history spam

## Problem

Real user testing found:

- first import: `Imported=1`, history snapshot added.
- second import with identical bytes: `AlreadyImported=1`, no target bytes changed, but history snapshot count still increased.

This is undesirable because repeated no-op imports pollute archive-review history.

## Required behavior

Manual import report should still be written after every import attempt.

But archive review/history/comparison refresh should be conditional:

```text
If at least one target output file was Imported/Overwritten/Changed:
    refresh fulfillment state
    refresh archive review
    store history
    refresh comparison

If all entries are AlreadyImported and no target output bytes changed:
    write manual import report
    optionally refresh fulfillment state only if needed
    do NOT store a new archive-review history snapshot
    do NOT create a new comparison snapshot pair

If import is MissingManifest / Conflict / Invalid / Failed only:
    write manual import report
    do NOT store a new archive-review history snapshot
```

Keep S026 service as the authority for copy/validation/conflict/report. Do not move validation into UI.

## Implementation hints

Preferred approach:

- In `UnityArchiveManualProviderImportService`, after processing entries, determine whether target archive bytes changed.
- The result already has per-entry statuses. Use imported/overwritten/changed statuses if the model has them; if only `Imported` means "bytes written", use `ImportedCount > 0`.
- Refresh review/history/comparison only when target bytes changed.
- Always write `manual-provider-import-report.json/.md`.
- Keep behavior deterministic.

If current model cannot express "target changed", add a property to result:

```csharp
public bool TargetOutputsChanged { get; init; }
```

or similar.

Do not introduce timestamps.

## Tests

Add tests:

1. First import writes target and creates/updates history.
2. Second import of identical bytes:
   - returns `AlreadyImported`;
   - `TargetOutputsChanged == false` if property exists;
   - history index sequence/count does not increase.
3. Conflict-only run:
   - no history snapshot added.
4. Overwrite with different bytes:
   - target changes;
   - history sequence/count increases.

Also update product smoke if useful.

---

# Part C — Semantic catalog foundation

## Current problem

`semantic_pack_v1` is currently acknowledged but does not map into `GamePackage v1`.

That is acceptable for earlier slices, but it leaves semantic output in limbo.

S028 must add a sidecar semantic catalog without changing GamePackage schema.

## Non-goal

Do not change `GamePackageDefinition`.

Do not add Runtime semantic behavior.

Do not unlock M6.

Do not require LLM.

Do not implement semantic text generation.

Do not add a giant universal world-ontology.

## Sidecar output

Add deterministic project-local semantic outputs:

```text
.llmgc/semantic/semantic-catalog.json
.llmgc/semantic/semantic-catalog-report.md
.llmgc/semantic/semantic-generation-context-preview.json
.llmgc/semantic/semantic-generation-context-preview.md
```

If existing project-local output conventions prefer `.llmgc/generator-plans/semantic/...`, use that convention, but document the chosen path.

Preferred path remains:

```text
.llmgc/semantic/
```

because semantic catalog is project-level generation memory, not a single package export artifact.

## Semantic catalog purpose

The semantic catalog is a project-local meaning memory:

- known terms;
- candidate terms;
- relations;
- source artifacts;
- usage hints for future generation;
- diagnostics;
- conflicts.

It should guide generation without restricting creativity.

---

# Part D — Semantic term model

Add Application-layer models, likely under:

```text
src/LLMGameCreator.Application/Design/Semantics/
```

or current project convention equivalent.

Suggested files:

```text
SemanticCatalogModels.cs
SemanticCatalogService.cs
SemanticCatalogMarkdownRenderer.cs
SemanticGenerationContextPreviewModels.cs
SemanticGenerationContextPreviewService.cs
SemanticGenerationContextPreviewMarkdownRenderer.cs
```

Use actual project namespace conventions.

## Required catalog model

Minimum JSON shape:

```json
{
  "schemaVersion": "1",
  "catalogId": "project-semantic-catalog",
  "terms": [
    {
      "termId": "theme/survival",
      "kind": "theme",
      "label": "Survival",
      "status": "known",
      "aliases": [],
      "sourceArtifactIds": [],
      "notes": ""
    }
  ],
  "relations": [
    {
      "relationId": "relation/location.sky_lantern_outpost/theme/survival",
      "sourceTermId": "location/sky_lantern_outpost",
      "relationKind": "has_theme",
      "targetTermId": "theme/survival",
      "status": "known",
      "sourceArtifactIds": []
    }
  ],
  "diagnostics": []
}
```

## Required term kinds

Support at least these semantic kinds:

```text
theme
tone
biome
faction
faction_relation
npc_archetype
dialogue_intent
quest_motif
item_affordance
location_mood
asset_style_hint
audio_mood_hint
entity_role
unknown
```

Do not hard-fail on unknown kind. Normalize to `unknown` with warning.

## Required statuses

```text
known
candidate
deprecated
conflict
invalid
```

Default for terms from generated semantic packs:

```text
candidate
```

Built-in seed terms may be:

```text
known
```

## Built-in seed semantics

Add a small built-in seed dictionary, not a huge ontology.

It may be in code or JSON under docs/test resources if project convention allows.

Minimum useful seed terms:

Themes:

```text
theme/survival
theme/exploration
theme/mystery
theme/political_intrigue
theme/occult
theme/trade
theme/combat
theme/crafting
```

Dialogue intents:

```text
dialogue_intent/greet
dialogue_intent/warn
dialogue_intent/threaten
dialogue_intent/bargain
dialogue_intent/comfort
dialogue_intent/reveal_secret
dialogue_intent/ask_for_help
dialogue_intent/give_quest
```

Item affordances:

```text
item_affordance/edible
item_affordance/tradable
item_affordance/craft_material
item_affordance/quest_item
item_affordance/weapon
item_affordance/tool
item_affordance/consumable
```

Location moods:

```text
location_mood/safe
location_mood/dangerous
location_mood/isolated
location_mood/sacred
location_mood/ruined
location_mood/busy
```

Asset style/audio hints:

```text
asset_style_hint/portrait
asset_style_hint/tile
asset_style_hint/icon
asset_style_hint/hand_painted
asset_style_hint/low_poly
audio_mood_hint/calm
audio_mood_hint/tense
audio_mood_hint/mysterious
audio_mood_hint/combat
```

Keep this seed small. The goal is infrastructure, not exhaustive content.

---

# Part E — Map semantic_pack_v1 into semantic catalog

## Existing behavior

`GeneratorPlanGamePackageAssembler` currently recognizes `semantic_pack_v1` and preserves it, but records it as unmapped because GamePackage v1 has no semantic field.

Do not remove preservation.

## Required behavior

Add a new semantic catalog builder that can consume approved artifacts including `semantic_pack_v1`.

It must:

- parse valid `semantic_pack_v1` artifacts;
- extract terms;
- extract aliases if present;
- extract relations if present;
- preserve unknown safe fields only where reasonable;
- record source artifact IDs;
- produce deterministic ordering;
- produce diagnostics;
- write catalog/report sidecar.

Do not require changing `GeneratorPlanGamePackageAssembler` mapping into GamePackage.

Preferred architecture:

```text
approved artifact set
→ GamePackage assembler remains as-is
→ SemanticCatalogService builds sidecar semantic catalog from the same approved artifact set
→ Package export / workflow writes both package output and semantic sidecar
```

If there is an existing package export pipeline hook where sidecar outputs can be added safely, integrate there.

If integration into package export is too risky for S028, implement service + tests + product smoke and document that UI/export integration is a follow-up.

## Semantic pack input flexibility

Since existing semantic_pack_v1 details may vary, support flexible JSON patterns:

Pattern A:

```json
{
  "terms": [
    {
      "id": "theme/survival",
      "kind": "theme",
      "label": "Survival",
      "aliases": ["survive"]
    }
  ],
  "relations": [
    {
      "source": "location/sky_lantern_outpost",
      "kind": "has_theme",
      "target": "theme/survival"
    }
  ]
}
```

Pattern B:

```json
{
  "semantic": {
    "terms": [],
    "relations": []
  }
}
```

Pattern C:

```json
{
  "themes": ["survival", "occult"],
  "tones": ["mysterious"],
  "dialogueIntents": ["warn", "bargain"]
}
```

For pattern C, generate IDs:

```text
theme/survival
theme/occult
tone/mysterious
dialogue_intent/warn
dialogue_intent/bargain
```

Unknown strings become candidate terms.

## ID normalization

Normalize labels/strings into safe IDs:

- lowercase;
- trim;
- spaces to `_`;
- only `a-z`, `0-9`, `_`, `-`, `/`;
- no empty segments;
- no `.` or `..`;
- no absolute/path-like values;
- max reasonable length, e.g. 128 chars;
- invalid values produce diagnostics and are skipped.

Do not use file-system paths from semantic IDs.

---

# Part F — Semantic generation context preview

Add a deterministic preview that answers:

```text
Given this semantic catalog, what compact context would we send to an LLM in a future generation step?
```

This is not an LLM call.

It produces:

```text
.llmgc/semantic/semantic-generation-context-preview.json
.llmgc/semantic/semantic-generation-context-preview.md
```

## Purpose

The preview should show that semantics reduce LLM load by giving compact project memory:

- top themes;
- active tones;
- candidate terms needing approval;
- dialogue intents;
- quest motifs;
- asset style hints;
- audio mood hints;
- important relations;
- unresolved conflicts.

## Required preview model

Minimum:

```json
{
  "schemaVersion": "1",
  "contextId": "semantic-generation-context-preview",
  "llmPolicy": {
    "llmRequiredFor": [],
    "deterministicSteps": [],
    "maxRecommendedPromptTerms": 80
  },
  "sections": [
    {
      "sectionId": "themes",
      "title": "Themes",
      "termIds": ["theme/survival"]
    }
  ],
  "candidateTerms": [],
  "diagnostics": []
}
```

## LLM minimization policy in preview

The preview must include explicit policy text.

LLM required for:

```text
new creative game concept text
new quest/dialogue prose
new art/audio prompt phrasing when no deterministic template is sufficient
resolving ambiguous user intent when deterministic presets conflict
```

Deterministic without LLM:

```text
ID generation
path generation
slot planning
schema validation
compatibility validation
request planning
fulfillment scanning
archive review/history/comparison
manifest template generation
semantic catalog merge
known-term lookup
basic relation validation
fallback placeholder generation
report rendering
```

This is documentation/data for future agents and UI; it must not call an LLM.

---

# Part G — User-facing procedure documentation

Add a doc:

```text
docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md
```

It must explain in simple product terms:

## Procedure from user point of view

Example:

```text
1. Choose or create a game idea/preset.
2. Combiner builds deterministic plan.
3. LLM is called only for missing creative artifacts.
4. Generated artifacts go to review.
5. User approves/rejects/repairs.
6. Combiner assembles GamePackage deterministically.
7. Combiner exports archive and request plan.
8. Media/code/provider outputs are fulfilled manually or by future explicit providers.
9. Review/history/comparison show what changed.
10. User tests and approves the next controlled vertical slice.
```

## When LLM is used

Explain:

- not for path creation;
- not for copying files;
- not for validation;
- not for balance formulas that can be deterministic/configurable;
- not for placeholder assets;
- not for reports;
- yes for creative content, natural-language variants, concept expansion, quest/dialogue prose, image/audio prompt phrasing where templates are insufficient.

## Expected LLM load tiers

Give rough tiers, not hard guarantees:

Small prototype:

```text
5-20 LLM calls, compact prompts, mainly concepts/NPCs/quests/dialogue seeds.
```

Medium game:

```text
30-150 LLM calls, batched by artifact kind, semantic context reused.
```

Large game:

```text
hundreds of calls, but chunked by region/faction/questline; never one huge monolithic prompt.
```

Huge content library:

```text
requires batching, caching, semantic reuse, deterministic expansion and user-approved generation queues.
```

## How combiner decides LLM is needed

Describe decision rule:

```text
If requested artifact can be produced by deterministic template/rules/preset/library/semantic lookup without quality loss, use deterministic generation.
If it requires original creative text/worldbuilding/dialogue/quest concept/prompt phrasing, schedule an LLM artifact request.
If uncertain, produce a reviewable generation plan instead of calling LLM immediately.
```

---

# Part H — Extensibility policy for future mechanics/formulas/modes

Add to the same doc or separate section.

New mechanics should not always require giant vertical slices.

Design target:

```text
data-first mechanics
formula registry
effect/action DSL
validation contracts
small UI adapters
runtime support only when a truly new runtime primitive is needed
```

Classify future requests into tiers:

Tier 1 — data/config only:

```text
new item stats
new dialogue intents
new quest motifs
new semantic tags
new balancing constants
```

No big vertical slice.

Tier 2 — formula/effect extension:

```text
new damage formula
new reputation formula
new requirement type
new reward type
```

Small/medium slice.

Tier 3 — new systemic mechanic:

```text
crafting network
faction diplomacy
settlement economy
stealth
weather survival
```

Medium/large slice, but should be isolated by contracts.

Tier 4 — new runtime interaction mode:

```text
turn-based tactical combat
real-time action combat
vehicle simulation
colony sim UI
```

Large controlled vertical slice.

The doc must make clear: the architecture should reduce repeated giant slices by making many future desires data/formula/DSL-driven.

---

# Part I — Optional UI/report surface

If low risk, add read-only semantic report visibility to an existing page.

Preferred:

- `Composition Workbench`, or
- `Artifact Review`, or
- a simple docs/report output only.

Do not build a large semantic editor in this slice.

Do not create complex UI if it risks the main service work.

A read-only report file and tests are sufficient for S028.

---

# Part J — Allowed files

You may add/update files in these areas:

```text
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualProviderImportService.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualImportTemplateModels.cs
src/LLMGameCreator.Application/Composition/UnityArchiveManualImportTemplateService.cs

src/LLMGameCreator.Application/Design/Semantics/
src/LLMGameCreator.Application/Design/GeneratorPlans/

src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPageControl.Designer.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewPresenter.cs
src/LLMGameCreator.WinForms/Pages/UnityArchiveReview/UnityArchiveReviewViewState.cs

tests/LLMGameCreator.Tests/Application/UnityArchiveManualProviderImportTests.cs
tests/LLMGameCreator.Tests/Application/UnityArchiveManualImportTemplateTests.cs
tests/LLMGameCreator.Tests/Application/Semantics/
tests/LLMGameCreator.Tests/WinForms/UnityArchiveReviewPresenterTests.cs
tests/LLMGameCreator.Tests/ProductSmoke/

.devflow/scripts/run-product-smoke.ps1
.devflow/CURRENT_RUN.md

docs/PRODUCT_SLICE_028_MANUAL_IMPORT_REPAIR_SEMANTIC_CATALOG_FOUNDATION.md
docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md
docs/PRODUCT_SMOKE_SCENARIOS.md
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
docs/agent-tasks/NEXT_PRODUCT_SLICE/028_MANUAL_IMPORT_REPAIR_SEMANTIC_CATALOG_FOUNDATION.md
```

You may update package export/run docs if needed to describe semantic sidecar output.

If adding new Application files requires csproj edits, stop and report. Do not edit `.csproj`.

---

# Part K — Forbidden files and areas

Do not edit:

```text
src/LLMGameCreator.Runtime/
src/LLMGameCreator.Runtime.Abstractions/
src/LLMGameCreator.GamePackage/
src/LLMGameCreator.Scripting/
src/LLMGameCreator.Infrastructure/
generator-library/
LLMGameCreator.sln
*.csproj
```

Do not change GamePackage schema.

Do not unlock M5.

Do not unlock M6.

Do not implement runtime semantic behavior.

Do not execute providers.

Do not call any LLM.

Do not call ComfyUI.

Do not call Suno.

Do not execute Lua.

Do not execute Unity.

Do not execute Runtime gameplay.

---

# Part L — Required tests

## S027 repair tests

Add/adjust:

```text
UnityArchiveManualImportTemplateServiceTests.EnsureManualImportDirectoryAcceptsManualImport
UnityArchiveManualImportTemplateServiceTests.EnsureManualImportDirectoryRejectsTraversal
UnityArchiveManualProviderImportTests.IdempotentAlreadyImportedDoesNotStoreNewHistorySnapshot
UnityArchiveManualProviderImportTests.ConflictOnlyRunDoesNotStoreNewHistorySnapshot
UnityArchiveManualProviderImportTests.OverwriteChangedBytesStoresNewHistorySnapshot
```

## Semantic catalog tests

Add:

```text
SemanticCatalogServiceTests.BuildsSeedCatalogDeterministically
SemanticCatalogServiceTests.MapsSemanticPackTermsAndRelations
SemanticCatalogServiceTests.UnknownSafeTermsBecomeCandidates
SemanticCatalogServiceTests.InvalidSemanticIdsBecomeDiagnosticsAndAreSkipped
SemanticCatalogServiceTests.DoesNotRequireGamePackageSchemaChange
SemanticCatalogMarkdownRendererTests.RendersTermsRelationsDiagnostics
SemanticGenerationContextPreviewTests.BuildsCompactPreviewWithoutLlm
```

## Product smoke

Add scenario:

```text
semantic-catalog-foundation
```

The smoke should:

1. create an approved artifact set with at least one `semantic_pack_v1`;
2. build semantic catalog;
3. write semantic catalog/report and preview files under a temp project;
4. assert:
   - catalog JSON exists;
   - report MD exists;
   - preview JSON exists;
   - preview MD exists;
   - unknown terms become candidates;
   - no LLM/provider execution.
5. also extend manual import smoke or add focused tests to assert:
   - manual-import folder helper returns success;
   - no-op AlreadyImported run does not increase history count.

Update `run-product-smoke.ps1` with:

```text
semantic-catalog-foundation
```

Do not remove existing smoke scenarios.

---

# Part M — Validation commands

Run:

```powershell
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ManualImport|FullyQualifiedName~UnityArchiveReview"
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~Semantic"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario semantic-catalog-foundation
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\run-product-smoke.ps1 -Scenario unity-archive-manual-import-workflow-ui
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --configuration Debug --filter "FullyQualifiedName~ProductSmoke"
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-devflow-state.ps1
powershell -ExecutionPolicy Bypass -File .\.devflow\scripts\check-all.ps1
```

Also run text audits over changed text files for forbidden path markers and mojibake markers.

Forbidden path markers:

```text
/mnt
/home/oai
sandbox:/
C:\mnt
```

Mojibake examples:

```text
РІ
РЅ
Рµ
Ð
```

Only report actual findings.

---

# Part N — State/docs update

After all checks pass, update:

```text
docs/CURRENT_GENERATOR_STATE.md
docs/CURRENT_GENERATOR_STATE.json
.devflow/CURRENT_RUN.md
```

Set last completed product slice:

```text
slice_id = product_slice_028_manual_import_repair_semantic_catalog_foundation_v1
title = Manual Import Repair + Semantic Catalog Foundation v1
scenario_id = semantic-catalog-foundation
```

Summary must mention:

- manual-import folder helper repaired;
- no-op AlreadyImported import no longer creates extra review-history snapshots;
- semantic_pack_v1 maps into project-local semantic catalog sidecar;
- semantic generation context preview added;
- LLM minimization policy documented;
- M5/M6 remain Locked.

Allowed next tasks should include:

```text
manual_import_workflow_polish_after_user_testing
one_controlled_product_vertical_slice_selection
semantic_catalog_ui_review_and_approval
formula_registry_foundation
```

Do not unlock M5/M6.

---

# Part O — Acceptance criteria

This slice is complete only if:

- `Create/Open manual-import folder` works for `manual-import`.
- Unsafe manual import directory paths are rejected.
- Re-running identical manual import produces `AlreadyImported` without creating a new archive-review history snapshot.
- Conflict-only manual import does not create a new archive-review history snapshot.
- Overwrite changed bytes creates a new archive-review history snapshot.
- `semantic_pack_v1` artifacts can be mapped into semantic catalog sidecar.
- Unknown safe semantic terms become candidates, not hard errors.
- Invalid semantic IDs create diagnostics.
- Semantic generation context preview is deterministic and compact.
- `docs/GENERATION_PROCEDURE_AND_LLM_POLICY.md` exists and explains when LLM is used.
- No LLM/provider/generator/Lua/Unity/Runtime execution is added.
- GamePackage schema is not changed.
- Runtime is not changed.
- M5/M6 remain Locked.
- Product smoke passes.
- `check-all.ps1` passes with 0 unexpected warnings/errors.

---

# Part P — Stop conditions

Stop and report instead of continuing if:

- semantic catalog requires changing GamePackage schema;
- semantic catalog requires Runtime changes;
- semantic catalog requires M5/M6 unlock;
- package export integration requires `.csproj` edits;
- S027 history-spam repair requires a major redesign of review/history services;
- any task requires LLM/provider/generator/Lua/Unity/Runtime execution;
- safe path containment cannot be preserved;
- current repository structure differs enough that allowed file list is insufficient.

---

# Part Q — Final report requirements

Final report must be in Russian.

Include:

- files read;
- files changed;
- S027 repair summary;
- exact fix for `manual-import` folder helper;
- exact rule used to prevent no-op history snapshot spam;
- semantic catalog output paths;
- semantic term kinds/statuses implemented;
- how `semantic_pack_v1` is mapped;
- how unknown terms are handled;
- how invalid terms are handled;
- semantic generation context preview summary;
- LLM minimization policy summary;
- tests run with pass/fail counts;
- product smoke results;
- `check-devflow-state.ps1` result;
- `check-all.ps1` result;
- mojibake/path-marker audit result;
- confirmation that M5/M6 remain Locked;
- confirmation that Runtime, GamePackage schema, generator-library, `.sln`, `*.csproj` were not touched;
- confirmation that no providers, generators, LLMs, Lua, Unity or Runtime gameplay were executed;
- recommendation: ready for user review or needs repair.
