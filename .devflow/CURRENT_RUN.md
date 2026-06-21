# CURRENT_RUN.md

Task id: PRODUCT_SLICE_017_UNITY_ARCHIVE_EXPORT_DRY_RUN
Goal: add deterministic editor-side Unity archive validation/export planning without implementing Unity
Task source: docs/agent-tasks/NEXT_PRODUCT_SLICE/017_UNITY_ARCHIVE_EXPORT_DRY_RUN.md

Source docs/code read:
- AGENTS.md, README.md, docs/CONTEXT_INDEX.md, docs/CURRENT_GENERATOR_STATE.md/json and docs/ROADMAP_TO_FULL_GENERATOR.md
- Product Slice 016 foundation, Unity target/runtime contract spec and Unity archive export dry-run spec
- target Application/test csproj files, Application/Composition target contracts/validator/export analogs, nearby tests and product-smoke runner/docs

Patterns reused:
- immutable Application/Composition records and deterministic diagnostic ordering
- project-local UTF-8 export with resolved-path containment checks
- timestamp-free markdown plus named headless xUnit ProductSmoke routing

Implemented:
- Unity archive dry-run request/result, plan, planned-file, diagnostic, readiness and validation-report models
- validator-backed dry-run service with stable logical archive file planning and safe `.llmgc/unity-export-dry-run/` output
- deterministic JSON/markdown/manifest/validation files with unsafe-path rejection
- readiness for current, warning, future-blocked, missing-requirement and invalid states
- unity-archive-export-dry-run product smoke and docs/state handoff

Non-goals preserved:
- no Unity project/runtime/build and no Runtime, GamePackageDefinition/package schema, WinForms, Scripting, Infrastructure/Generation or generator-library changes
- no solution/project/package changes, provider calls, ComfyUI/Suno integration or generator execution
- M4.1 and controlled-slice gates remain guarded; Product Slice 016 is accepted as the parent

Checks run before state update:
- UnityArchiveExport filtered tests: passed, 4 tests
- ProductSmoke filtered tests: passed, 15 tests
- all fourteen named product smoke scenarios: passed
- unity-archive-export-dry-run: passed, run 20260622_001814-product-smoke

Final guards:
- check-devflow-state.ps1: passed; STOP_REVIEW preserved, 9 tasks, 2 known warnings
- check-all.ps1: passed; build 0 warnings/0 errors, tests 541 passed; run 20260622_002112-check-all
- CURRENT_GENERATOR_STATE.json parse/guard inspection: passed; M4.1 phase/milestone preserved and Product Slice 017 records Product Slice 016 as parent
- mojibake marker scan over all 10 changed files: passed, no markers found
