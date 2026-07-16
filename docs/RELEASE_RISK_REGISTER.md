# Release Risk Register

Status: Goal 097 planning register
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

## P0 Release Blockers

Goal152C note: external LocalAppData Unity workspaces prevent generated Unity settings from contaminating the repository. Automated proof cannot accept a PlayerAdapter standalone; the five-step human screen review remains required and Goal152/Goal152A stay unaccepted.

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

Goal 137 review note: the canonical Runtime Unity/player loop playback harness is accepted by human handoff with `acceptedByHuman=true`, `acceptedByCodex=false`, selectedCandidateId=`minimal-map-game-balanced-baseline`, 13 playback frames, Unity playback smoke GREEN, projectionOnly=false and unityGameplayTruth=false. Release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 138 review note: the runtime-backed Unity player-loop stepper/HUD harness consumes the Goal137 playback frames plus Goal136 Runtime snapshots/result and Goal135 PlayerAdapter contract to produce 13 runtime-backed stepper frames, a Unity Editor stepper window, batchmode stepper smoke and one-click report while keeping runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false. It is now recorded as accepted by human handoff with acceptedByCodex=false; release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 139 review note: the runtime-backed Unity player-loop interactive controls harness builds on the Goal138 stepper model/result plus Goal137 playback frames, Goal136 Runtime snapshots and Goal135 PlayerAdapter contract to produce 13 runtime-backed control frames, first/previous/next/last/auto-step/auto-play/copy-summary control proof, a Unity Editor controls window, batchmode controls smoke and one-click report while keeping runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false. It is now recorded as accepted by the Goal140 human handoff with acceptedByCodex=false; release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 140 review note: the runtime-backed Unity player-loop controls UX polish and noise guard builds on the Goal139 interactive controls artifacts to produce human-readable frame numbering, Step Once / Play All To End semantics, copy/reset status proof, Unity/player controls UX smoke and bounded BuildProfileContext editor-noise classification while keeping runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false. It is now accepted by the Goal141 human handoff with acceptedByCodex=false; release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 141 review note: the runtime-backed Unity/player command roundtrip bridge records Goal140 human acceptance, maps six Unity/PlayerAdapter control intents into correlated request/response pairs, routes four controls into Runtime execution, keeps `load_model` and `copy_frame_summary` presentation-only with unchanged state hashes, writes updated runtime snapshots and proves Unity/player consumes the roundtrip result while keeping runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false. It remains `accepted=false`; release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 142 review note: the runtime-significant product-line variant matrix and selection handoff materializes four deterministic variants from the read-only minimal-map sample template, validates all four packages, runs all four through the Runtime-backed player command roundtrip, proves four distinct final runtime state hashes and selects `minimal-map-game-exploration-resource-focus` with score 100 while keeping runtimeAuthority=true, projectionOnly=false and unityGameplayTruth=false. It is accepted by explicit Goal143 human handoff with acceptedByCodex=false; Goal141 remains `accepted=false`, and release blockers remain open for final release, provider/live geodata/network policy, public schema, Lua, generator-library, final art/atlas, sample promotion, real hands-on playable quality and Unity scene/prefab/project-settings/StreamingAssets/release-packaging approval.

Goal 143 review note: the selected runtime variant end-to-end PlayerAdapter handoff validates the Goal142 selected package SHA-256, repeats the corrected Runtime sequence to the same final state hash, builds 15 request-correlated frames and passes a real read-only Unity consumer smoke while keeping Runtime as gameplay truth. Goal143 is accepted by explicit Goal144 human handoff with acceptedByCodex=false.

Goal 144 review note: the selected variant now has a persistent Runtime-owned action session, 14 package/state-derived descriptors, correlated individual actions, invalid-action no-mutation, journal checkpoint reload and full deterministic replay to the accepted Goal142 hash. Unity remains read-only and WinForms remains an Application adapter. Goal144 is accepted by explicit Goal145 human handoff; hands-on playable quality, final release, public schema, providers, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release packaging remain open.

Goal 144A review note: descriptor targets are no longer decorative. Harvest binds to and executes `node/apple_tree`, basic attack binds to and executes `goblin`, descriptor ranges drive Runtime execution, and checkpoint/final replay evidence reports immutable counts 8/13. The hotfix does not claim Goal144 human acceptance or reduce the remaining release/manual-review risks.

Goal 145 review note: Goal144 is accepted by explicit human handoff. Goal145 discovers four Goal142 candidates, validates package metadata/path/SHA, executes the same Runtime session/replay kernel for each, proves four distinct final hashes and semantic alchemy/combat/exploration effects, and exposes operator selection in WinForms plus a read-only Unity matrix. Goal145 is accepted by the later Goal146 human handoff with `acceptedByCodex=false`; hands-on playable quality, final release, public schema, providers, Lua, generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release packaging remain open.

Goal 145A review note: the selector reentrancy/selection-drift risk is fixed with operator-only `SelectionChangeCommitted`, guarded programmatic binding and behavioral STA coverage. Callback counts are 0/0/1 with maximum depth 1; combat selection and package identity persist through refreshes, while a real candidate change resets prior session/checkpoint/action/replay state. Goal145 is accepted by the later Goal146 human handoff.

Goal 146 review note: Goal145 is accepted by human handoff. Goal146 closes the immediate hardcoded-prebuilt-candidate composition risk for the proven vertical slice: eight FeatureModule combinations create novel packages and pass one shared Runtime/save/replay qualifier with distinct semantic outcomes. Goals146/147 are accepted by the exact human decision recorded with Goal148. Remaining P0/P1 risks still include hands-on playable quality, clean-machine release proof, public-schema decisions, providers/licensing, Lua/generator-library, final art/atlas, sample promotion and Unity scene/prefab/project-settings/StreamingAssets/release packaging.

Goal 147 review note: the immediate in-memory/hardcoded authoring risk is bounded by a file-based fingerprinted module registry, eight generic typed parameters, atomic saved-composition persistence with stale/missing-module diagnostics, and independently cached per-module certification. A 100-module catalog avoids powerset certification and keeps interaction coverage at 9 rows under the 24-row cap. Default Goal146 hashes remain stable and a custom all-three composition passes the shared Runtime seam. Goals146/147 are accepted by explicit human decision; all broader release, provider, schema, Lua, art and Unity gameplay-truth risks remain open.

Goal 147A review note: the delayed ItemCheck callback/stale-document risk and UI-thread heavy-action freeze risk are repaired with real STA lifecycle coverage and off-thread in-process execution. The flat certification dependency fingerprint is replaced by deterministic transitive closure, with selective dependent invalidation and pre-Runtime cycle rejection.

Goal 148 review note: the user-facing fragmentation risk is reduced by consolidating project creation/open/save, mechanics, typed parameters and Runtime-qualified build activation on `Игры`. Transactional rollback protects package bytes/current state/last hashes, and legacy numbered panels are hidden by default behind an explicit diagnostics toggle. Goal148A closes the immediate first-build failure for production-created projects by planning, staging, validating and transactionally activating package-required scripts without overwriting differing user files. Remaining risks include replacing the read-only sample-backed narrow-alpha support source with release-owned templates, broader clean-machine content provisioning, release packaging and playable quality. Goal148 remains unaccepted pending review.

Goal 148B review note: the real Goal148 manual build exposed a synchronous worker-thread `CurrentChanged` delivery into WinForms and `_navigation`. Named disposal-safe dispatch now covers every subscriber, async page refresh failures are observed, and a real MainForm + Projects automated build retry is GREEN. The implementation risk is closed, but the Goal148 human workflow retry remains required and Goal148 remains unaccepted.

Goal 149 review note: Goal148 is accepted by human. The normal unified project
path no longer relies on the fixed 13-action plan: structured FeatureModule
contracts create a deterministic action/checkpoint/replay plan, and the first
equipment module proves generic metadata materialization, save/replay and a
catalog-configured player-only combat bonus. Catalog-only optional growth no
longer makes unrelated projects stale. Goal149 still requires human review;
hands-on playable quality, clean-machine release, public-schema, provider, Lua,
art and Unity packaging risks remain open.

Goal 150 review note: attributes and level progression now attach through
default-off FeatureModules, a target-kind mutation registry, canonical
Runtime-owned state and capability-driven presentation. Stat/equipment damage
combines independently and progression stage resolution reuses `OutputApplier`.
The legacy Goal149 hash paths remain exact and catalog-only additions remain
additive-compatible. This reduces mechanic-expansion and save/replay risk, but
the bundled Goals149/150 hands-on review plus broader playable-quality,
clean-machine, provider, art and packaging risks remain open.

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
| Complete-suite recursive smoke duration | The 1736-test suite includes long ProductSmoke routes and Goal150B shards shared mutable roots. | Use Goal150C exact-HEAD disposable worktrees, per-shard ProductSmoke roots, adaptive split/retry and terminal accounting; keep manual readiness false until failed/missing/aborted are zero. |
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

Goal151 adds an explicit stale executable/launch-target risk: Technical Details now
shows executable path, SHA-256 and file/informational version, and saved-project
diagnosis must use a fresh-build disposable-copy proof before attributing a defect to
current composition code.

Goal153 closes the immediate active-ability/mana/turn-status Runtime gap with generic
effect/cost/definition contracts. Remaining risk: future status stacking policy and richer
effect kinds must extend the same transactional replay-stable seam; they must not add
module/status/ability ID switches or Unity-owned gameplay truth.

Goal153A closes the P1 correctness risks found after Goal153: fixed two-tick qualification,
ignored EndTurn target binding, leaked success events after rollback, missing lethal-status
encounter resolution, undersized qualification health and non-causal mana configuration.
The remaining release risk is manual usability/readability only and is covered by the combined
Goal153/Goal153A gate; human acceptance is not inferred from automated GREEN.

Goal153B removes the remaining module-specific parameter relation, makes the declared parameter domain
and participant resource domain causal before Runtime, and requires independent audit before the combined family gate.

Goal153C closes the activated proof-fixture/global-capacity release risk. Qualification now uses real deterministic hostile content, proof-only high capacity stays in test memory, and expiry/defeat/encounter terminal outcomes plus conditional skips are replay-stable. Independent audit and the combined human gate remain required; no acceptance is inferred from automated GREEN.

Goal154B1 closes the P1 reward-preservation and action-scoped trusted-effect risk left in Goal154B. Goal154C3 subsequently closes the saved-project/WinForms/standalone publication surface, and Goal154D closes the exact all-selected precompleted-quest qualification blocker exposed by the first human attempt. The complete family was later accepted by the owner at `fc2ac34db60d2627e1cafc86493396937bf63fe4`.
## Goal154C3 closure risks

- Human acceptance remains pending by design; no Codex or human acceptance is claimed.
- Historical malformed Goal154B1 evidence remains immutable; this closure records only its truthful intended meaning.
- Standalone proof is cache-only by contract: the recorded host cache was reused, `HostRebuilt=false`, and Unity process starts were zero.

## Goal154D all-selected qualification closure

- The P1 all-selected precompleted-quest blocker is closed by a generic capability qualification guard. It never treats `quest.not_active` as Runtime success and never restarts a completed quest.
- A redundant advance skips only when the exact quest/objective are completed and the prior canonical history contains one `QuestCompleted` and one `QuestRewardGranted`; malformed, failed, missing or ambiguous states fail causally.
- The first failed gate remains historical. The exact retry was accepted by the owner; no current Goal154 human gate remains.

## Goal155 accepted-mechanics RC integration

- Goal155 independent audit found P1 `rc_record_not_correlated_with_current_package_and_document` at `7084244a`; Goal155A closes it by rejecting current package mismatch/missing evidence and correlating record, document, identity and authoring fingerprint before CURRENT.
- Historical 64-test closure remains validation debt and is not reopened.
- Clean-machine install and final release packaging remain future milestone work.
- Goal155 creates no human gate; it remains `accepted=false` and requires independent audit.

## Goal156 seeded generated project integration

- Goal155A independent-audit intake is GREEN at `ebaa4aba`, closing the Goal155 RC milestone without changing Goal155/155A acceptance.
- Goal156 creates a real seeded generated project without LLM, provider, Lua or Unity generation. The generated world is additive and preserved through modern FeatureModule build, replay and standalone.
- Independent audit at `12ef8a4d` found a P1 provenance risk: declared source inputs did not causally reproduce the stored plan, and overlay/base were not rebuilt from the canonical Goal142 baseline. Goal157 closes this risk.

## Goal157 generated-world provenance and activation

- Generated project provenance is now reproducible from its declared request. Seed/mode/style/variants drive full plan/rule/tiny/MVP regeneration and canonical Goal142 overlay/base reconstruction; a source-only seed edit fails validation.
- The final player package starts on generated content and executes a Runtime-owned move/interact loop. Accepted modern mechanics remain a separately typed baseline-start compatibility proof with their own hashes and qualification flags.
- Goal157 independent audit is GREEN at `8939aea0`. Goal158 closes the generated-region travel risk while preserving the strict source and two-lane foundation.
- Post-creation seed regeneration is not implemented; project-local generation source and sidecars are immutable inputs to the current build path.
- Cache-only proof is bounded to one hidden smoke: host reused, host not rebuilt, Unity process starts zero.

## Goal158 generated-region travel Runtime and standalone

- Generated projects now start in generated content and traverse at least one generated plan connection through a generic Runtime-owned map transition, with deterministic replay/save/standalone truth. Seed regeneration remains the next explicit product decision.
- Lane A accepted mechanics/social remains the compatibility authority. Lane B's only gameplay additions are one namespaced prototype and one deterministic gate entity per directed connection; project identity and generated start remain explicit controlled deltas.
- Runtime invalid-transition, route-planning, replay, roundtrip and final-validation failures are transactional. Old v2 Goal157 history can prove only `START_CURRENT`, never `TRAVEL_CURRENT`.
- Preset-label ambiguity is recorded as bounded P2 debt; it does not affect ID-based deterministic travel selection.

## Goal159 transactional seed regeneration

- Goal158 independent audit is GREEN at `9a350c63`. Existing generated projects now regenerate from a new deterministic request only after a complete isolated candidate builds, repeats deterministically and reopens `TRAVEL_CURRENT`.
- Source v2 separates requested and resolved options and correlates preset definition plus explicit overrides. v1 remains no-rewrite compatible and upgrades only on successful regeneration.
- Promotion is protected by source/authoring/package/identity/RC tokens, a second concurrency recheck, a durable journal and exact before-hash rollback/crash recovery. Identity, authoring, history and prior RC evidence are preserved.
- User-selectable historical-world rollback and cross-seed gameplay-save migration remain release risks for later explicit slices; Goal159 neither claims nor implements them.

## Goal160 sealed regeneration and generated-world history rollback

Regeneration and history rollback now share one operation lease, sealed candidate truth and semantic validation inside the rollback window. Historical worlds contain only strict generation artifacts and are rebuilt with current mechanics before apply. Gameplay save-state migration between generated worlds remains the next product decision.

- Goal159 independent audit found P1 `regeneration_commit_not_sealed_inside_shared_operation_and_semantic_rollback_boundary` at `c7788e1e`; Goal160 closes it with a whole-operation cross-process lock, cached immutable seal authority, in-transaction truth/inventory recheck, journal `validating`, and semantic rollback/recovery before commit cleanup.
- Historical rollback never promotes historical package, authoring, identity or RC as current truth. It restores historical generation into an isolated candidate, rebuilds with current mechanics/parameters/identity, repeats and reopens `TRAVEL_CURRENT`, then uses the normal sealed transaction.
- Cross-world gameplay save-state migration remains unimplemented and is the next explicit product decision. It must not be inferred from world-history rollback or portable project recovery.
