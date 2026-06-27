# Capability Bundle Pipeline Inputs Report

- Accepted: false
- Manual gate: capability_bundle_pipeline_inputs_verification
- Previous accepted gate: development_complexity_stabilization_verification passed
- Profiles: 3/3
- Pipeline inputs: 3
- Selection artifact hash: 2d2813325fcc82f9ddfe21cee9fc4448b1e42810d0c6a93e556044758c94d5ca
- Generator inputs hash: d77a779db0c3dc5b48b04de7bad9308079e1bd132011072bfafd348648ae4e27
- Gap report hash: f1720cc9ea8811818b782254bd4ba3f4341ea4fd60714626ccc360002b907446
- Report hash: 7d680f708c7808b26abc6b54dcd4d640e913fa775100c54c757cd066ce85869f
- Invalid/fake/leak scenarios rejected: 16/16
- External execution: none

## Selections

- game_profile/frontier-survival-minimum-alpha: bundles=8, blocked=1, futureRequired=34
- game_profile/gothic-mystery-investigation-alpha: bundles=9, blocked=1, futureRequired=33
- game_profile/trade-caravan-social-economy-alpha: bundles=9, blocked=0, futureRequired=35

## Pipeline Inputs

- game_profile/frontier-survival-minimum-alpha: readyForPackageAssemblyPlanning=false, contracts=24, validators=58
- game_profile/gothic-mystery-investigation-alpha: readyForPackageAssemblyPlanning=false, contracts=26, validators=66
- game_profile/trade-caravan-social-economy-alpha: readyForPackageAssemblyPlanning=true, contracts=27, validators=63

## Diagnostics

- info: capability_bundle.goal021_evidence.present [.llmgc/procedural/generated-game-profile-contract] Accepted Goal 021 compact report and pipeline plan are present.
- info: capability_bundle.goal021_profile_gate_recorded [generated_game_profile_contract_verification passed] User-confirmed Goal 021 generated game profile contract verification is recorded as passed.
- info: capability_bundle.goal022_gate_recorded [development_complexity_stabilization_verification passed] User-confirmed Goal 022 development complexity stabilization verification is recorded as passed.
- info: capability_bundle.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through profile, atlas, selector, gap or scope guard diagnostics.
- info: capability_bundle.no_external_execution [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua, Unity build, package assembly or generator-library execution was invoked.
