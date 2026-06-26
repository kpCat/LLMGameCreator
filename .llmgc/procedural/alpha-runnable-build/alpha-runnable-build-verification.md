# Alpha Runnable Windows Build Verification

Stopped at:

```text
alpha_unity_build_environment_blocker
```

- Previous accepted gate: unity_runtime_export_vertical_slice_artifact_verification passed
- Final runnable gate remains required: alpha_runnable_windows_build_verification
- Unity executable discovered: true
- Unity executable path: (omitted; local machine path is not part of deterministic evidence)
- Unity version evidence: 6000.1.10f1
- Repository Unity project found: false
- Repository Unity project: (none)
- Repository Unity build script found: false
- Repository Unity build script: (none)
- Unity command executed: false
- Unity command to run after adding a repo-local project/build script: (none)
- Build output folder: .llmgc/procedural/alpha-runnable-build/build/windows
- Executable relative path: (none)
- Launch verified: false
- Play loop verified: false
- Invalid/fake/leak scenarios rejected: 14/14

User steps to unblock:

1. Add or point the repository to a Unity project/template containing `ProjectSettings/ProjectVersion.txt`, `Assets/` and `Packages/`.
2. Add a repository-local headless build entrypoint or script that invokes `BuildPipeline.BuildPlayer` for Windows x64.
3. Run the build to `.llmgc/procedural/alpha-runnable-build/build/windows/` and rerun `run-product-smoke.ps1 -Scenario alpha-runnable-build`.
4. Launch the produced `.exe`, verify content load and the selected loop, then record play evidence in a later bounded task.
