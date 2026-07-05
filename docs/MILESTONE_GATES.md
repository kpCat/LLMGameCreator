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
