# Milestone Gates

Status: Goal 097 acceptance-gate plan
Manual gate: `final_roadmap_rebaseline_dream_scope_productivity_verification required`
Accepted: false

Each milestone must produce visible product progress. Contract and proof artifacts are required, but they are not sufficient by themselves.

## Vertical Slice Final

Goal 110 note: `offline_geoworld_alpha_manual_acceptance_verification` now packages the offline geoworld Alpha manual checklist, result template and release-risk/milestone links for review, but it remains `accepted=false` and does not by itself close `vertical_slice_final_verification`.

Goal 111 note: the manual-result intake bridge can classify a real Goal110 result as pending, invalid, incomplete, accepted-false or a valid candidate for the human gate. Current repository state is `BLOCKED_PENDING_MANUAL_RESULT` because no real manual result JSON exists; this does not close `vertical_slice_final_verification` and does not start final release, Runtime, provider/network/geodata, schema or final-art work.

Goal 112 note: the acceptance operator pack surfaces Goal110 checklist instructions and Goal111 decision status as RC readiness visibility. Current repository state is `OPERATOR_READY_PENDING_HUMAN_RUN` because no real manual result JSON exists; this does not mean Alpha accepted, does not close `vertical_slice_final_verification`, and does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.

Goal 113 note: the manual-result workbench surfaces Goal110 required steps, Goal111/Goal112 statuses, the preferred real result path and a draft/template for human copy/edit. Current repository state is `WORKBENCH_READY_PENDING_HUMAN_RESULT` because no real manual result JSON exists; this does not mean Alpha accepted, does not close `vertical_slice_final_verification`, does not write `.llmgc/manual/**`, and does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.

Goal 114 note: the Unity Safe Mode compile hotfix unblocks reported Unity compile errors in the manual acceptance helper scripts only. It does not mean Alpha accepted, does not close `vertical_slice_final_verification`, does not write `.llmgc/manual/**`, and does not start live geodata/provider/network/runtime/schema/Lua/generator-library/final-art/final-release work.

Goal 115 note: the human-result revalidation reads the real local `.llmgc/manual/**` result and records `GREEN_ACCEPTABLE_CANDIDATE`, 12/12 passed required steps and manualResultSha256 `8c2ad299d241d4315248b642b723ae8cf33ecabaa42a46462985ea5dc8335aeb`. It does not mean Alpha accepted, does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and still requires explicit human decision for `offline_geoworld_alpha_manual_acceptance_verification`.

Goal 116 note: explicit human acceptance for `offline_geoworld_alpha_manual_acceptance_verification` is recorded with manualGateStatus=`ACCEPTED_BY_HUMAN`, humanAccepted=true and the exact statement `Я принимаю offline_geoworld_alpha_manual_acceptance_verification по Goal115 GREEN_ACCEPTABLE_CANDIDATE.` This closes only that manual gate decision. It does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, Runtime, provider/live geodata/network, public schema, Lua, generator-library, final art/atlas or Unity scene/prefab/project-settings/release-packaging work.

Goal 117 note: the post-acceptance continuation matrix is GREEN and recommends `accepted_alpha_baseline_review` / `goal-118-offline-geoworld-accepted-alpha-baseline-review` as the next bounded lane. This does not start Goal118 automatically and does not approve live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 118 note: the accepted Alpha baseline review package is GREEN with baselineId `offline_geoworld_alpha_accepted_baseline_v1`, acceptedBaselineReady=true and recommendedNextDecision=`EXPLICIT_NEXT_LANE_SELECTION`. This packages the accepted evidence chain for review only; it does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 119 note: the accepted Alpha Unity playable projection entrypoint is GREEN with Unity menu path `LLMGameCreator/Accepted Alpha/Build/Refresh Playable Projection` and generated root `__LLMGC_AcceptedAlphaPlayableProjection__`. This creates a hands-on Unity verification route over the Goal118 accepted baseline only; it does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 119A note: the accepted Alpha Unity material warning hotfix keeps the Goal119 route and removes the edit-mode marker material-instantiation warning from the projection. The next manual check is still the Goal119 Unity menu route, now with the expected Console result that no material-leak warning is emitted. This does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 120 note: the accepted Alpha projection usability and cleanup pass keeps the Goal119 route, adds descriptor-backed selection controls, a visible legend and bounded Unity editor-noise cleanup commands. The next manual check is still the accepted Alpha Unity menu route, now with focus/select/legend controls and cleanup dry-run/apply proof. This does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 121 note: the accepted Alpha interaction drilldown and one-click verification pass keeps the Goal119 route and makes the primary manual check one menu action plus `Run Full Projection Verification`. It adds selected-marker details, interaction/action preview, objective/replay details, a compact event log and batchmode full verification proof. This does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 122 note: the accepted Alpha projection action-loop and window-polish pass keeps the same one-button verification path, adds projection-local Preview/Apply/Reset state and makes the EditorWindow readable with compact status plus bounded panels. This does not close `vertical_slice_final_verification`, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 123 note: the generic GamePackage playable projection adapter pass adds `Run Generic Package Projection Verification` to the accepted Alpha Unity projection route and visualizes `samples/minimal-map-game/package.json` as read-only projection data. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 124 note: the generic GamePackage quest/dialogue/interaction loop pass adds `Run Generic Package Gameplay Loop Verification` to the accepted Alpha Unity projection route and proves a projection-local sign inspect, old guard dialogue, help healer objective, inventory/resource summary and event log over `samples/minimal-map-game/package.json` as read-only input. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 125 note: the generic GamePackage systems loop pass adds `Run Generic Package Systems Loop Verification` to the accepted Alpha Unity projection route and proves projection-local recipe craft, harvest, transaction preview, encounter/combat preview, inventory/resource summary and systems event log over `samples/minimal-map-game/package.json` as read-only input. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 126 note: the generic GamePackage full playthrough pass adds `Run Generic Package Full Playthrough Verification` to the accepted Alpha Unity projection route and proves a projection-only map path, sign inspection, dialogue summary, quest objective status, inventory/resource/systems summaries, transaction preview, combat preview and event transcript over `samples/minimal-map-game/package.json` as read-only input. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 127 note: the WinForms Unity projection verification runner makes `.devflow\scripts\run-unity-projection-verification.cmd` the normal repo-local verification path for the Goal126 batchmode full playthrough, with result/log scan and bounded cleanup surfaced in Visual World Stream Preview Workspace. Manual Unity inspection remains optional. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Goal 128 note: the same normal runner now accepts optional `-PackagePath`, resolves only repo-local GamePackage JSON outside `.llmgc/manual/**`, forwards it to Unity as `-llmgcPackagePath`, and surfaces package-path/result/log/cleanup status in Visual World Stream Preview Workspace and WinForms. Manual Unity inspection remains optional. This does not close `vertical_slice_final_verification`, does not mutate the sample package, does not commit `.llmgc/manual/**`, and does not approve final release, live geodata/provider/network, Runtime/schema, Lua, generator-library, final renderer/atlas, Unity scene/prefab/project-settings/packages/StreamingAssets or release-packaging work.

Acceptance gate:

- user-visible/editor-visible generated package workflow;
- one coherent playable or simulatable loop with movement/exploration, interaction/event/quest and reward/cost/state change;
- generated package export/import proof;
- Unity/player proof where the selected slice uses Unity/player handoff;
- validation tiers: restore/build, focused tests, current-state docs guard, scenario artifact scope and selected product/player smoke;
- manual acceptance checklist with screenshots/log excerpts or direct artifact/player proof;
- release risk review against P0/P1 register.

Exit statement:

```text
vertical_slice_final_verification required
```

## Strong Alpha

Acceptance gate:

- editor-visible generation/review/export workflow for at least three distinct supported family/profile shapes;
- generated package export/import and replayable validation for each selected family;
- Unity/player proof for the main presentation target;
- save/load proof for selected finite/infinite world deltas;
- rating/adult safe-public export proof when adult metadata exists;
- validation tiers include spine-fast or full/observed-full for shared/core-risk changes;
- manual acceptance checklist covers playability, inspectability, exportability and known release risks.

Exit statement:

```text
strong_alpha_verification required
```

## v1 Full Final

Acceptance gate:

- clean-machine install/export/player launch proof;
- supported game-family matrix with explicit non-supported modes;
- generated package export/import plus player/runtime consumption proof;
- Unity/player performance budget for selected target;
- save/load/replay proof for selected gameplay/world modes;
- provider/provenance/license/rating manifest for shipped assets and sample packages;
- release docs, sample games and diagnostics;
- full or observed-full validation plus targeted product/player smoke routes;
- manual release acceptance checklist and P0/P1 risk closure.

Exit statement:

```text
v1_full_final_verification required
```

## Dream Full Final

Acceptance gate:

- explicit track selection from `docs/context/DREAM_SCOPE_REGISTER.md`;
- dream-track research gate and risk review before implementation;
- generated or ingested world proof appropriate to the selected track;
- Unity/player/runtime proof without runtime LLM/provider dependency;
- licensing/ToS/provider policy proof for geospatial or provider-backed tracks;
- rating/export/store policy proof for adult-capable tracks;
- release-grade validation and manual acceptance for the selected dream scope.

Exit statement:

```text
dream_full_final_verification required
```

## Manual Checklist Template

- The user can identify what changed without reading raw evidence.
- The editor or player exposes the result.
- Export/import or handoff is proven from real files.
- Runtime/player consumes package data or approved refs only.
- Validation tier matches milestone risk.
- Release risk register was reviewed and updated.
- The next goal is not started until this gate is accepted.
