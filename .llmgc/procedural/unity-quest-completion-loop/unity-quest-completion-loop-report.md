# Unity Generated Quest Completion Loop Report

- Accepted: false
- Final status: unity_generated_quest_completion_loop_verification
- Previous gate: unity_generated_runtime_state_loop_verification passed
- Completed slices: S138, S139, S140, S141, S142, S143, S144, S145
- Product smoke route: unity-quest-completion-loop
- Selected package/style/thread: game/content_generation/frontier-survival / frontier_survival / thread/frontier-survival/000
- Selected quest/dialogue/choice/item/event/reward: quest/frontier-survival/c2ed6dc235/000 / dialogue/frontier-survival/51924b073c/028 / choice/frontier-survival/8cb7a7d288/028 / item/frontier-survival/7f81222990/004 / event/frontier-survival/4f2d855a33/000 / item/frontier-survival/7f81222990/004
- Quest completion loop verified: true
- Phase/objective/correlation: true / true / true
- Quest completed/reward granted: true / true
- Movement/focus/interaction/play-loop: true / true / true / true
- Objective count: 6
- Quest-loop hash: 1e5ad0b13a44078ebc6c5aa53e7e54c21d43a8c779a7b6937a2398eb9ad62b83
- Plan hash: 9fe5ddc1abe483ca75f508da7aa3a85f709272f039d9bf734f3dc0c2f0fb7085
- State hash: 1e5ad0b13a44078ebc6c5aa53e7e54c21d43a8c779a7b6937a2398eb9ad62b83
- Deterministic report hash: c2cec57aead38f85c46b2281a53d75b0779360a7f5e32b94c7aa0879d353534c
- Build manifest hash: 0b4859331314dc86505e2e14fec2b50856040274c97c3e5bf7e5dfb09e543771
- Invalid/fake/leak scenarios rejected: 24/24

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
- info: unity_quest_loop.goal016_gate_recorded [unity_generated_runtime_state_loop_verification] User-confirmed Goal 016 Unity runtime state loop verification is recorded as passed.
- info: unity_quest_loop.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak quest completion scenarios must reject through quest-loop, runtime-state, previous-evidence or firewall validation paths.
- info: unity_quest_loop.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
