# Release Risk Register

Status: Goal 097 planning register
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

## P0 Release Blockers

Goal 110 review note: the offline geoworld Alpha manual acceptance gate supplies a deterministic checklist, result template and release gate dashboard for the existing Goal109 export package. It keeps manual acceptance pending and does not close P0 release blockers until user acceptance and later player/export release proof are completed.

Goal 111 review note: the manual-result intake bridge reads the Goal110 package and exposes `BLOCKED_PENDING_MANUAL_RESULT` until a real human result JSON is supplied. Even a future `GREEN_ACCEPTABLE_CANDIDATE` result remains a candidate for explicit human gate decision, not final release or Codex acceptance.

Goal 112 review note: the acceptance operator pack and RC readiness dashboard expose `OPERATOR_READY_PENDING_HUMAN_RUN` over Goal110/Goal111 and tell the human where to place the real result JSON. It does not close release blockers, does not fabricate acceptance, and does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.

Goal 113 review note: the manual-result workbench exposes `WORKBENCH_READY_PENDING_HUMAN_RESULT`, the Goal110 required steps, Goal111/Goal112 statuses, the preferred `.llmgc/manual` result path and a safe draft/template outside `.llmgc/manual/**`. It does not close release blockers, does not fabricate or commit a real manual result, and does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.

Goal 114 review note: the Unity Safe Mode compile hotfix removes the reported Unity helper compile blockers and records source-scan evidence, but it does not close release blockers, does not fabricate or commit a real manual result, and does not mark the Alpha manual gate accepted. Manual acceptance still requires a human-created `.llmgc/manual/**` result and explicit gate decision.

Goal 115 review note: the real local human result now validates as `GREEN_ACCEPTABLE_CANDIDATE` with 12/12 required steps passed and manualResultSha256 `8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`. This still does not close release blockers or mark Alpha accepted; it is evidence for explicit human gate decision only, and the `.llmgc/manual/**` input remains uncommitted local human input.

Goal 116 review note: the explicit human gate decision is recorded as `ACCEPTED_BY_HUMAN` for `offline_geoworld_alpha_manual_acceptance_verification` using Goal115 GREEN candidate evidence. This closes only the manual gate decision; release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/release-packaging approval.

Goal 117 review note: the continuation matrix recommends `accepted_alpha_baseline_review` / `goal-118-offline-geoworld-accepted-alpha-baseline-review` and keeps `doNotStartAutomatically=true`. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/release-packaging approval.

Goal 118 review note: the accepted Alpha baseline review package records baselineId `offline_geoworld_alpha_accepted_baseline_v1`, acceptedBaselineReady=true and recommendedNextDecision=`EXPLICIT_NEXT_LANE_SELECTION` over Goal098-117 evidence. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 119 review note: the accepted Alpha Unity playable projection entrypoint records Unity menu path `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection`, generated root `__LLMGC_AcceptedAlphaPlayableProjection__`, script inventory, smoke plan and negative proof over the Goal118 accepted baseline. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 119A review note: the accepted Alpha Unity material warning hotfix removes the edit-mode projection marker material-instantiation warning and adds batchmode/source/log guard evidence for the same Goal119 menu route. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 120 review note: the accepted Alpha projection usability and cleanup pass adds descriptor-backed selection, a visible legend, a Goal120 batchmode usability smoke and a bounded Unity editor-noise cleanup script for the same accepted Alpha route. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 121 review note: the accepted Alpha interaction drilldown and one-click verification pass reduces manual Unity checking to the same accepted Alpha menu route plus `Run Full Projection Verification`, adds selected marker details, interaction/action preview, objective/replay details, compact event log and batchmode full verification proof. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 122 review note: the accepted Alpha projection action-loop and window-polish pass keeps the same one-button Unity verification route, adds projection-local Preview/Apply/Reset state and makes the EditorWindow readable with compact status plus bounded panels. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 123 review note: the generic GamePackage playable projection adapter pass adds a projection-only read of `samples/minimal-map-game/package.json` to the accepted Alpha Unity projection route and verifies package identity, map dimensions, start/player proxy, tiles, entities, interactions, item summary and event log. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 124 review note: the generic GamePackage quest/dialogue/interaction loop pass adds projection-local sign inspect preview/apply, old guard dialogue, help healer objective, inventory/resource summary and event log over `samples/minimal-map-game/package.json` to the accepted Alpha Unity projection route. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 125 review note: the generic GamePackage systems loop pass adds projection-local recipe craft, harvest, transaction preview, encounter/combat preview, inventory/resource summary and systems event log over `samples/minimal-map-game/package.json` to the accepted Alpha Unity projection route. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 126 review note: the generic GamePackage full playthrough pass ties the projection-local map path, sign inspection, dialogue, quest objective, inventory/resource/systems, transaction, combat and event transcript checks over `samples/minimal-map-game/package.json` into one accepted Alpha Unity projection route. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 127 review note: the WinForms Unity projection verification runner makes the Goal126 batchmode route repo-local, dry-run visible, cleanup-aware and surfaced in the existing Visual World Stream Preview Workspace. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 128 review note: the repo-local Unity projection verification runner now accepts optional `-PackagePath`, keeps `samples/minimal-map-game/package.json` as the read-only default, rejects paths outside the repository or under `.llmgc/manual/**`, forwards the resolved path to Unity and surfaces package-path/result/log/cleanup status in WinForms. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 129 review note: the candidate matrix runner verifies multiple repo-local GamePackage candidates through the Goal128 parameterized runner, including a byte-copy baseline and sample-derived variant, and records per-candidate result/log scans plus an aggregate matrix result in Goal129 artifacts. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 130 review note: the candidate factory materializes three repo-local projection-compatible GamePackage candidates from the read-only sample template, feeds the generated candidate index into the Goal129 matrix runner and records GREEN 3/3 factory plus matrix proof in Goal130 artifacts. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 131 review note: the candidate recipe catalog scoring and promotion pipeline materializes four metadata-only repo-local projection-compatible GamePackage candidates from the read-only sample template, feeds the generated candidate index into the Goal129 matrix runner, scores GREEN 4/4 matrix-passed candidates and promotes `minimal-map-game-balanced-baseline` as the selected candidate with score 100 in Goal131 artifacts. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 132 review note: the WinForms candidate pipeline operator panel exposes the existing Goal131 recipe pipeline command, selected candidate proof, matrix counts and output-tail capture in the workspace with GREEN_READY operator evidence. Release blockers remain open for final release, Runtime/player proof, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 133A review note: the product-line strategy rebaseline records that LLMGameCreator is a data-driven game product-line combiner, not prompt-to-game; LLM is optional authoring assistance only; GamePackage plus canonical runtime state are source of truth. Release blockers remain open until Goal134 or later proves selected-candidate package validation, canonical runtime playthrough, save/load/replay and Unity/player consumption of canonical transcript/state summary. Projection-only candidate/operator evidence is explicitly not enough for product readiness.

Goal 134 review note: the canonical Runtime selected-candidate playthrough matrix proves the Goal131 selected candidate through package validation, Runtime-owned command/event transcript and state summary, state hash chain, save/load/replay and Unity/player consumption of canonical transcript/state summary. Release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, deeper playable player-loop quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 135 review note: the canonical Runtime playable player-loop readiness proof turns the Goal134 transcript/state summary into a PlayerAdapter contract, deterministic 13-step player-facing plan, classified diagnostic set and Unity/player readiness smoke while keeping canonical Runtime output as gameplay authority. Release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 136 review note: the canonical Runtime player command-loop execution matrix executes the Goal131 selected candidate through 13 Runtime-owned player commands with one snapshot per command, runtime event/state-hash proof, all required command categories, classified diagnostics and Unity/player snapshot consumption smoke. Release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 137 review note: the canonical Runtime Unity/player loop playback harness turns the Goal136 Runtime-owned snapshots into 13 deterministic Unity/player playback frames with required HUD/player/interaction/dialogue/quest/inventory/crafting/harvest/transaction/encounter/combat/final-state categories and batchmode playback smoke. Release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

| Risk | Why it blocks release | Required gate |
|---|---|---|
| Playable quality vs proof quality | Existing evidence can pass while the player experience remains inspection-only or debug-like. | Vertical Slice Final manual checklist with player-visible loop and package export/import proof. |
| Runtime dependency boundary violation | Runtime or Unity must not call LLMs, RAG, media providers, WinForms or external generation tools. | Runtime/player dependency scan and package-only consumption proof. |
| Save/load and infinite world deltas | Large/infinite worlds require bounded, deterministic discovered/mutated state. | Save/load/replay gate for selected finite/infinite world mode. |
| Clean-machine install/export | A local developer proof is not a release proof. | Clean-machine installer/export/player launch smoke. |
| Provider/provenance/licensing | Unlicensed media, map data or provider output can block distribution. | License/provenance manifest and fail-closed export policy. |
| Adult/rating leakage | Adult-capable metadata must not leak into safe/public builds. | Rating export filter and safe fallback gate. |
| Geospatial licensing/ToS/API | Real-world map/geodata ingestion can violate ToS or redistribution rules. | Legal/licensing/provider policy before implementation. |

## P1 Serious Risks

| Risk | Impact | Required mitigation |
|---|---|---|
| Unity performance | Streamed chunks, atlases, UI and save/load may fail target hardware. | Performance budget smoke for selected player target. |
| StreamingAssets/platform issues | Paths, casing, file sizes and platform packaging can diverge across Windows/Unity targets. | Platform handoff probe plus clean export matrix. |
| Visual consistency | Deterministic assets can still look incoherent across packs/styles. | Approved renderer/atlas style review and visual consistency diagnostics. |
| Source-health/code-size | Large files and oversized seams slow review and increase defect risk. | Continue source-health guard and split before source limits are exceeded. |
| Validation noise/duration | Long full checks can hide actionable failures and waste cycles. | Use Goal089 tiers; reserve full/observed full for milestone/release-like work. |
| Runtime vs Unity parity | Headless runtime proof can diverge from Unity/player behavior. | Shared package inputs plus side-by-side runtime/player proof for milestone gates. |

## P2 Technical Debt

| Risk | Impact | Required mitigation |
|---|---|---|
| Repeated proof helpers | Repeated hash/read/evidence code can drift. | Extract only when duplication causes real maintenance or behavior risk. |
| Large method/file debt | Known debt remains in Application and Unity bootstrap areas. | Dedicated bounded decomposition goals with tests held fixed. |
| Artifact timestamp-like values | Deterministic claims become harder to audit. | Normalize volatile values in future reproducibility pass. |
| Visual final renderer missing | Text SVG proof is not production output. | Renderer/atlas prototype and approved output contract. |
| Provider quarantine missing | Future provider outputs cannot be safely promoted. | Candidate quarantine/provenance/review ledger gate. |
| Release documentation gaps | Users need supported/unsupported mode clarity. | v1 docs and sample package pass. |

## P3 Deferrable Polish

| Risk | Impact | Defer rule |
|---|---|---|
| Dashboard/UI polish | Functional review surfaces may remain dense. | Defer until core player/export loop is stable. |
| Extra visual styles | More styles can distract from one coherent release path. | Add only when a milestone needs them. |
| Advanced dream tracks | Realism/geospatial/space-rangers-like tracks can explode scope. | Keep as future register until v1 path is stronger. |
| Broad refactors | Cleanup can consume limit without player value. | Only refactor to remove a named blocker or source-health breach. |

## Release Gate Plan

1. Vertical Slice Final: prove one generated player-facing loop plus export/import and risk review.
2. Strong Alpha: prove repeatable multi-family generation, Unity/player path, save/load deltas and rating-safe export behavior.
3. v1 Full Final: prove clean-machine install/export/player launch, docs, samples, diagnostics, dependency/license audit and release validation.
4. Dream Full Final: only after v1, select specific dream tracks and require research/legal gates before implementation.
