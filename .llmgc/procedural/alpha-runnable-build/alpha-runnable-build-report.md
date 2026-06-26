# Alpha Runnable Windows Build Report

- Accepted: false
- Final status: alpha_runnable_windows_build_verification
- Blocker reached: false
- Previous gate: unity_runtime_export_vertical_slice_artifact_verification passed
- Completed slices: S106, S107, S108, S109, S110, S111, S112, S113
- Product smoke route: alpha-runnable-build
- Primary candidate: frontier_survival
- Package hash: 3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53
- Asset manifest hash: 3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595
- Export manifest hash: cd28d799e3e9b5d96be785eab80b6a5749552172dcbd6dbce6e28f7280a39a81
- Staging folder: .llmgc/procedural/alpha-runnable-build/staging
- Build folder: .llmgc/procedural/alpha-runnable-build/build/windows
- Executable: LLMGameCreatorAlpha.exe
- Windows executable produced: true
- Unity Editor executed: true
- Unity build produced: true
- Launch verified: true
- Play loop verified: false
- Deterministic report hash: 9f0f8d0283df74ea30c6efdce16614d5b5b0b31477f690cc79abb5055fb93f64
- Build manifest hash: 6c774e5228b13f49ecc6cd65031edad00705dd9dfee84ce8f99f5f4fe950c57d

## Style Candidates

- frontier_survival: package=game/content_generation/frontier-survival packageHash=3e8a42663e1a2fdabd98cdd8c30ab6188810bd4d0f4d36aa4e3089a71b952d53 assetManifestHash=3dd392bae4cbac24db34b1810a52c83cf64791521df8849c75ac61e8fdcfa595 exportManifestHash=cd28d799e3e9b5d96be785eab80b6a5749552172dcbd6dbce6e28f7280a39a81
- gothic_mystery: package=game/content_generation/gothic-mystery packageHash=b960642adb592af8949ce5d99b3e2a6eb88c79d2c9870116ec6459e47d85191f assetManifestHash=09dc29d8c7d41df403e20122d035be088b3746d32151df7e892e895c7bbcf195 exportManifestHash=cc802ab8cec3455c1e1331f8db85042ae42e4e51be3860adb0a719dcb6231da6
- trade_caravan: package=game/content_generation/trade-caravan packageHash=02f2f2d207e4d32184682be5b42b02539be12f7da5bd34142f0e1e011b634706 assetManifestHash=5aa6d73963072647eb32be772c4c0564f74651054bed512ec909486b975fd81b exportManifestHash=442e0fb472e47b4207b2635d8d1be152042ffe4fd00769323120058444c8d58c

## Invalid Matrix

- absolute_output_path_injection: actualValid=false diagnostics=alpha_build.output.unsafe_path
- asset_manifest_hash_mismatch: actualValid=false diagnostics=alpha_build.contract.asset_manifest_hash_mismatch
- copied_expectation_report_without_build_files: actualValid=false diagnostics=alpha_build.invalid.expectation_only_report
- cross_style_package_export_asset_leakage: actualValid=false diagnostics=alpha_build.contract.cross_style_leakage
- export_manifest_hash_mismatch: actualValid=false diagnostics=alpha_build.contract.export_manifest_hash_mismatch
- mismatched_executable_build_file_hash: actualValid=false diagnostics=alpha_build.output.hash_mismatch
- missing_accepted_goal012_evidence: actualValid=false diagnostics=alpha_build.contract.missing_goal012_evidence
- missing_executable: actualValid=false diagnostics=alpha_build.output.missing_executable
- missing_staged_asset_payload: actualValid=false diagnostics=alpha_build.staging.missing_asset_payload
- missing_staged_game_data: actualValid=false diagnostics=alpha_build.staging.missing_game_data
- package_hash_mismatch: actualValid=false diagnostics=alpha_build.contract.package_hash_mismatch
- path_traversal_in_staging_manifest: actualValid=false diagnostics=alpha_build.staging.unsafe_path
- runtime_preview_dependency_claim: actualValid=false diagnostics=alpha_build.contract.runtime_preview_dependency
- unity_build_claim_without_artifact: actualValid=false diagnostics=alpha_build.output.unity_build_claim_without_artifact

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
- error: alpha_build.output.unsafe_path [C:/escape.exe] Build output paths must be safe relative paths.
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
