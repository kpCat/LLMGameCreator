# Goal 082 — Edit-Driven Unity Alpha StreamingAssets Handoff

Repo URL:
https://github.com/kpCat/LLMGameCreator

Working copy:
C:\Users\endim\LLMGameCreator\

Branch:
main

Codex reasoning:
very high

## Objective

Consume the real Goal 080 projected GamePackage and Goal 081 runtime-preview playthrough artifacts, then produce a bounded Unity Alpha StreamingAssets handoff that the Unity side can read without calling LLM/provider/media systems and without modifying public GamePackage schema, Runtime, Lua, generator-library, or existing Unity bootstrap code.

This goal must move beyond a report-only proof:

- create a disk-backed Unity handoff payload under `.llmgc/procedural/goal-082-edit-driven-unity-alpha-streamingassets-handoff/`;
- mirror the bounded player-facing payload into `unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/EditDrivenGoal082/`;
- add one small Unity probe script, `EditDrivenGamePackageHandoffProbe.cs`, that can read the StreamingAssets payload, validate expected files/markers/hashes, and expose a compact probe result for manual Unity inspection;
- add Application-side simulation/tests that read the exact mirrored StreamingAssets payload and reject missing/tampered/fake-success cases;
- add a bounded WinForms tab to the existing Campaign Authoring Review Workspace showing the Goal082 handoff state.

Do not mutate `AlphaRuntimeBootstrap.cs`. It is already P2 debt and must remain read-only/no-change in this goal.

## Preflight requirements

1. Confirm current branch is `main`.
2. Fetch `origin/main` and confirm current HEAD matches or is based on the latest `origin/main` before editing.
3. Confirm Goal 081 exists in history and its artifacts are present:
   - `.llmgc/procedural/goal-081-edit-driven-gamepackage-runtime-preview-playthrough/edit-driven-gamepackage-runtime-preview-playthrough-report.md`
   - package read proof, command script, transcript, state hash chain, coverage ledger, negative proof, quality gate.
4. Record Goal 081 acceptance by handoff before Goal 082 in current-state docs, but do not rewrite Goal 081 artifact evidence: it must remain `accepted=false` and manually gated.
5. Confirm Goal 080 projected GamePackage exists and validates in its own evidence.
6. Record baseline line count and SHA256 for `unity/LLMGameCreatorAlpha/Assets/Scripts/AlphaRuntimeBootstrap.cs` and verify it stays unchanged.
7. Note the P3 process debt that Goal 080 commit message was not in the preferred `GREEN Goal XXX ...` prefix form, but do not rewrite history.

## Required behavior

### 1. Application seam

Create a new BCL-only Application seam under:

`src/LLMGameCreator.Application/Design/EditDrivenUnityAlphaStreamingAssetsHandoff/`

Suggested files, but split as needed to keep files readable:

- `EditDrivenUnityAlphaStreamingAssetsHandoffEvidenceService.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffModels.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffHash.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffPayloadBuilder.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffReadValidator.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffQualityGateScanner.cs`
- `EditDrivenUnityAlphaStreamingAssetsHandoffReportRenderer.cs`

The service must consume real Goal 080 and Goal 081 artifacts, not hardcoded success markers:

- Goal 080 report;
- Goal 080 projected package/package index/validation report/runtime preview proof;
- Goal 081 report;
- Goal 081 package read proof;
- Goal 081 command script;
- Goal 081 transcript;
- Goal 081 state hash chain;
- Goal 081 coverage ledger;
- Goal 081 negative proof.

It must produce deterministic Goal082 artifacts under:

`.llmgc/procedural/goal-082-edit-driven-unity-alpha-streamingassets-handoff/`

Required artifacts:

- `edit-driven-unity-alpha-streamingassets-handoff-report.md`
- `unity-streamingassets-handoff-manifest.json`
- `unity-streamingassets-file-ledger.json`
- `unity-probe-read-proof.json`
- `unity-probe-negative-proof.json`
- `unity-probe-command-transcript-proof.json`
- `winforms-binding-inventory.json`
- `quality-gate-scan.json`
- `source-artifact-manifest.json`

The report must remain:

- `implementationStatus: GREEN` only if all proof gates pass;
- `accepted: false`;
- manual gate required: `edit_driven_unity_alpha_streamingassets_handoff_verification`.

### 2. Unity StreamingAssets payload

Mirror a bounded player-facing payload into:

`unity/LLMGameCreatorAlpha/Assets/StreamingAssets/LLMGameCreator/EditDrivenGoal082/`

Payload must be small and deterministic. It should include enough data for a Unity-side probe to verify the current package/playthrough without embedding huge logs:

- `handoff-manifest.json`
- `projected-package-index.json` or a compact subset/index pointing to the projected package identity/hash/counters;
- `playthrough-command-index.json` or compact command coverage summary;
- `playthrough-transcript-index.json` or compact transcript coverage summary;
- `expected-hashes.json`
- optional `README.md` explaining manual Unity inspection.

Do not copy huge generated logs. If a file is large, store a compact index with SHA256 and counters instead of full content.

The Application-side product smoke must read the exact mirrored StreamingAssets files and validate:

- package hash matches Goal 080;
- command/transcript/state hashes match Goal 081;
- row/target/action/command counts match expected values;
- required Unity payload files are present;
- fake success without reading the payload is rejected.

### 3. Unity probe script

Add exactly one new Unity script unless a strong reason is documented:

`unity/LLMGameCreatorAlpha/Assets/Scripts/EditDrivenGamePackageHandoffProbe.cs`

The probe must be small, readable, and independent from `AlphaRuntimeBootstrap.cs`. It should:

- use `Application.streamingAssetsPath`;
- look for `LLMGameCreator/EditDrivenGoal082/handoff-manifest.json`;
- read required payload files;
- compute or compare expected hashes/counters where practical with Unity-compatible BCL APIs;
- expose a simple result object/string/status for manual scene attachment or debug inspection;
- fail closed when required files are missing or tampered;
- not call LLM, provider, media, Runtime, or editor-only APIs.

Do not wire it into `AlphaRuntimeBootstrap.cs`. This is a probe/handoff contract first, not a broad Unity refactor.

If adding a Unity script is not possible without project-file changes or broad Unity edits, return BLOCKED and explain exactly why.

### 4. WinForms workspace tab

Add a separate UserControl under:

`src/LLMGameCreator.WinForms/Pages/CampaignAuthoringReviewWorkspace/`

Suggested name:

- `CampaignUnityAlphaStreamingAssetsHandoffControl.cs`
- `CampaignUnityAlphaStreamingAssetsHandoffControl.Designer.cs`

Integrate it into `CampaignAuthoringReviewWorkspacePageControl` as a separate tab. The parent page may load the new Application seam during activation and pass the built result into the child control. Keep the parent page bounded; do not turn it into a god-form.

The tab should show:

- Goal082 status/gate/accepted;
- StreamingAssets relative root;
- payload file count;
- package/command/transcript/state hashes;
- Unity probe read proof status;
- negative proof status;
- quality status.

### 5. Tests

Add focused tests under:

`tests/LLMGameCreator.Tests/Application/EditDrivenUnityAlphaStreamingAssetsHandoff/`

Required coverage:

- builds deterministic handoff artifacts from real Goal 080/081 inputs;
- mirrored StreamingAssets payload can be read and validated;
- missing manifest is rejected;
- tampered payload hash is rejected;
- fake success without payload read is rejected;
- Unity probe source references the expected StreamingAssets root and does not reference `AlphaRuntimeBootstrap` as an integration dependency;
- parent WinForms page binds Goal082 result into the separate handoff control;
- quality scanner catches minified/CR-only/zero-LF source and detects any accidental `AlphaRuntimeBootstrap.cs` modification.

Add exact product smoke:

`tests/LLMGameCreator.Tests/ProductSmoke/EditDrivenUnityAlphaStreamingAssetsHandoffProductSmokeTests.cs`

The product smoke must read real artifacts and mirrored Unity payload. Do not use report-only `passed=true` checks.

### 6. Quality and source health

Quality gate must check and report:

- max C# line length <= 500;
- no C# file > 1000 lines;
- no minified source;
- no zero-LF or CR-only source;
- synthetic CR-only/zero-LF samples rejected;
- parent workspace line count;
- `AlphaRuntimeBootstrap.cs` before/after hash and line count unchanged;
- Unity probe source is below 300 lines and does not depend on runtime/provider/LLM/media systems;
- no absolute local paths/timestamps/heavy logs in tracked Goal082 evidence;
- no scratch/tamper files in tracked artifacts.

### 7. Docs and state

Update:

- `docs/CURRENT_GENERATOR_STATE.md`
- `docs/CURRENT_GENERATOR_STATE.json`
- `docs/CONTEXT_INDEX.md`
- `docs/FULL_GENERATOR_GOAL_QUEUE.md`
- `docs/technical-debt/GENERATOR_SPINE_QUALITY_DEBT_REGISTER.md` if new P2/P3 debt is found.

State rules:

- Goal 081 must be recorded as accepted by handoff before Goal 082.
- Goal 081 artifact evidence must remain `accepted=false`.
- Goal 082 must be produced for review with `accepted=false`.
- Do not mark Goal 082 accepted/passed.
- Preserve Goal 072 historical BLOCKED evidence.
- Preserve Goal 031/032 produced-for-review/not-passed status if present.

Update `.devflow/artifact-scope/artifact-scope-policy.json` with a Goal082 scenario allowlist.

## Stop / block conditions

Return BLOCKED if:

- a real Unity StreamingAssets handoff cannot be produced without touching forbidden public schema/Runtime/provider/Lua/generator-library/project files;
- `AlphaRuntimeBootstrap.cs` must be modified to prove the handoff;
- Unity probe requires external dependencies or project file changes;
- mirrored StreamingAssets payload would require tracking heavy logs or excessive generated content;
- GamePackage/runtime-preview evidence cannot be tied back to Goal 080/081 hashes;
- validation fails for caused reasons that cannot be repaired inside allowed files.

Return FAILED if the repo is left uncompilable or tests regress due to the changes and no bounded repair is possible.

## Mandatory commit / push policy

Always commit and push to `origin/main`, even for GREEN/BLOCKED/FAILED.

Commit message must honestly reflect status and use the preferred prefix form:

- `GREEN Goal 082 edit-driven Unity Alpha StreamingAssets handoff`
- `BLOCKED Goal 082 edit-driven Unity Alpha StreamingAssets handoff`
- `FAILED Goal 082 edit-driven Unity Alpha StreamingAssets handoff`
