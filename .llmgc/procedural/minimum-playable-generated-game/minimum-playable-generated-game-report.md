# Minimum Playable Generated Game Report

- Accepted: false
- Final status: minimum_playable_generated_game_verification
- Previous gate: unity_alpha_readable_presentation_verification passed
- Completed slices: S162, S163, S164, S165, S166, S167, S168, S169
- Product smoke route: minimum-playable-generated-game
- Selected package/style/thread: game/content_generation/frontier-survival / frontier_survival / thread/frontier-survival/000
- Selected quest/reward: Quest 000 (quest/frontier-survival/c2ed6dc235/000) / Reward: item 004 (item/frontier-survival/7f81222990/004)
- Review package: .llmgc/procedural/minimum-playable-generated-game/review-package
- Review package verified: true
- Automated launch/quest completion: true / true
- Readable presentation verified: true
- Manual review required: true
- Manifest hash: de81cb001184078d0ce06bd1d29b925576b48f063ba10881b4593942df5e6551
- Review package hash: ba4cd31e805476ef67c53b044b7cb1f2487fdf690eec2cde30fc28da6c724be7
- Deterministic report hash: eab5487cb13073bba82e7c55ecb4ea7e11016821e89e88dc6b33ec50c77fee28
- Invalid/fake/leak scenarios rejected: 25/25

## Diagnostics

- error: minimum_playable_game.readable.not_verified [unity-alpha-readable-presentation-report.json] Goal 019 readable presentation must be verified before package review.
- info: alpha_build.environment.not_blocked [alpha_unity_build_environment_blocker] A real Windows build path exists and produced verifiable output; the runnable gate remains required for review.
- info: alpha_build.environment.unity_found [unity_cli] Unity Editor executable was discovered; local machine path is omitted from deterministic artifacts.
- info: alpha_build.goal012_gate_recorded [unity_runtime_export_vertical_slice_artifact_verification] User-confirmed Goal 012 artifact verification is recorded as passed.
- info: alpha_build.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios must fail through the Alpha build validation path.
- info: alpha_build.launch.executed [logs/alpha-player-launch.log] The produced Windows player was launched in batch diagnostic mode.
- info: alpha_build.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
- info: alpha_build.unity_build.executed [logs/unity-build.log] Unity Editor was invoked through the repository-local Alpha build entrypoint.
- info: alpha_build.unity_build.exit_success [exit_code:0] Unity build process completed; see logs/unity-build.log for details.
- info: alpha_build.valid_matrix_passed [valid_matrix] Three style candidates and deterministic staging are required.
- info: minimum_playable_game.goal019_gate_recorded [unity_alpha_readable_presentation_verification] User-confirmed Goal 019 readable presentation verification is recorded as passed.
- info: minimum_playable_game.invalid_matrix_rejected [invalid_matrix] Invalid/fake/leak scenarios reject through file, hash, script, log, presentation or firewall validation paths.
- info: minimum_playable_game.no_external_providers [execution_boundary] No LLM, RAG, provider, media, arbitrary Lua or generator-library execution was invoked.
- info: minimum_playable_game.previous.goal019_evidence_present [unity-alpha-readable-presentation-report.json] Accepted Goal 019 compact evidence is present and matching.
