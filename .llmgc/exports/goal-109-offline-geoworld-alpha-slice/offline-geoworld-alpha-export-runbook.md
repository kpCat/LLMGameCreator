# Offline Geoworld Alpha Export Runbook

- goalId: goal_109_offline_geoworld_alpha_slice_export_package
- manualGate: offline_geoworld_alpha_slice_export_package_verification required
- accepted: false
- packageType: deterministic directory package
- notFinalReleaseOrRuntimeBuild: true

## Steps

1. Review `offline-geoworld-alpha-export-manifest.json`.
2. Review `offline-geoworld-alpha-export-file-index.json`.
3. Verify `offline-geoworld-alpha-export-checksums.json` from the package root.
4. Confirm all manual gates remain listed and accepted=false.
5. Confirm Goal107 final objective acceptance hash is present.
6. Confirm Goal108A source split and immutability audit is present.
7. Open Unity menu `LLMGameCreator/Offline Geoworld Alpha Slice Package`.
8. Use the package verifier without mutating scenes automatically.

## Manual Gates

- offline_geoworld_alpha_slice_export_package_verification
- offline_geoworld_alpha_slice_orchestrator_verification
- offline_geoworld_interaction_playable_probe_verification
- offline_geoworld_interactive_travel_preview_verification
- offline_geoworld_objective_acceptance_run_verification
- offline_geoworld_playmode_travel_preview_verification
- offline_geoworld_session_persistence_replay_verification
- offline_geoworld_unity_editor_preview_tool_verification
- offline_geoworld_unity_preview_runner_verification

## Warnings

- Manual gate offline_geoworld_alpha_slice_export_package_verification remains required.
- Goal109 package is portable Alpha review/export tooling, not final Runtime or release build.
- Real geodata, providers, final art, scene/prefab changes and public schema changes remain separate future gates.
