# Goal 080 — Edit-Driven GamePackage Runtime Preview Bridge

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Primary objective

Build the first edit-driven bridge from the Goal 077/078 disk-backed review package/playable session spine into a real existing-schema GamePackage/runtime-preview handoff.

Goal 080 is GREEN only if it:
1. Consumes real Goal 077/078/079/079A artifacts from disk.
2. Produces a deterministic disk-backed package projection using existing public GamePackage contracts/schema only.
3. Reads that projected package back from disk and validates file/hash/coverage integrity.
4. Proves a runtime-preview/player-facing bridge through existing package/runtime-preview/projection APIs or existing Application seams.
5. Adds a bounded separate WinForms dashboard tab bound through Campaign Authoring Review Workspace parent activation.
6. Rejects missing/tampered/fake-success scenarios.
7. Leaves public schema, Runtime, Unity, providers, Lua, generator-library and project files untouched.

This must not be another sidecar-only registry/proof layer. If the existing runtime-preview bridge cannot be built without forbidden changes, return BLOCKED honestly.

## Required preflight

1. Confirm current branch is `main`.
2. Fetch `origin/main` and record current top commit.
3. Confirm `7c252d5` / Goal 079A is in current history or resolve the current equivalent top commit.
4. Confirm Goal 079A evidence exists:
   - `.llmgc/procedural/goal-079a-source-format-line-ending-guard/source-format-line-ending-guard-report.md`
   - `.llmgc/procedural/goal-079a-source-format-line-ending-guard/source-format-line-ending-guard-scan.json`
5. Confirm Goal 079A remains `accepted=false`, `implementationStatus=GREEN`.
6. Record handoff in docs before Goal 080:
   - `source_format_line_ending_guard_verification passed before Goal 080`
   - also record that Goal 079 quality consolidation is accepted for continuation after the Goal 079A source-format hotfix, without rewriting historical evidence.
7. Confirm Goal 077/078 review package and playable session artifacts exist and are GREEN/accepted=false.
8. Baseline source health using the Goal 079A raw-byte metrics:
   - zero-LF C# count
   - CR-only C# count
   - raw physical one-line C# count
   - max raw physical line length
   - logical max line length
   - minified count
   - files over 1000 lines
9. Confirm `AlphaRuntimeBootstrap.cs` line count/hash and do not change it.

## Required implementation

### 1. New BCL-only Application seam

Create a new namespace/folder:

`src/LLMGameCreator.Application/Design/EditDrivenGamePackageRuntimePreviewBridge/`

Use BCL-only code. Do not add dependencies.

Suggested classes, but adapt names if the existing project conventions require it:

- `EditDrivenGamePackageRuntimePreviewBridgeEvidenceService`
- `EditDrivenGamePackageRuntimePreviewBridgeModels`
- `EditDrivenGamePackageRuntimePreviewBridgeHash`
- `EditDrivenGamePackageRuntimePreviewBridgeReadValidator`
- `EditDrivenGamePackageRuntimePreviewBridgeQualityGateScanner`
- optional narrow renderer/helper classes if a single file would approach 800 lines

Hard limits:
- No new C# file over 1000 lines.
- Prefer splitting before 750-800 lines.
- Max C# line length <= 500, target <= 260.
- Do not create minified/one-physical-line files.
- Raw-byte LF/CR metrics must be recorded.

### 2. Consume real Goal 077/078 package/session artifacts

Load and validate from disk:

- Goal 077 report.
- Goal 077 `review-package/manifest.json`.
- Goal 077 `review-package/package-index.json`.
- Goal 077 `review-package/player-readable-index.json`.
- Goal 077 package ledger.
- All 18 Goal 077 target JSON payloads.
- Goal 078 playable session report.
- Goal 078 session replay/state/action proof artifacts.
- Goal 079 quality consolidation report/quality scan.
- Goal 079A source-format guard report/scan.

Do not duplicate the old data by hardcoding constants. The service must read the artifacts/files and fail if required files are missing, hashes mismatch, row/target/action coverage is incomplete, or the prior gates are not GREEN/accepted=false as expected.

### 3. Produce a deterministic projected GamePackage/review package

Under:

`.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/`

produce a disk-backed package projection using existing public GamePackage schema/contracts only.

Expected output should include, as appropriate for the existing schema discovered during read-first:

- package/project metadata
- world/map/region entrypoint or equivalent
- entities/NPCs/items/interactions/quests/objectives derived from the 9 rows / 18 targets
- a deterministic start point / initial runtime-preview state contract
- command/action plan references derived from Goal 078
- package index / player-readable bridge index if the existing schema supports it
- validation report / projection manifest
- file ledger with sha256 and byte counts

If the existing schema cannot represent these concepts without changes, do not modify the schema. Return BLOCKED with exact missing schema/API facts.

### 4. Runtime-preview/player-facing bridge proof

Create a real bridge proof that reads the projected package from disk and drives an existing preview/projection/runtime-facing path.

Preferred order:
1. Use existing Application GamePackage validation/materialization/runtime-preview services if present.
2. Use existing Runtime Preview projection path if it can be invoked without UI automation and without changing Runtime/Runtime.Abstractions.
3. If only a narrower existing Application preview contract exists, use it, but it must read projected package files and validate actual rows/targets/actions.
4. Do not implement a fake preview that only reads the Goal 080 manifest and returns success.

The proof must record:
- projected package root
- file ledger hash
- package validation status
- runtime-preview/player-facing bridge status
- rows covered
- targets covered
- actions covered
- initial state hash
- post-load state hash
- post-action/replay state hash
- save/load or replay hash if an existing path supports it
- diagnostics

### 5. Negative proof

Produce a negative proof artifact showing all relevant failure modes are rejected:

- missing projected package manifest
- tampered projected target/payload
- projected package index references a missing file
- runtime-preview bridge tries to report success without reading package payloads
- invalid package id or mismatched source Goal 077/078 hash
- action/target mismatch from Goal 078 command plan

Do not leave tamper scratch files in tracked evidence.

### 6. WinForms workspace tab

Add a separate UserControl tab to Campaign Authoring Review Workspace, for example:

- `CampaignGamePackageRuntimePreviewBridgeControl.cs`
- `CampaignGamePackageRuntimePreviewBridgeControl.Designer.cs`

The parent page should:
- own Application service activation/loading
- call the new service during normal activation
- bind the result to the child control
- keep existing Goal 074-079 tabs intact
- keep child tabs as separate UserControls

The child control should show at least:
- package projection status
- projected package root/relative path
- row/target/action coverage
- runtime-preview bridge status
- hashes
- negative proof summary
- quality/source-format status

Do not turn the parent page into a god-form. If parent page growth becomes uncomfortable, add narrow private helper methods only inside allowed files.

### 7. Evidence artifacts

Write deterministic artifacts under:

`.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/`

Required artifact names:

- `edit-driven-gamepackage-runtime-preview-bridge-report.md`
- `projected-gamepackage-manifest.json`
- `projected-gamepackage-file-ledger.json`
- `runtime-preview-bridge-proof.json`
- `runtime-preview-negative-proof.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

If additional projected package files are needed, place them under:

`.llmgc/procedural/goal-080-edit-driven-gamepackage-runtime-preview-bridge/projected-gamepackage/**`

The report must keep:
- `accepted=false`
- `implementationStatus=GREEN` only if all required behavior passes
- manual gate: `edit_driven_gamepackage_runtime_preview_bridge_verification required`

### 8. Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/EditDrivenGamePackageRuntimePreviewBridge/`

Required test coverage:
- service builds deterministic artifacts from real Goal 077/078 inputs
- projected package file ledger covers all generated package files
- existing-schema validation/bridge proof passes
- negative proof rejects missing/tampered/fake-success cases
- raw source-format scanner rejects synthetic CR-only and zero-LF/one-physical-line samples, or delegates to Goal 079A scanner with explicit proof
- WinForms parent activation binds the new child control result, not only standalone control bind

Add product smoke:

`tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenGamePackageRuntimePreviewBridgeProductSmokeTests.cs`

Product smoke must:
- read disk artifacts
- read projected package files
- invoke the real bridge path
- check row/target/action coverage
- check negative proof
- fail if the bridge is reduced to report-only success

### 9. Docs/state updates

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md`

Required state:
- record Goal 079A handoff before Goal 080
- record Goal 080 produced for review
- keep Goal 080 `accepted=false`
- keep Goal 072 historical BLOCKED evidence unchanged
- keep AlphaRuntimeBootstrap size as P2/P3 debt, not touched
- preserve c8343e8 adaptive docs debt as P3 unless the docs are naturally indexed without scope expansion

### 10. Artifact scope

Update `.devflow/artifact-scope/artifact-scope-policy.json` with scenario:

`goal-080-edit-driven-gamepackage-runtime-preview-bridge`

Allow only the paths in `allowed-files.md`.

## Quality gate

GREEN requires all of the following:

- Real disk-backed projected package files exist.
- Package projection is based on Goal 077/078 artifacts read from disk.
- Existing public schema/contracts are used; no public schema mutation.
- Runtime-preview/player-facing bridge proof reads generated package files.
- Missing/tampered/fake-success negative proof rejects.
- WinForms parent activation binds the new child control.
- No forbidden areas changed.
- No new minified/one-line/CR-only/zero-LF C# source files.
- Max C# line length <= 500.
- No new file over 1000 lines.
- Product smoke proves behavior, not just `report=true`.
- No absolute local paths, timestamps, heavy logs or scratch tamper files in tracked evidence.
- `check-all.ps1` passes.
- Artifact scope passes.

## BLOCKED / FAILED conditions

Return BLOCKED if:
- a valid existing-schema projected GamePackage cannot be produced without schema changes;
- the existing runtime-preview/player-facing path cannot load/read the projected package without forbidden Runtime/Runtime.Abstractions/Unity changes;
- the bridge would be report-only or fake success;
- required prior artifacts are missing or internally inconsistent before your changes;
- fixing the issue requires forbidden files.

Return FAILED if:
- compilation breaks and cannot be repaired within allowed files;
- tests regress due to your changes and cannot be repaired within allowed files;
- source-format P0 returns and cannot be repaired within allowed files.

## Mandatory commit/push policy

Always commit and push to `origin/main` even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status:

- `GREEN Goal 080 edit-driven gamepackage runtime preview bridge`
- `BLOCKED Goal 080 edit-driven gamepackage runtime preview bridge`
- `FAILED Goal 080 edit-driven gamepackage runtime preview bridge`
