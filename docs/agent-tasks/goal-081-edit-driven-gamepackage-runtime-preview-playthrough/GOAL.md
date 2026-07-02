# Goal 081 — Edit-Driven GamePackage Runtime Preview Playthrough

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Objective

Consume the real Goal 080 disk-backed projected GamePackage as player/session input and produce a deterministic runtime-preview playthrough transcript/hash-chain using existing Application/GamePackage/runtime-preview/interaction-preview seams.

This must be a practical feature step, not a paper registry. Goal 080 proved that the projected GamePackage can be read and validated. Goal 081 must prove that a player-facing playthrough can be built from that package and replayed deterministically with coverage over Goal077 targets and Goal078 actions.

Do not change public GamePackage schema, Runtime, Unity, providers/LLM/RAG/media, Lua/Scripting, generator-library, `.sln`, `.csproj`, or lockfiles. If a real playthrough bridge cannot be built without those forbidden changes, return BLOCKED with evidence and push the BLOCKED commit.

## Required behavior

1. Preflight
   - Follow `read-first.md`.
   - Confirm branch `main` and current `origin/main` ancestry.
   - Confirm Goal 080 is present, GREEN, accepted=false, and handoff is recorded before Goal 081.
   - Record the Goal 080 commit-message-order mismatch as P3 process debt only; do not rewrite history.
   - Confirm `AlphaRuntimeBootstrap.cs` baseline line count/hash and keep it read-only.

2. New BCL-only Application seam
   - Add a new namespace under:
     `src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewPlaythrough/`
   - Suggested shape, adjust names only if needed:
     - `EditDrivenGamePackageRuntimePreviewPlaythroughEvidenceService.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughModels.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughHash.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughCommandBuilder.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughReplayEngine.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughQualityGateScanner.cs`
     - `EditDrivenGamePackageRuntimePreviewPlaythroughReportRenderer.cs`
   - Consume actual Goal 080 artifacts from disk:
     - projected GamePackage `package.json`;
     - projected package index;
     - player-readable bridge index;
     - source targets;
     - runtime-preview bridge proof;
     - runtime-preview negative proof;
     - quality gate scan.
   - Re-read `projected-gamepackage/package.json` from disk. Do not rely on in-memory or hardcoded success.
   - Build a deterministic player command script from package contents and Goal 080 bridge index. Commands should be data-derived and stable, for example:
     - load package;
     - start at `startMapId`;
     - inspect runtime-preview region/map;
     - inspect/collect each projected target item;
     - inspect linked NPC/dialogue/quest/mechanic if present;
     - cover all Goal077 targets;
     - cover all Goal078 actions through their projected target linkage;
     - final coverage/assert command.
   - Run those commands through a deterministic playthrough/replay engine in the new Goal 081 namespace, reusing existing Application validation/runtime-preview seams where available.
   - Produce a state-hash chain proving:
     - initial package read state;
     - command-script state;
     - replay transcript state;
     - final coverage state;
     - replay re-run final hash matches the first final hash.
   - The playthrough must be package-driven: negative tests must fail if the package/index/source-target linkage is missing/tampered.

3. Artifacts
   Write Goal 081 artifacts under:
   `.llmgc/procedural/goal-081-edit-driven-gamepackage-runtime-preview-playthrough/`

   Required artifacts:
   - `edit-driven-gamepackage-runtime-preview-playthrough-report.md`
   - `playthrough-command-script.json`
   - `playthrough-transcript.json`
   - `playthrough-state-hash-chain.json`
   - `playthrough-coverage-ledger.json`
   - `package-read-proof.json`
   - `playthrough-negative-proof.json`
   - `winforms-binding-inventory.json`
   - `quality-gate-scan.json`
   - `source-artifact-manifest.json`

   Requirements:
   - Report must be GREEN only if all proofs pass.
   - Report must keep `accepted=false` and manual gate `edit_driven_gamepackage_runtime_preview_playthrough_verification required`.
   - Evidence must be deterministic and path-relative.
   - No absolute local paths, timestamps, scratch/tamper files, or heavy logs.

4. Negative proof
   Include at least these rejection scenarios:
   - missing projected GamePackage payload;
   - tampered projected GamePackage payload;
   - missing player-readable bridge index;
   - command script references a nonexistent target;
   - replay order mismatch;
   - fake success without package read;
   - source Goal080 lineage/hash mismatch.

   Each scenario must include expected/actual status and diagnostics. A boolean-only `passed=true` is not enough.

5. WinForms workspace
   - Add a separate UserControl:
     `CampaignGamePackageRuntimePreviewPlaythroughControl.cs` and Designer.
   - Add it as a separate tab in `CampaignAuthoringReviewWorkspacePageControl`.
   - Bind it through real parent activation using the Goal 081 Application seam result.
   - Keep parent page bounded; avoid god-form growth. If needed, do only a tiny local extraction to keep activation readable, not a broad UI refactor.
   - Update `CompositionRoot.cs` only for registration/wiring if needed.

6. Tests
   Add focused tests under:
   `tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewPlaythrough/`

   Required coverage:
   - service consumes real Goal080 artifacts and writes all Goal081 artifacts;
   - command script is deterministic and covers 9 rows, 18 targets, and 57 actions unless upstream evidence changes;
   - transcript/replay final hash is stable;
   - negative scenarios reject missing/tampered/fake inputs;
   - quality gate rejects report-only smoke and source-format problems;
   - WinForms parent activation binds the new playthrough control.

   Add product smoke:
   `tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewPlaythroughProductSmokeTests.cs`

   Product smoke must read actual Goal080 projected package artifacts from disk and verify behavior. It must not only check `report=true`.

7. Current state and docs
   - Record Goal080 handoff before Goal081.
   - Update docs quartet:
     - `docs/CURRENT_GENERATOR_STATE.md`
     - `docs/CURRENT_GENERATOR_STATE.json`
     - `docs/CONTEXT_INDEX.md`
     - `docs/FULL_GENERATOR_GOAL_QUEUE.md`
   - Update debt register only for real P2/P3 debt.
   - Keep Goal080 accepted=false unless explicitly handoff-accepted for continuation; do not mark manual verification as fully accepted by user.
   - Do not erase Goal072 historical BLOCKED context.

8. Artifact scope
   - Add a scenario allowlist for:
     `goal-081-edit-driven-gamepackage-runtime-preview-playthrough`
   - Keep it tight: only Goal081 task pack, Goal081 artifacts, new Application namespace, new tests/product smoke, bounded WinForms files, docs quartet, debt register, and policy itself.

## Quality gate

Must pass:

- No minified/one-line `.cs` files.
- Raw-byte LF/CR guard remains active.
- Max C# line length <= 500.
- No new C# file over 1000 lines.
- Parent workspace remains bounded and separately composed.
- No forbidden area changes.
- Product smoke reads real package artifacts and validates behavior.
- `AlphaRuntimeBootstrap.cs` hash/line count unchanged.
- No absolute local paths/timestamps/heavy logs/scratch files in evidence.

## Validation

Run everything from `validation.md`.

## Stop / block conditions

Return BLOCKED if:

- The playthrough cannot consume Goal080 projected GamePackage without public schema, Runtime, Unity, provider/LLM/RAG/media, Lua/Scripting, generator-library, `.sln`, `.csproj`, or lockfile changes.
- Existing runtime-preview/interaction-preview seams are too weak to support a real package-driven playthrough and the only alternative is a fake report-only proof.
- check-all fails due to Goal081 changes and cannot be repaired inside allowed files.
- artifact-scope cannot be made tight without broad allowlists.

Return FAILED if:

- The repo cannot compile due to Goal081 changes.
- Focused/product smoke tests fail due to Goal081 changes and no bounded repair is possible.

## Mandatory commit / push policy

Always commit and push to `origin/main` even for GREEN/BLOCKED/FAILED.

Commit message must start with the honest status:

- `GREEN Goal 081 edit-driven GamePackage runtime preview playthrough`
- `BLOCKED Goal 081 edit-driven GamePackage runtime preview playthrough`
- `FAILED Goal 081 edit-driven GamePackage runtime preview playthrough`

Do not use a reversed status suffix such as `Goal 081 ... GREEN`.
