# Goal 083 — Visual / Adult Layer Context Integration & Media Policy Gates

## Repo

https://github.com/kpCat/LLMGameCreator

## Working copy

`C:\Users\endim\LLMGameCreator\`

## Branch

`main`

## Codex reasoning

very high

## Primary objective

Integrate the visual-layer and adult/NSFW visual-layer documentation that was pushed above Goal 082 into the project’s official context/navigation spine, without implementing provider calls, image generation, explicit media assets, Runtime changes, Unity runtime changes, public GamePackage schema changes, or new dependencies.

This goal exists because the adult/visual docs are important design input and must not remain as isolated “docs noise”. They must become indexed, routed, policy-bounded project context for future visual/media pipeline goals.

## Current context

Recent spine:
- Goal 074–081 built the edit-driven campaign/review/package/session/runtime-preview/playthrough spine.
- Goal 082 added Unity Alpha StreamingAssets handoff and a separate Unity probe.
- Goal 082A repaired/strengthened source-format physical-line guard evidence and preserved the separate `adult docs` commit.
- The separate commit `21f2525 adult docs` added visual/adult layer documentation above Goal 082. Treat it as intentional design context, not disposable unrelated changes.

Important boundary:
Adult/NSFW work in this goal is documentation, metadata, policy and future-pipeline routing only. Do not add explicit image prompts, explicit text fixtures, real NSFW assets, generated images, provider calls, or Runtime/Unity adult behavior.

## Required preflight

1. Confirm branch is `main`.
2. Fetch `origin/main`.
3. Confirm current `HEAD` includes:
   - `21f2525 adult docs`
   - `2a74a398 GREEN Goal 082 edit-driven Unity Alpha StreamingAssets handoff`
   - `f26309ba GREEN Goal 082 edit-driven Unity Alpha StreamingAssets handoff`
   - `57025434 GREEN Goal 082A source format physical line repair`
4. Confirm Goal 082 and Goal 082A remain `accepted=false` with manual gates required.
5. Confirm `AlphaRuntimeBootstrap.cs` was not touched after Goal 082/082A.
6. Inspect current dirty state before edits. If user has unrelated untracked or modified files, do not stage/revert them unless they are this Goal 083 task pack or directly allowed Goal 083 outputs.
7. Confirm current source-format evidence is not P0 before doing docs work:
   - no zero-LF source files in guarded Goal082/082A scope;
   - no CR-only source files in guarded Goal082/082A scope;
   - no raw physical one-line C# source files in guarded Goal082/082A scope;
   - max physical line length <= 500.

## Read first

Read these files before editing anything:

- `AGENTS.md`
- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.llmgc/procedural/goal-082a-source-format-physical-line-repair/source-format-physical-line-repair-report.md`
- `.llmgc/procedural/goal-082a-source-format-physical-line-repair/source-format-physical-line-repair-scan.json`
- `docs/agent-tasks/CODEX_TASK_ADULT_VISUAL_LAYER_DOCS_ONLY.md`
- `docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
- `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`

If present, also read:
- `docs/agent-tasks/CODEX_TASK_VISUAL_DETAIL_GENERATOR_CORE.md`
- `docs/agent-tasks/CODEX_TASK_PROCEDURAL_VISUAL_PART_PACK_COMPILER.md`
- any other visual/adaptive quality docs already referenced by `CONTEXT_INDEX.md` or the adult docs manifest.

## Allowed files / areas

You may change only:

- `docs/CONTEXT_INDEX.md`
- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`
- `.devflow/artifact-scope/artifact-scope-policy.json`
- `docs/context/ADULT_VISUAL_LAYER_DOCUMENTATION_MANIFEST.md`
- `docs/context/VISUAL_WORLD_GENERATION_CONTEXT_BRIEF.md`
- `docs/context/METAMODULE_CARRIER_VISUAL_NSFW_CONTEXT_BRIEF.md`
- `docs/proposals/ADULT_VISUAL_LAYER_STRATEGY.md`
- `docs/proposals/CREATURE_VISUAL_GENOME_AND_PRESENTATION.md`
- `docs/proposals/PROCEDURAL_VISUAL_PART_PACKS.md`
- `docs/proposals/VISUAL_PART_PACK_ADULT_EXTENSION.md`
- New docs under `docs/context/` or `docs/proposals/` only if needed for the integration index / roadmap.
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/`
- `docs/agent-tasks/goal-083-visual-adult-layer-context-integration/`

Recommended new docs:
- `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`
- `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`

Recommended Goal 083 evidence:
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-layer-context-integration-report.md`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-doc-inventory.json`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/visual-adult-policy-routing-matrix.json`
- `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/quality-gate-scan.json`

## Forbidden files / areas

Do not change:

- Any C# source file.
- Any Unity `.cs`, `.asmdef`, scene, prefab, project setting, package setting, or build setting.
- `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs`
- Public GamePackage schema.
- Runtime or Runtime.Abstractions.
- Infrastructure provider / LLM / RAG / media provider code.
- Lua / Scripting.
- `generator-library`.
- `.sln`
- `.csproj`
- package lock files.
- Binary media assets.
- Any real NSFW image, fixture, generated media, prompt dump, provider output, or model output.
- External dependencies.

Do not perform branch/merge/rebase/cherry-pick/reset/stash/clean/force-push.

## Exact behavior

### 1. Preserve the adult docs commit as project context

Do not revert or rewrite `21f2525 adult docs`.

Treat it as a deliberate design/context commit.

### 2. Create a visual/adult context integration index

Create or update `docs/context/VISUAL_ADULT_LAYER_CONTEXT_INDEX.md`.

It must summarize and route the current visual/adult docs without duplicating their full content.

Required sections:

- Purpose
- Source documents
- Architectural rules
- Safety / rating boundary
- Safe/public build fallback rules
- Provider quarantine / promotion rules
- GamePackage and asset-catalog source-of-truth rules
- Future implementation sequence
- Stop conditions for future agents

Use careful, non-explicit language. This is a system-design document, not adult content.

### 3. Create a visual media pipeline implementation roadmap

Create or update `docs/proposals/VISUAL_MEDIA_PIPELINE_IMPLEMENTATION_ROADMAP.md`.

It must define a future goal sequence that can be consumed by later Codex tasks.

The roadmap should split future work into stages such as:

- visual asset contract / rating metadata;
- visual part-pack compiler;
- provider candidate quarantine and review;
- safe fallback generation;
- adult/rating-gated extension metadata;
- WinForms review workspace;
- Unity/player consumption of already-approved asset references;
- no runtime provider calls.

Keep this roadmap practical and bounded. Do not ask Codex to generate finished art records or explicit content.

### 4. Update context/state routing

Update `docs/CONTEXT_INDEX.md` so future agents can find the visual/adult context.

Update `docs/FULL_GENERATOR_GOAL_QUEUE.md` so the next visual/media goals are visible as future candidates, but do not replace the active edit-driven gate unless the current state docs already require Goal 083 to be the next handoff.

Update `docs/CURRENT_GENERATOR_STATE.md` and `docs/CURRENT_GENERATOR_STATE.json` to record Goal 083 as produced-for-review / documentation-integration state with `accepted=false` and a manual gate such as:

`visual_adult_layer_context_integration_verification required`

Do not mark Goal 082 or Goal 082A accepted unless current project rules explicitly require accepting them by handoff before Goal 083. If you record a handoff, preserve the previous artifacts as produced-for-review and state the gate transition clearly.

Update the debt register:
- Clear the P3 note that adult/visual docs are unindexed only if the new index and queue routing are complete.
- Add remaining P2/P3 debt for future implementation, such as no visual asset contract yet, no candidate quarantine implementation yet, and no rating-gated export enforcement yet.
- Do not create fake P0/P1 debt.

### 5. Generate deterministic Goal 083 evidence

Create `.llmgc/procedural/goal-083-visual-adult-layer-context-integration/`.

The evidence must include:

#### visual-adult-doc-inventory.json

Include:
- source document paths;
- whether each exists;
- short purpose tags;
- whether it is indexed in `CONTEXT_INDEX.md`;
- whether it is routed in `FULL_GENERATOR_GOAL_QUEUE.md`;
- whether it is policy-bounded;
- whether it contains prohibited implementation requests.

#### visual-adult-policy-routing-matrix.json

Include policy/routing facts such as:
- adult visuals are rating-gated extension metadata, not a separate generator;
- Runtime/Unity Player must not call LLM/media providers;
- provider output must remain candidate/quarantined until reviewed;
- safe/public builds require deterministic safe fallbacks;
- source of truth is GamePackage/manifests/catalogs, not prompts;
- no real NSFW assets are added by this goal;
- no explicit prompt generation is added by this goal.

#### quality-gate-scan.json

Include:
- no C# changed;
- no Unity files changed;
- no project files changed;
- no binary media added;
- no provider integration added;
- no generated image/assets added;
- no explicit prompt dump added;
- adult docs indexed;
- future goals routed;
- artifact-scope ready.

#### visual-adult-layer-context-integration-report.md

Include:
- `implementationStatus`;
- `accepted=false`;
- manual gate;
- docs indexed;
- docs routed;
- policy boundaries;
- future goal sequence;
- remaining debt;
- deterministic report hash.

### 6. Content boundaries

The docs may discuss architecture and policy for adult-capable games, but they must not include explicit sexual descriptions, explicit generation prompts, real NSFW asset examples, or media-provider prompt recipes.

Use neutral metadata terms such as:

- `rating`
- `adultEnabled`
- `safeFallbackRequired`
- `candidateQuarantine`
- `reviewStatus`
- `exportPolicy`
- `assetSlot`
- `approvedAssetRef`

Do not write erotic prose.

Do not add images.

### 7. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` for Goal 083 if needed.

Artifact scope must allow:
- Goal 083 task pack;
- Goal 083 docs outputs;
- Goal 083 `.llmgc/procedural` evidence;
- docs quartet/debt register changes.

It must not allow code/Unity/provider/schema changes.

## Quality gate

Goal 083 is GREEN only if:

- No forbidden files changed.
- No C# files changed.
- No Unity files changed.
- No project files changed.
- No binary media assets added.
- No real NSFW fixtures or generated assets added.
- Adult/visual docs are indexed and routed.
- Policy boundaries are explicit.
- Future implementation goals are practical and bounded.
- Evidence JSON/Markdown exists and is deterministic.
- `dotnet build` still passes.
- CurrentState docs tests pass.
- `check-all.ps1` passes or any failure is proven unrelated and recorded honestly.
- artifact-scope check passes for Goal 083.

## Validation commands

Run:

```powershell
dotnet restore .\LLMGameCreator.sln
dotnet build .\LLMGameCreator.sln --no-restore
dotnet test tests\LLMGameCreator.Tests\LLMGameCreator.Tests.csproj --filter CurrentState
.\.devflow\scripts\check-all.ps1
.\.devflow\scripts\check-artifact-scope.ps1 -Scenario "goal-083-visual-adult-layer-context-integration"
git diff --check
git diff --cached --check
```

Also run a text hygiene scan over changed Markdown/JSON files for:
- mojibake markers;
- absolute local paths;
- timestamps/heavy logs in tracked evidence;
- accidental prompt dumps;
- accidental binary/media additions.

## Stop / block conditions

Return BLOCKED if:

- Integrating the docs requires C# source changes.
- Integrating the docs requires Unity changes.
- Integrating the docs requires public schema changes.
- Integrating the docs requires provider integration.
- Integrating the docs requires real NSFW media assets or explicit prompt recipes.
- Existing docs conflict so badly that a safe policy boundary cannot be stated honestly.
- artifact scope cannot be satisfied without broadening into forbidden zones.

Return FAILED if:

- Build/tests regress due to this goal and cannot be fixed within allowed docs/artifact files.
- JSON evidence cannot be made valid/deterministic.
- The final worktree cannot be made clean without touching forbidden files.

## Final report format

Report:

- Final status: GREEN / BLOCKED / FAILED.
- Latest commit before work.
- Latest commit after work.
- Push status.
- Preflight summary.
- Files changed.
- Adult docs commit handling.
- Documents indexed.
- New/updated docs.
- Policy boundaries added.
- Future implementation sequence added.
- Evidence artifacts created.
- Validation results.
- Artifact scope result.
- Evidence hygiene result.
- Remaining P2/P3 debt.
- Final git status.
- Git commands used and why.

## Mandatory commit / push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 083 visual adult layer context integration`
- `BLOCKED Goal 083 visual adult layer context integration`
- `FAILED Goal 083 visual adult layer context integration`

Do not rewrite history.
