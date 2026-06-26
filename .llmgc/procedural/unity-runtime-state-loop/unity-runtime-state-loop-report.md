# Unity Generated Runtime State Loop Report

- Accepted: false
- Final status: unity_generated_runtime_state_loop_verification
- Previous gate: unity_generated_scene_content_projection_verification passed
- Completed slices: S130, S131, S132, S133, S134, S135, S136, S137
- Product smoke route: unity-runtime-state-loop
- Selected package: game/content_generation/frontier-survival
- Selected style: frontier_survival
- Selected thread: thread/frontier-survival/000
- Runtime state loop verified: true
- State transition trace verified: true
- Quest/dialogue/inventory/event: true / true / true / true
- Movement/focus/interaction/play-loop: true / true / true / true
- Command/state transition count: 7
- State-loop hash: 5d73bd5d27c171ffb2a0235d09fdd0d5cbe961c486608561ac97c5ee96eeb5ec
- Deterministic report hash: 23cd50c8e1bf1bf2fb3a8d281eb646d00fe87b395fc40b457ef6d76ae9a7abd2
- Build manifest hash: 3ee38b49cd4fd0f96333b4f2f73f6c72ed2a467bc740496442b7c381ac03cfbe
- Invalid/fake/leak scenarios rejected: 20/20

## Diagnostics

- error: alpha_build.contract.asset_manifest_hash_mismatch [frontier_survival] Candidate asset manifest hash must match selected Goal 012 evidence.
- error: alpha_build.contract.cross_style_leakage [frontier_survival] Package, export and asset evidence must come from the same style candidate.
- error: alpha_build.contract.export_manifest_hash_mismatch [frontier_survival] Candidate export manifest hash must match selected Goal 012 evidence.
- error: alpha_build.contract.missing_goal012_evidence [frontier_survival] Alpha candidates must reference accepted Goal 012 export evidence.
- error: alpha_build.contract.package_hash_mismatch [game/content_generation/frontier-survival] Candidate package hash must match selected Goal 012 evidence.
- error: alpha_build.contract.runtime_preview_dependency [runtime_host] Alpha proof must not depend on WinForms Runtime Preview.
- error: alpha_build.invalid.expectation_only_report [alpha-runnable-build-report.json] Expectation reports cannot replace physical build files.
- error: alpha_build.output.hash_mismatch [build/windows/LLMGameCreatorAlpha.exe] Build manifest hashes must match actual file bytes.
- error: alpha_build.output.missing_executable [build/windows] Build validation rejects missing Windows executable.
- error: alpha_build.output.unity_build_claim_without_artifact [build/windows] Unity build claims require real output files.
- error: alpha_build.output.unsafe_path [absolute-output-path-injection] Build output paths must be safe relative paths.
- error: alpha_build.staging.missing_asset_payload [assets] Staging must contain physical asset payloads.
- error: alpha_build.staging.missing_game_data [game-data/game-package.json] Staging must contain physical game data.
- error: alpha_build.staging.unsafe_path [../escape.json] Staging manifest paths must stay inside the staging root.
- info: alpha_build.environment.not_blocked [alpha_unity_build_environment_blocker] A real Windows build path exists and produced verifiable output; the runnable gate remains required for review.
- info: alpha_build.environment.unity_found [unity_cli] Unity Editor executable was discovered; local machine path is omitted from deterministic artifacts.
- info: alpha_build.goal012_gate_recorded [unity_runtime_export_vertical_slice_artifact_verification] User-confirmed Goal 012 artifact verification is recorded as passed.
- info: alpha_build.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios must fail through the Alpha build validation path.
- info: alpha_build.launch.executed [logs/alpha-player-launch.log] The produced Windows player was launched in batch diagnostic mode.
- info: alpha_build.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
- info: alpha_build.unity_build.executed [logs/unity-build.log] Unity Editor was invoked through the repository-local Alpha build entrypoint.
- info: alpha_build.unity_build.exit_success [exit_code:0] Unity build process completed; see logs/unity-build.log for details.
- info: alpha_build.valid_matrix_passed [valid_matrix] Three style candidates and deterministic staging are required.
- info: unity_runtime_state.goal015_gate_recorded [unity_generated_scene_content_projection_verification] User-confirmed Goal 015 generated scene projection verification is recorded as passed.
- info: unity_runtime_state.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak runtime state scenarios must reject through state-loop, log and firewall validation paths.
- info: unity_runtime_state.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
